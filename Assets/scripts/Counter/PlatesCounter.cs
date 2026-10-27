using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Netcode;
public class PlatesCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO plateso;
    [SerializeField] private float spawnrate=3;
    [SerializeField] private float platecountmax=2;
    private float Timer=0;
    private List<KitchenObject> plateslist=new List<KitchenObject>();


    private void Update()
    {
        
       if(!IsOwner)
        {
            return;
        }
        if (plateslist.Count < platecountmax)
        {
            Timer += Time.deltaTime;
        }
        if (Timer>spawnrate)
        {
            Timer = 0;
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
         SpawnPlate();
    }
     [ServerRpc(RequireOwnership =false)]

    private void InteracteLogicServerRPC()
    {   
        InteracteLogicClientRPC();
    }
   [ClientRpc]
   private void InteracteLogicClientRPC()
    {
         plateslist.RemoveAt(plateslist.Count - 1);
    }

    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject()==false)
        {
            if (plateslist.Count > 0)
            {
                //KitchenObject.CreateKitchenObject()
                InteracteLogicServerRPC();
            }
        }
       
    
}
    public void SpawnPlate()
    {    

        if (plateslist.Count >= platecountmax) 
        {
            Timer = 0;
            return; 
        }
        
            KitchenObject kitchenObject = GameObject.Instantiate(plateso.prefab, GetHoldpoint()).GetComponent<KitchenObject>();
            kitchenObject.transform.localPosition = Vector3.zero + Vector3.up * 0.1f * plateslist.Count;
            plateslist.Add(kitchenObject);
        

    }
}
