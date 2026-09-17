using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 选关界面右侧的关卡信息面板（预制体）。
/// 显示关卡名、星级、历史最高分和本关会出现的订单菜谱，并提供"开始挑战"按钮。
/// 由 LevelChooseUI 在聚焦的关卡变化时调用 Show()，面板会从右边滑入。
/// </summary>
public class LevelSelectPanel : MonoBehaviour
{
    [Header("文本")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI startButtonText;

    [Header("星级")]
    [Tooltip("三颗星对应的 Image，顺序为从左到右")]
    [SerializeField] private Image[] starImages;
    [Tooltip("星星图片；留空则保持预制体里已经填好的图")]
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Color earnedStarColor ;
    [SerializeField] private Color lockedStarColor;

    [Header("食材图片")]
    [Tooltip("当前关卡展示的食材图标，数量可按需要配置；会按顺序显示")]
    [SerializeField] private Image[] ingredientImages;

    [Header("按钮 / 动画")]
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("面板滑入的时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float showDuration = 0.25f;
    [Tooltip("起始位置相对最终位置往右偏多少像素")]
    [SerializeField] private float hiddenOffsetX = 160f;
    [Tooltip("点击开始挑战时的额外回调（例如播放音效）；加载场景由脚本负责")]
    public UnityEvent onStartChallenge;

    private RectTransform rect;
    private LevelDefinitionSO currentLevel;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private float animationTime;
    private bool animating;

    /// <summary>当前显示的关卡。</summary>
    public LevelDefinitionSO CurrentLevel => currentLevel;

    private void Awake()
    {
        rect = transform as RectTransform;
        if (rect != null)
        {
            shownPosition = rect.anchoredPosition;
            hiddenPosition = shownPosition + new Vector2(hiddenOffsetX, 0f);
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (startButton != null) startButton.onClick.AddListener(StartChallenge);

        SetVisible(false, false);
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartChallenge);
    }

    private void Update()
    {
        if (!animating || rect == null) return;

        animationTime += Time.unscaledDeltaTime;
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(animationTime / Mathf.Max(showDuration, 0.01f)));
        rect.anchoredPosition = Vector2.Lerp(hiddenPosition, shownPosition, t);
        if (canvasGroup != null) canvasGroup.alpha = t;
        if (t >= 1f) animating = false;
    }

    /// <summary>显示某个关卡的信息；传 null 表示收起面板。</summary>
    public void Show(LevelDefinitionSO level)
    {
        currentLevel = level;
        if (level == null)
        {
            SetVisible(false, true);
            return;
        }

        string levelKey = level.LevelKey;
        if (levelNameText != null)
        {
            levelNameText.text = string.IsNullOrWhiteSpace(level.displayName) ? level.name : level.displayName;
        }

        if (bestScoreText != null)
        {
            int highScore = PlayerProgress.GetHighScore(levelKey);
            bestScoreText.text = highScore > 0 ? "历史最高分  " + highScore : "历史最高分  暂无记录";
        }

        UpdateStars(PlayerProgress.GetStars(levelKey));
        UpdateIngredientImages(level);

        if (startButtonText != null) startButtonText.text = "开始游戏";

        SetVisible(true, true);
    }

    /// <summary>开始挑战当前显示的关卡。</summary>
    public void StartChallenge()
    {
        if (currentLevel == null) return;

        if (onStartChallenge != null) onStartChallenge.Invoke();

        if (string.IsNullOrWhiteSpace(currentLevel.sceneName))
        {
            Debug.LogError("LevelSelectPanel：关卡 " + currentLevel.name + " 没有填 sceneName，无法加载场景", currentLevel);
            return;
        }

        Loader.LoadScene(currentLevel.sceneName);
    }

    private void UpdateStars(int stars)
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            Image star = starImages[i];
            if (star == null) continue;
            if (starSprite != null) star.sprite = starSprite;
            star.color = i < stars ? earnedStarColor : lockedStarColor;
        }
    }

    private void UpdateIngredientImages(LevelDefinitionSO level)
    {
        if (ingredientImages == null) return;

        for (int i = 0; i < ingredientImages.Length; i++)
        {
            Image image = ingredientImages[i];
            if (image == null) continue;

            image.enabled = false;
            image.sprite = null;
        }

        if (level == null || level.ingredientSprites == null) return;

        for (int i = 0; i < ingredientImages.Length && i < level.ingredientSprites.Count; i++)
        {
            Image image = ingredientImages[i];
            if (image == null) continue;

            image.sprite = level.ingredientSprites[i];
            image.enabled = image.sprite != null;
        }
    }

    private void SetVisible(bool visible, bool animate)
    {
        animating = false;
        animationTime = 0f;

        if (rect != null) rect.anchoredPosition = visible && !animate ? shownPosition : hiddenPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible && !animate ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        if (visible && animate) animating = true;
    }
}