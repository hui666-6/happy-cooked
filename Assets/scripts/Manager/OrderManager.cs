using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeSuccessed;
    public event EventHandler OnRecipeFailed;
    [SerializeField] private recipelistSO recipesolist;
    [SerializeField] private float orderRate = 2;
    [SerializeField] private int orderMax = 5;
    private List<RecipeSO> orderRecipeSOList = new List<RecipeSO>();
    private float orderTimer = 0;
    private bool isStartOrder = false;
    private bool waitForTutorialFinish = false;
    private int orderCount = 0;
    private int successDeliveryCount = 0;
    private void Start()
    {
        GameManager.Instance.onchangstate += GameManager_onchangstate;

        // 每个关卡的菜单列表不同：优先使用当前关卡（LevelManager）配置的菜单池，
        // 未配置时回退到本组件上序列化的默认菜单，保证没有 LevelManager 的场景也能运行。
        if (LevelManager.Instance != null && LevelManager.Instance.RecipeList != null)
        {
            recipesolist = LevelManager.Instance.RecipeList;
        }

        TutorialGuideUI tutorialGuide = TutorialGuideUI.Instance;
        if (tutorialGuide != null && tutorialGuide.BlocksOrderGeneration)
        {
            waitForTutorialFinish = true;
            tutorialGuide.OnTutorialFinished += TutorialGuideUI_OnTutorialFinished;
        }
    }

    private void GameManager_onchangstate(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlayingState() && !waitForTutorialFinish)
        { 
          startsqawnorder();
        }
    }

    private void TutorialGuideUI_OnTutorialFinished(object sender, EventArgs e)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePlayingState())
        {
            startsqawnorder();
        }
    }

    private void Update()
    {
        if (isStartOrder)
        {
            OrderUpdate();
        }
    }
    public void Awake()
    {
        Instance = this;
    }
    private void OrderUpdate()
    {
        orderTimer += Time.deltaTime;
        if (orderTimer >=orderRate)
        {
            orderTimer = 0;
            OrderNewRecipe();

        }

    }

    private void OrderNewRecipe()
    {
        if (orderCount >= orderMax) return;
        // 菜单池为空则不生成，避免空列表随机取值报错
        if (recipesolist == null || recipesolist.recipeSOList == null || recipesolist.recipeSOList.Count == 0) return;
        orderCount++;
        int index = UnityEngine.Random.Range(0, recipesolist.recipeSOList.Count);
        orderRecipeSOList.Add(recipesolist.recipeSOList[index]);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }
    public void DeliveryRecipe(platekitchenobject platekitchenobject)
    {
        RecipeSO correctrecipe = null;
        foreach (RecipeSO recipe in orderRecipeSOList)
        {
            if (IsCorrect(recipe, platekitchenobject))
            { 
             correctrecipe = recipe;
                break;
            }
        }
        if (correctrecipe == null)
        {
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
            print("�ϲ�ʧ��");
        }
        else
        { 
        orderRecipeSOList.Remove(correctrecipe);
        OnRecipeSuccessed?.Invoke(this, EventArgs.Empty);
            print("�ϲ˳ɹ�");
            successDeliveryCount++;
        }

    }
    private bool IsCorrect(RecipeSO recipe,platekitchenobject platekitchenobject)
    { 
        List<KitchenObjectSO> list1 = recipe.kitchenObjectSOList;
        List<KitchenObjectSO>list2 =platekitchenobject.GetkitchenObjectList();
        if (list1.Count != list2.Count) return false;
        List<KitchenObjectSO> remaining = new List<KitchenObjectSO>(list2);
        foreach (KitchenObjectSO kitchenObjectSO in list1)
        {
            if (remaining.Remove(kitchenObjectSO) == false)
            {
                return false;
            }
        }
        return true;
    
    }
    public List<RecipeSO> GetOrderList()
    {
        return orderRecipeSOList;
    }

    private void startsqawnorder()
    { 
      isStartOrder = true;
    }

    public int GetsuccessDeliverCount()
    { 
      return successDeliveryCount;
    }
}
