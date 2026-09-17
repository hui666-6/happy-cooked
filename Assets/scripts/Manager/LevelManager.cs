using UnityEngine;

/// <summary>
/// 关卡管理器：每个关卡场景放一个，通过 levelType 声明当前关卡类型。
/// 目前只有一个场景，作为新手教程关（Tutorial）。
/// 后续新增关卡 = 新建场景 + 一个 LevelManager，设置不同的 levelType 和菜单列表即可。
/// 获取当前关卡类型请使用 LevelManager.Instance.CurrentLevel。
/// 获取当前关卡的唯一标识请使用 LevelManager.Instance.LevelKey。
/// 获取当前关卡的菜单列表请使用 LevelManager.Instance.RecipeList。
/// 选关界面用的 LevelDefinitionSO 也可以在这里配置，优先级高于 LevelSelectPanel 上的配置；如果没有配置，则选关界面会使用 LevelSelectPanel 上的配置。
/// </summary>
public class LevelManager : MonoBehaviour
{
    public enum LevelType
    {
        Tutorial,   // 新手教程关
        Normal      // 普通关卡
    }

    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelType levelType = LevelType.Tutorial;
    [Tooltip("本关卡的唯一标识，用于按关卡存档（如最高分）；留空时自动使用“场景名_关卡类型”")]
    [SerializeField] private string levelId = "";
    [Tooltip("本关卡的配置（选关界面用的那份）；填了以后 LevelKey 和订单菜谱都以它为准")]
    [SerializeField] private LevelDefinitionSO definition;
    [Tooltip("本关卡可生成的菜单列表；每个关卡可配置不同的菜单池")]
    [SerializeField] private recipelistSO recipeList;
     /// <summary>只读属性，当读取这个属性时，直接返回recipeList</summary>
    public recipelistSO RecipeList => recipeList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 LevelManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public LevelType CurrentLevel => levelType;

    /// <summary>本关卡的唯一标识，用于本地存档等需要按关卡区分的数据。</summary>
    public string LevelKey
    {
        get
        {
            if (definition != null) return definition.LevelKey;
            if (!string.IsNullOrWhiteSpace(levelId)) return levelId;
            string sceneName = gameObject.scene.name;
            return string.IsNullOrEmpty(sceneName) ? levelType.ToString() : sceneName + "_" + levelType.ToString();
        }
    }

    /// <summary>按分数换算星级（0~3）；没有配置 definition 时返回 0。</summary>
    public int ComputeStars(int score)
    {
        return definition == null ? 0 : definition.ComputeStars(score);
    }

    public bool IsTutorialLevel()
    {
        return levelType == LevelType.Tutorial;
    }

   public LevelDefinitionSO GetLevelDefinition()
    {
        return definition;
    }
  
    
}
