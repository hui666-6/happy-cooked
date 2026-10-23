using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class KitchenObject :NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    public IKitchenObjectHolder kitchenObjectparent; 
    
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;    
    }
     public  void SetKitchenobjectParent(IKitchenObjectHolder newKitchenObjectParent)
    {
        if(this.kitchenObjectparent != null)
        {
            this.kitchenObjectparent.ClearKitchenObject();
        }
        this.kitchenObjectparent = newKitchenObjectParent;
        if(newKitchenObjectParent.IsHavekitchenobject())
        {
            Debug.LogError("新父对象已经有一个厨房物体了");
        } 
        kitchenObjectparent.SetKitchenobject(this);
       
    }

    public static void CreateKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectHolder kitchenObjectHolder)
    {
      KitchenGameMultiPlayer.Instance.CreateKitchenObject(kitchenObjectSO, kitchenObjectHolder);
    }
}
