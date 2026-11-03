using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class FryingRecipeSO :ScriptableObject
{
    public List<FryingRecipe> list;
    public bool TryGetFryingRecipe(KitchenObjectSO input, out FryingRecipe fryingrecipe)
    {
        foreach (FryingRecipe recipe in list)
        {
            if (recipe.input == input)
            {
                fryingrecipe = recipe;
                return true;
            }
        }
        fryingrecipe = null;
        return false;
    }
      public FryingRecipe GetFryingRecipeFromList(int index)
    {
        if(index<0||index>=list.Count)
        {
            return null;
        }
        return list[index];
    }

   public int GetFryingRecipeIndex(FryingRecipe fryingRecipe)
    {
        return list.IndexOf(fryingRecipe);
    }
}

[Serializable] public class FryingRecipe 
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float fryingTime;
} 


