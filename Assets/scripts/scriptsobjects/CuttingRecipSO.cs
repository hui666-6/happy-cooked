using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu()]
public class CuttingRecipListSO : ScriptableObject
{
    public List<CuttingRecipe> cuttingRecipeList;
    public KitchenObjectSO Getoutput(KitchenObjectSO input)
    {

        foreach (CuttingRecipe recipe in cuttingRecipeList)
        {
            if (recipe.input == input)
            {
                return recipe.output;
            }
        }
        return null;
    }
    [Serializable]
    public class CuttingRecipe
    {
        public KitchenObjectSO input;
        public KitchenObjectSO output;
        public int cuttingcountmax;
    }
    public bool TryGetCuttinigRecipe(KitchenObjectSO input,out CuttingRecipe cuttingRecipe)
    {
        foreach (CuttingRecipe recipe in cuttingRecipeList)
        {
            if (recipe.input == input)
            {
                cuttingRecipe= recipe;
                return true;
            }
        }
        cuttingRecipe = null;
        return false;
    }
}
