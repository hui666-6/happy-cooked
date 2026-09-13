using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI recipeTitleText;
    [SerializeField] private TextMeshProUGUI startButtonText;

    [Header("星级")]
    [Tooltip("三颗星对应的 Image，顺序为从左到右")]
    [SerializeField] private Image[] starImages;
    [Tooltip("星星图片；留空则保持预制体里已经填好的图")]
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Color earnedStarColor = new Color(1f, 0.84f, 0.3f, 1f);
    [SerializeField] private Color lockedStarColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("订单菜谱")]
    [SerializeField] private RectTransform recipeContainer;
    [Tooltip("面板里默认关闭的菜谱行模板")]
    [SerializeField] private RectTransform recipeRowTemplate;
    [SerializeField] private float recipeRowHeight = 46f;
    [SerializeField] private float recipeRowGap = 6f;
    [SerializeField] private float recipeIconSpacing = 40f;
    [SerializeField] private int maxRecipeRows = 4;

    [Header("按钮 / 动画")]
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("面板滑入的时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float showDuration = 0.25f;
    [Tooltip("起始位置相对最终位置往右偏多少像素")]
    [SerializeField] private float hiddenOffsetX = 160f;
    [Tooltip("点击开始挑战时的额外回调（例如播放音效）；加载场景由脚本负责")]
    public UnityEvent onStartChallenge;

    private const string RowCloneName = "RecipeRow";
    private const string RowNameChild = "Name";
    private const string RowIconsChild = "Icons";
    private const string IconTemplateChild = "IconTemplate";

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
        if (recipeRowTemplate != null) recipeRowTemplate.gameObject.SetActive(false);

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

        if (recipeTitleText != null) recipeTitleText.text = "本关订单菜谱";
        if (startButtonText != null) startButtonText.text = "开始挑战";

        BuildRecipeRows(level);
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

    private void BuildRecipeRows(LevelDefinitionSO level)
    {
        if (recipeContainer == null || recipeRowTemplate == null) return;

        ClearRecipeRows();

        List<RecipeSO> recipes = level.recipeList != null ? level.recipeList.recipeSOList : null;
        if (recipes == null) return;

        int rowCount = Mathf.Min(recipes.Count, maxRecipeRows);
        for (int i = 0; i < rowCount; i++)
        {
            RecipeSO recipe = recipes[i];
            if (recipe == null) continue;

            RectTransform row = Instantiate(recipeRowTemplate);
            row.name = RowCloneName;
            row.SetParent(recipeContainer, false);
            row.anchoredPosition = new Vector2(recipeRowTemplate.anchoredPosition.x,
                recipeRowTemplate.anchoredPosition.y - i * (recipeRowHeight + recipeRowGap));
            row.gameObject.SetActive(true);

            Transform nameChild = row.Find(RowNameChild);
            TextMeshProUGUI rowNameText = nameChild != null ? nameChild.GetComponent<TextMeshProUGUI>() : null;
            if (rowNameText != null) rowNameText.text = recipe.recipeName;

            BuildRecipeIcons(row, recipe);
        }
    }

    private void BuildRecipeIcons(RectTransform row, RecipeSO recipe)
    {
        RectTransform iconsParent = row.Find(RowIconsChild) as RectTransform;
        if (iconsParent == null) return;

        for (int i = iconsParent.childCount - 1; i >= 0; i--)
        {
            Transform child = iconsParent.GetChild(i);
            if (child.name == RowCloneName) Destroy(child.gameObject);
        }

        RectTransform iconTemplate = iconsParent.Find(IconTemplateChild) as RectTransform;
        if (iconTemplate == null || recipe.kitchenObjectSOList == null) return;

        for (int i = 0; i < recipe.kitchenObjectSOList.Count; i++)
        {
            KitchenObjectSO ingredient = recipe.kitchenObjectSOList[i];
            if (ingredient == null) continue;

            RectTransform icon = Instantiate(iconTemplate);
            icon.name = RowCloneName;
            icon.SetParent(iconsParent, false);
            icon.anchoredPosition = new Vector2(iconTemplate.anchoredPosition.x + i * recipeIconSpacing,
                iconTemplate.anchoredPosition.y);
            icon.gameObject.SetActive(true);

            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null) iconImage.sprite = ingredient.Sprite;
        }
    }

    private void ClearRecipeRows()
    {
        for (int i = recipeContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = recipeContainer.GetChild(i);
            if (child.name == RowCloneName) Destroy(child.gameObject);
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