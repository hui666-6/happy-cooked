using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
            InteracteLogicServerRPC(); 

        }
    }
    [ServerRpc(RequireOwnership =false)]

    private void InteracteLogicServerRPC()//服务器通知所有客户端播放动画
    {   
        InteracteLogicClientRPC ();
    }
   [ClientRpc]
   private void InteracteLogicClientRPC()
    {
        
     ContainerCounterVisual.playOpen();
    }
          
       
    
}
