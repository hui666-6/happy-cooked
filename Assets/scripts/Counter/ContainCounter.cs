using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchen0bjectSO;
    [SerializeField]private ContainerCounterVisual ContainerCounterVisual;

    
    public override void Interact(player player)
    {

        if (!player.IsHavekitchenobject()) 
        {
            KitchenObject.CreateKitchenObject(kitchen0bjectSO, player);
            TransferKitchenObject(this, player);
            ContainerCounterVisual.playOpen();

        }
    }
    
          
       
    
}
