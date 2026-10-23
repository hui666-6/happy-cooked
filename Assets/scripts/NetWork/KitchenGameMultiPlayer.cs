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

    [ServerRpc(RequireOwnership = false)]
     public void CreateKitchenObjectServerRpc(int KitchenObjectSOIndex,NetworkObjectReference kitchenObjectHolderNetWorkReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromList(KitchenObjectSOIndex);
        if (kitchenObjectSO == null)
        {
            return;
        }
        NetworkObject KitchenObjectNetWorkObject =Instantiate(kitchenObjectSO.prefab).GetComponent<NetworkObject>();
        KitchenObjectNetWorkObject.Spawn(true);
        KitchenObject kitchenObject = KitchenObjectNetWorkObject.GetComponent<KitchenObject>();

        kitchenObjectHolderNetWorkReference.TryGet(out NetworkObject kitchenObjectHolderNetWorkObject);
        IKitchenObjectHolder KitchenObjectParent = kitchenObjectHolderNetWorkObject.GetComponent<IKitchenObjectHolder>();
        kitchenObject.SetKitchenobjectParent(KitchenObjectParent);
    }

    public KitchenObjectSO GetKitchenObjectSOFromList(int index)
    {
        return kitchenObjectListSO.kitchenObjectsSOList[index];
    }
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectsSOList.IndexOf(kitchenObjectSO);
    }
}
