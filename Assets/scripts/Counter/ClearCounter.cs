using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    
    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject())
        {
            if (player.GetKitchenObject().TryGetComponent<platekitchenobject>(out platekitchenobject platekitchenobject))//������
            {
                if (IsHavekitchenobject() == false)
                {
                    TransferKitchenObject(player, this); //�����ӷ���������
                }
                else 
                {
                 bool Success = platekitchenobject.AddKitchenobjectSO(GetKitchenObjectSO());
                 if (Success)
                 {
                   Destorykitchenobject();
                 }
                }

            }
            else 
            {
                if (IsHavekitchenobject() == false) //��̨Ϊ��
                {
                    TransferKitchenObject(player, this);
                }
                else
                {
                    if (GetKitchenObject().TryGetComponent<platekitchenobject>(out platekitchenobject))
                    {
                        if (platekitchenobject.AddKitchenobjectSO(player.GetKitchenObjectSO())) 
                        {
                            player.Destorykitchenobject();
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
