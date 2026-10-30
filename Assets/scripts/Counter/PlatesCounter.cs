using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
public class PlatesCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO plateSO;
    [SerializeField] private float spawnrate=3f;
    [SerializeField] private int  platecountmax=2;

    private int plateAmount=0;
    private float timer=0;
   
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateCounterInteract;

    private void Update()
    {
        
       if(!IsOwner)
        {
            return;
        }
        if (plateAmount< platecountmax)
        {
            timer += Time.deltaTime;
        }
        if (timer>spawnrate)
        {
            timer = 0;
            SpawnPlateServerRPC();
        }
    }
     [ServerRpc(RequireOwnership =false)]
    private void SpawnPlateServerRPC()
    {
        SpawnPlateClientRPC();
     }
    [ClientRpc]
       private void SpawnPlateClientRPC()
       {
         OnPlateSpawned?.Invoke(this,EventArgs.Empty);
         plateAmount++;   
       }
     [ServerRpc(RequireOwnership =false)]

    private void InteracteLogicServerRPC()
     {   
         InteracteLogicClientRPC();
     }
    [ClientRpc]
    private void InteracteLogicClientRPC()
    {
      OnPlateCounterInteract?.Invoke(this,EventArgs.Empty);
      plateAmount--;
    }

    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject()==false)
        {
            if (plateAmount > 0)
            {
                KitchenObject.CreateKitchenObject(plateSO,player);
                InteracteLogicServerRPC();
            }
        }
       
    
}
   
}
