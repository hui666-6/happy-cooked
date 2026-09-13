using UnityEngine;

/// <summary>
/// 关卡管理器：每个关卡场景放一个，通过 levelType 声明当前关卡类型。
/// 目前只有一个场景，作为新手教程关（Tutorial）。
/// 后续新增关卡 = 新建场景 + 一个 LevelManager，设置不同的 levelType 和菜单列表即可。
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

    private void Awake()
    {
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
            return string.IsNullOrEmpty(sceneName) ? levelType.ToString() : sceneName + "_" + levelType;
        }
    }

    /// <summary>按分数换算星级（0~3）；没有配置 definition 时返回 0。</summary>
    public int ComputeStars(int score)
    {
        return definition != null ? definition.ComputeStars(score) : 0;
    }

    public bool IsTutorialLevel()
    {
        return levelType == LevelType.Tutorial;
    }

    /// <summary>本关卡的菜单列表（可能为空，调用方需自行兜底）。</summary>
    public recipelistSO RecipeList => recipeList != null ? recipeList : (definition != null ? definition.recipeList : null);
}
