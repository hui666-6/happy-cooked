using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitchenGameMultiPlayer : NetworkBehaviour
{
    public static KitchenGameMultiPlayer Instance { get; private set; }
    public KitchenObjectListSO kitchenObjectListSO;
    private void Awake()
    {
        Instance = this;
    }

    public void CreateKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectHolder kitchenObjectHolder)
    {
        CreateKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectHolder.GetNetWorkObject());
    }

    [ServerRpc(RequireOwnership = false)] //标记成SeverRPC客户端调用它时，代码不会在客户端执行，而是发送消息给服务器执行  如果owner为true，就只有服务器自己能调用，客户端一按就被拒
     public void CreateKitchenObjectServerRpc(int KitchenObjectSOIndex,NetworkObjectReference kitchenObjectHolderNetWorkReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromList(KitchenObjectSOIndex);
        if (kitchenObjectSO == null)
        {
            return;
        }
        NetworkObject KitchenObjectNetWorkObject =Instantiate(kitchenObjectSO.prefab).GetComponent<NetworkObject>();
        KitchenObjectNetWorkObject.Spawn(true);//在服务器上实例化预制体，把这个对象存在的消息发给所有客户端，客户端会自动创建一份副本 参数true是destroyWithScence.场景卸载时一并销毁
        KitchenObject kitchenObject = KitchenObjectNetWorkObject.GetComponent<KitchenObject>();

        kitchenObjectHolderNetWorkReference.TryGet(out NetworkObject kitchenObjectHolderNetWorkObject);
        IKitchenObjectHolder KitchenObjectParent = kitchenObjectHolderNetWorkObject.GetComponent<IKitchenObjectHolder>();
        kitchenObject.SetKitchenobjectParent(KitchenObjectParent);//设置该对象的归属 广播该对象归谁
    }

    public KitchenObjectSO GetKitchenObjectSOFromList(int index)
    {
        return kitchenObjectListSO.kitchenObjectsSOList[index];
    }
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectsSOList.IndexOf(kitchenObjectSO);
    }
    public void DestoryKitchenObject(KitchenObject kitchenObject)
    {
        DestoryKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }
    [ServerRpc(RequireOwnership = false)] //标记成SeverRPC客户端调用它时，代码不会在客户端执行，而是发送消息给服务器执行  如果owner为true，就只有服务器自己能调用，客户端一按就被拒
     public void DestoryKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetWorkReference)
    {
        kitchenObjectNetWorkReference.TryGet(out NetworkObject kitchenObjectNetWorkObject);
        KitchenObject kitchenObject = kitchenObjectNetWorkObject.GetComponent<KitchenObject>();
         if (kitchenObject==null)
         return ;
        kitchenObject.Destorykitchenobject();
        ClearKitchenObjectOnParentClientRpc(kitchenObject.NetworkObject);
    }
  
    [ClientRpc]
    public void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference  kitchenObjectNetWorkReference)
    {   
        kitchenObjectNetWorkReference.TryGet(out NetworkObject kitchenObjectNetWorkObject);
        KitchenObject KitchenObject= kitchenObjectNetWorkObject.GetComponent<KitchenObject>();
        KitchenObject.ClearKitchenObjectOnParent();
        
    }
}
