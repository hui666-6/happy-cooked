using System.Collections.Generic;
using UnityEngine;

public class OrderListUI : MonoBehaviour
{
    [SerializeField] private Transform RecipeParent;
    [SerializeField] private RecipeUI RecipeUITemplate;
    private readonly List<RecipeUI> spawned = new List<RecipeUI>();

    private void Start()
    {
        if (OrderManager.Instance == null) return;
        RecipeUITemplate.gameObject.SetActive(false);
        OrderManager.Instance.OnRecipeSpawned += Refresh;
        OrderManager.Instance.OnRecipeSuccessed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (OrderManager.Instance == null) return;
        OrderManager.Instance.OnRecipeSpawned -= Refresh;
        OrderManager.Instance.OnRecipeSuccessed -= Refresh;
    }

    private void Refresh(object sender = null, System.EventArgs e = null) => updateUI();

    private void updateUI()
    {
        foreach (RecipeUI item in spawned)
        {
            if (item == null) continue;
            item.gameObject.SetActive(false);
            Destroy(item.gameObject);
        }
        spawned.Clear();

        foreach (RecipeSO so in OrderManager.Instance.GetOrderList())
        {
            RecipeUI ui = Instantiate(RecipeUITemplate, RecipeParent, false);
            ui.gameObject.SetActive(true);
            ui.UpdateRecipeSo(so);
            spawned.Add(ui);
        }
    }
}

