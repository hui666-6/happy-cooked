using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter :BaseCounter
{
    public static event EventHandler OnObjectTransh;
    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject())
        { 
           KitchenObject.DestoryKitchenObject(player.GetKitchenObject());
           DestroyKitchenObjectServerRpc();
        }
    }
   [ServerRpc(RequireOwnership = false)]
    public void DestroyKitchenObjectServerRpc()
    {
        DestroyKitchenObjectClientRpc();
    }
    [ClientRpc]
    public void DestroyKitchenObjectClientRpc()
    {
        OnObjectTransh?.Invoke(this, EventArgs.Empty);//通知SoundManager播放音效
    }


  public static void ClearStaticdata()
    {
        OnObjectTransh=null;

    }
}
