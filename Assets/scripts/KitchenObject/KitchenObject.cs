using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class KitchenObject :NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    public IKitchenObjectHolder kitchenObjectparent; 

    public FollowTransform followTransform;


    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    public void Destorykitchenobject()
    {
        Destroy(gameObject);
    }
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;    
    }

    /// <summary>
    /// 设置跟随目标。食材带 NetworkObject，不能直接 SetParent 到没有 NetworkObject 的
    /// Holdpoint 上（NGO 会抛 InvalidParentException），位置表现统一交给 FollowTransform。
    /// </summary>
    public void SetFollowTarget(Transform target, Vector3 offset = default)
    {
        if (followTransform == null)
        {
            followTransform = GetComponent<FollowTransform>();
        }

        if (followTransform == null)
        {
            followTransform = gameObject.AddComponent<FollowTransform>();
        }

        followTransform.SetTargetTransform(target, offset);
    }
     public  void SetKitchenobjectParent(IKitchenObjectHolder newKitchenObjectParent)
    {
        NetworkObjectReference holderReference = newKitchenObjectParent.GetNetWorkObject();

        // 服务器（含 Host）直接广播，客户端才需要走 ServerRpc 请求
        if (IsServer)
        {
            SetKitchenObjectTransformClientRpc(holderReference);
        }
        else
        {
            SetKitchenObjectTransformServerRpc(holderReference);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectTransformServerRpc(NetworkObjectReference kitchenObjectHolderNetWorkReference )
    {
        SetKitchenObjectTransformClientRpc(kitchenObjectHolderNetWorkReference);
    }
    [ClientRpc]
    private  void SetKitchenObjectTransformClientRpc(NetworkObjectReference  kitchenObjectHolderNetWorkReference)
    {
        // 先解析新持有者，解析失败就不要动旧状态，避免食材凭空失去归属
        if (!kitchenObjectHolderNetWorkReference.TryGet(out NetworkObject kitchenObjectHolderNetWorkObject))
        {
            Debug.LogWarning("KitchenObject：解析持有者失败（持有者可能没有 NetworkObject）。");
            return;
        }

        IKitchenObjectHolder KitchenObjectParent = kitchenObjectHolderNetWorkObject.GetComponent<IKitchenObjectHolder>();
        if (KitchenObjectParent == null)
        {
            Debug.LogWarning("KitchenObject：持有者身上没有 IKitchenObjectHolder 实现。");
            return;
        }

        if (this.kitchenObjectparent != null && !ReferenceEquals(this.kitchenObjectparent, KitchenObjectParent))
        {
            this.kitchenObjectparent.ClearKitchenObject();
        }

        this.kitchenObjectparent = KitchenObjectParent;
        if( KitchenObjectParent.IsHavekitchenobject())
        {
            Debug.LogError("新父对象已经有一个厨房物体了");
            return;
        } 
        kitchenObjectparent.SetKitchenobject(this); //替换对象身上的物体
        SetFollowTarget(kitchenObjectparent.GetHoldpoint());
    }

    public static void CreateKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectHolder kitchenObjectHolder)
    {
      KitchenGameMultiPlayer.Instance.CreateKitchenObject(kitchenObjectSO, kitchenObjectHolder);
    }
    public static void DestoryKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiPlayer.Instance.DestoryKitchenObject(kitchenObject);
    }
 
    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectparent.ClearKitchenObject();
    }
}

