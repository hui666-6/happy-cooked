using UnityEngine;

/// <summary>
/// 单个关卡的配置：选关界面用它显示关卡图片、关卡名、历史记录和本关订单菜谱，
/// 点击"开始挑战"时加载 sceneName 对应的场景。
/// 存档用的关卡标识是 LevelKey，必须和运行时 LevelManager 的 LevelKey 一致（两边填同一个 levelId 即可）。
/// </summary>
[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Kitchen/Level Definition")]
public class LevelDefinitionSO : ScriptableObject
{
    [Tooltip("关卡唯一标识，用于存档；留空时用场景名")]
    public string levelId;
    [Tooltip("选关界面显示的关卡名")]
    public string displayName;
    [Tooltip("选关界面的关卡图片：胶片框里的图和左侧大图都用它")]
    public Sprite preview;
    [Tooltip("点击开始挑战要加载的场景名，例如 2-GameScence")]
    public string sceneName;
    [Tooltip("本关会产生的订单菜谱")]
    public recipelistSO recipeList;
    [Tooltip("拿 1 星需要的分数")]
    public int star1Score = 100;
    [Tooltip("拿 2 星需要的分数")]
    public int star2Score = 200;
    [Tooltip("拿 3 星需要的分数")]
    public int star3Score = 300;

    /// <summary>存档用的关卡标识。</summary>
    public string LevelKey
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(levelId)) return levelId;
            if (!string.IsNullOrWhiteSpace(sceneName)) return sceneName;
            return name;
        }
    }

    /// <summary>按分数换算星级（0~3）。</summary>
    public int ComputeStars(int score)
    {
        if (score >= star3Score) return 3;
        if (score >= star2Score) return 2;
        if (score >= star1Score) return 1;
        return 0;
    }
}