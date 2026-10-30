using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{

    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject() &&
             player.GetKitchenObject().TryGetComponent<platekitchenobject>(out platekitchenobject platekitchenobject))
        {
            OrderManager.Instance.DeliveryRecipe(platekitchenobject);
            KitchenObject.DestoryKitchenObject(player.GetKitchenObject());
                
        }
    }
}
