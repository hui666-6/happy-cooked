
using UnityEngine;

public class ClearCounter : BaseCounter
{

    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject())
        {
            if (player.GetKitchenObject().TryGetComponent<platekitchenobject>(out platekitchenobject platekitchenobject))
            {
                if (IsHavekitchenobject() == false)
                {
                    TransferKitchenObject(player, this);
                }
                else
                {
                    bool Success = platekitchenobject.AddKitchenobjectSO(GetKitchenObjectSO()); //食材是否可以加入盘子中
                    if (Success)
                    {
                        Debug.Log(GetKitchenObject());
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());

                    }
                }

            }
            else
            {
                if (IsHavekitchenobject() == false) //玩家手上的不是盘子
                {
                    TransferKitchenObject(player, this);
                }
                else
                {
                    if (GetKitchenObject().TryGetComponent<platekitchenobject>(out platekitchenobject))
                    {
                        if (platekitchenobject.AddKitchenobjectSO(player.GetKitchenObjectSO()))
                        {
                            KitchenObject.DestoryKitchenObject(player.GetKitchenObject());
                        }
                    }
                }

            }
        }

        else
        {

            if (IsHavekitchenobject() == false) //��̨Ϊ��
            {

            }
            else
            {
                TransferKitchenObject(this, player);
            }


        }

    }


}
