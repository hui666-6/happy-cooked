using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingRecipListSO;

public class CuttingCounter : BaseCounter
{
    [SerializeField] public CuttingRecipListSO cuttingRecipList;
    [SerializeField] private ProgressBarUI progressBarUI;
    [SerializeField] private CuttingCounterVisual CuttingCounterVisual;
                     private int cuttingcount;
    public static event EventHandler OnChop;
    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject()) //判断玩家手上是否有食材
        {
            if (IsHavekitchenobject() == false)//玩家身上有食材，柜台上没有食材
            {
                TransferKitchenObject(player, this);
            }
      
        }
        else
        {
            if (IsHavekitchenobject() == false)  return;
            else
            {
                TransferKitchenObject(this, player);//柜台上有食材
            }

        }

    }
    public override void InteractOperate(player player)
    {
        if (IsHavekitchenobject())
        {
          InteractOperateServerRpc();
        }
    }
   private void Cut()
   {
     OnChop?.Invoke(this, EventArgs.Empty);
     CuttingCounterVisual.playCut();
   }

    public static void clearStaticData()
    { 
     OnChop = null;
    }

    [ServerRpc(RequireOwnership =false)]
    public void InteractOperateServerRpc()
    {
        if (!IsHavekitchenobject())
            return;
        if(cuttingRecipList.TryGetCuttinigRecipe(GetKitchenObject().GetKitchenObjectSO(), out CuttingRecipe cuttingRecipe))
        {
            cuttingcount++;
            float progress = ((float)cuttingcount / cuttingRecipe.cuttingcountmax);
            InteractOperateClientRpc(GetCuttingRecipeSOIndex(cuttingRecipe),progress);
            if (cuttingcount >= cuttingRecipe.cuttingcountmax)
            { 
            KitchenObject.DestoryKitchenObject(GetKitchenObject());
            KitchenObject.CreateKitchenObject(cuttingRecipe.output,this);
            cuttingcount = 0;
            }
        }
    }
    [ClientRpc]//客户端只处理外部表现不参与计算
    public void  InteractOperateClientRpc(int cuttingRecipeSOIndex,float progress)
    {           
         Cut();
         CuttingRecipe cuttingRecipeSO =GetCuttingRecipeSOFromList(cuttingRecipeSOIndex);
         progressBarUI.UpdateProgress(progress);
                  
    }
  
     public int GetCuttingRecipeSOIndex(CuttingRecipe cuttingRecipe)
    {
        return cuttingRecipList.cuttingRecipeList.IndexOf(cuttingRecipe);
    }
     public CuttingRecipe GetCuttingRecipeSOFromList(int index)
    {
        return cuttingRecipList.cuttingRecipeList [index];
    }
}
