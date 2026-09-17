using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个关卡的配置：选关界面用它显示关卡图片、关卡名、历史记录以及本关会出现的食材图标，
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
    [Tooltip("下一关的配置；点击结算面板的“下一关”时会加载它的场景。留空表示这是最后一关")]
    public LevelDefinitionSO nextLevel;
    [Tooltip("本关会出现的食材图片列表，可在每个关卡配置表中直接拖入图片")]
    public List<Sprite> ingredientSprites = new List<Sprite>();
    [Tooltip("拿 1 星需要的分数")]
    public int star1Score =0;
    [Tooltip("拿 2 星需要的分数")]
    public int star2Score =0;
    [Tooltip("拿 3 星需要的分数")]
    public int star3Score =0;

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