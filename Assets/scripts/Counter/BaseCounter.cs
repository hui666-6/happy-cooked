using System;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectHolder
{
    [SerializeField] private GameObject selectedcounter;
    [SerializeField] private Transform Holdpoint;

    private KitchenObject kitchenObject;

    public static event EventHandler ondrop;

    // 被玩家选中/取消选中时触发，供各类柜台的视觉表现（如按键提示）订阅
    public event EventHandler OnSelected;
    public event EventHandler OnDeselected;

    public virtual void Interact(player player)
    {
        Debug.LogWarning("该柜台没有实现 Interact。");
    }

    public virtual void InteractOperate(player player)
    {
    }

    public bool IsHavekitchenobject()
    {
        return kitchenObject != null;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObject != null ? kitchenObject.GetKitchenObjectSO() : null;
    }

    public void SetKitchenobject(KitchenObject newKitchenObject)
    {
        if (kitchenObject != newKitchenObject && newKitchenObject != null)
        {
            ondrop?.Invoke(this, EventArgs.Empty);
        }

        kitchenObject = newKitchenObject;

        if (kitchenObject != null)
        {
            kitchenObject.transform.localPosition = Vector3.zero;
        }
    }

    public Transform GetHoldpoint()
    {
        return Holdpoint;
    }

    public void TransferKitchenObject(IKitchenObjectHolder sourceHolder, IKitchenObjectHolder targetHolder)
    {
        if (sourceHolder == null || targetHolder == null)
        {
            Debug.LogWarning("物品转移失败：来源或目标持有者为空。");
            return;
        }

        KitchenObject sourceKitchenObject = sourceHolder.GetKitchenObject();
        if (sourceKitchenObject == null)
        {
            Debug.LogWarning("物品转移失败：来源没有食材。");
            return;
        }

        if (targetHolder.IsHavekitchenobject())
        {
            Debug.LogWarning("物品转移失败：目标已有食材。");
            return;
        }

        // 走 KitchenObject 的同步链路：ServerRpc 决定归属 → ClientRpc 在所有端更新
        // 持有者状态和 FollowTransform 目标，不再各端各改本地状态
        sourceKitchenObject.SetKitchenobjectParent(targetHolder);
    }

    public void AddKitchenObject(KitchenObject newKitchenObject)
    {
        if (newKitchenObject == null)
        {
            return;
        }

        // 食材是带 NetworkObject 的网络对象，不能 SetParent 到没有 NetworkObject 的 Holdpoint
        // （NGO 会抛 InvalidParentException 并把父级回滚），位置改由 FollowTransform 驱动
        newKitchenObject.SetFollowTarget(Holdpoint);
        SetKitchenobject(newKitchenObject);
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public static void ClearStaticData()
    {
        ondrop = null;
    }

    public void SelectCounter()
    {
        selectedcounter.SetActive(true);
        OnSelected?.Invoke(this, EventArgs.Empty);
    }

    public void CancelSelect()
    {
        selectedcounter.SetActive(false);
        OnDeselected?.Invoke(this, EventArgs.Empty);
    }

    public NetworkObject GetNetWorkObject()
    {
        return NetworkObject;
    }
}
