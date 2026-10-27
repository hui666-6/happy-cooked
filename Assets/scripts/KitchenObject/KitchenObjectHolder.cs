using UnityEngine;
using Unity.Netcode;
/// <summary>
/// 表示能够持有一个 KitchenObject 的对象，例如玩家或柜台。
/// 该接口只定义持有物品的共同能力；网络同步由具体的 NetworkBehaviour 实现。
/// </summary>
public interface IKitchenObjectHolder
{
    bool IsHavekitchenobject();
    KitchenObject GetKitchenObject();
    KitchenObjectSO GetKitchenObjectSO();
    void SetKitchenobject(KitchenObject kitchenObject);
    Transform GetHoldpoint();
    void AddKitchenObject(KitchenObject kitchenObject);
    void ClearKitchenObject();
    void Destorykitchenobject();
    void creatkitchenobject(GameObject kitchenobjectprefab);
    NetworkObject GetNetWorkObject();
}
