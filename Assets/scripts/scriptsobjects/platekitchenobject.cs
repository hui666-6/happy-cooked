using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platekitchenobject :KitchenObject
{
    // 任意餐盘成功加入一个食材时触发（供新手教程等监听）。args 携带被加入的食材类型。
    public static event EventHandler<OnIngredientAddedEventArgs> OnAnyIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> useblekitchenobject;
    [SerializeField] private PlateCompleteVisual PlateCompleteVisual;
    [SerializeField] private KitchenObjectUI KitchenObjectUI;
    private List<KitchenObjectSO> kitchenObjectSOlist = new List<KitchenObjectSO>();

    public bool AddKitchenobjectSO(KitchenObjectSO kitchenobjectSO)
    {
        if (kitchenObjectSOlist.Contains(kitchenobjectSO)) 
        {

            return false;
        }
        if (useblekitchenobject.Contains(kitchenobjectSO) == false)
        { 
          return false;
        }
        PlateCompleteVisual.ShowKitchenObject(kitchenobjectSO);
        KitchenObjectUI.showkitchenobjectUI(kitchenobjectSO);
        kitchenObjectSOlist.Add(kitchenobjectSO);
        OnAnyIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs { kitchenObjectSO = kitchenobjectSO });
        return true;

    }

    // 场景重载时清理静态事件，避免残留旧订阅导致的空引用/重复触发。
    public static void ClearStaticData()
    {
        OnAnyIngredientAdded = null;
    }

    public List<KitchenObjectSO> GetkitchenObjectList()
    { 
    
     return kitchenObjectSOlist;
    }

}
