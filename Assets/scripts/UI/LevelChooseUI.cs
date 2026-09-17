using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 选关界面的水平胶片条。
/// 每个关卡对应一个胶片框（图片取 LevelDefinitionSO.preview），进入界面时聚焦第一关；
/// 点击左右箭头切换关卡，也支持拖动 / 滚轮 / 方向键。
/// 聚焦的关卡会显示在屏幕左侧的大图上，并让右侧的关卡信息面板（预制体）滑入。
/// 脚本挂在带 RectMask2D 的胶片条容器上，该物体就是滑动的视口（viewport）。
/// </summary>
public class LevelChooseUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("引用")]
    [Tooltip("胶片框的父物体；留空时自动在本物体下新建一个 Content")]
    [SerializeField] private RectTransform content;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [Tooltip("屏幕右侧的关卡信息面板预制体")]
    [SerializeField] private LevelSelectPanel panelPrefab;
    [Tooltip("面板挂在哪个父物体下；留空时自动用本物体所在的 Canvas")]
    [SerializeField] private RectTransform panelParent;
    [SerializeField] private Button settingsButton;

    [Header("关卡")]
    [Tooltip("按顺序排列的关卡配置，第 0 个对应第一关")]
    [SerializeField] private LevelDefinitionSO[] levels;

    [Header("胶片条")]
    [Tooltip("胶片框底图")]
    [SerializeField] private Sprite frameSprite;
    [Tooltip("单个胶片框的尺寸")]
    [SerializeField] private Vector2 frameSize = new Vector2(1300f, 271f);
    [Tooltip("关卡图片相对胶片框的内缩，用来避开框边")]
    [SerializeField] private Vector2 levelImagePadding = new Vector2(24f, 24f);
    [Tooltip("相邻两个胶片框的水平间隔")]
    [SerializeField] private float frameSpacing = 40f;

    [Header("滑动")]
    [Tooltip("进入界面时聚焦的关卡下标")]
    [SerializeField] private int startIndex;
    [Tooltip("滑到下一关的动画时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float scrollDuration = 0.35f;
    [Tooltip("拖动结束时的速度超过该值会多翻一关")]
    [SerializeField] private float swipeSpeedThreshold = 900f;
    [Tooltip("拖动时可以超出首尾的距离，松手后弹回")]
    [SerializeField] private float dragOvershoot = 160f;
    [Tooltip("首尾相接循环滑动")]
    [SerializeField] private bool loop;
    [Tooltip("滚轮切换关卡")]
    [SerializeField] private bool mouseWheel = true;
    [Tooltip("键盘左右方向键 / A、D 切换关卡")]
    [SerializeField] private bool keyboard = true;

    [Header("事件")]
    [Tooltip("聚焦的关卡变化时触发，参数为关卡下标")]
    public UnityEvent<int> onLevelFocused;

    private RectTransform rect;
    private float contentY;
    private int index;
    private float dragStartPointerX;
    private float dragStartContentX;
    private float dragSpeed;
    private Coroutine scrollRoutine;
    private LevelSelectPanel panel;

    /// <summary>关卡数量（至少为 1，方便先把空胶片框摆出来看效果）。</summary>
    public int LevelCount => levels != null && levels.Length > 0 ? levels.Length : 1;

    /// <summary>当前聚焦的关卡下标。</summary>
    public int CurrentIndex => index;

    /// <summary>当前聚焦的关卡配置，可能为 null（还没配置关卡时）。</summary>
    public LevelDefinitionSO CurrentLevel => LevelAt(index);

    /// <summary>相邻两个胶片框中心的距离。</summary>
    private float Step => frameSize.x + frameSpacing;

    private float FirstContentX => 0f;

    private float LastContentX => -(LevelCount - 1) * Step;

    private void Awake()
    {
        rect = transform as RectTransform;
        if (rect == null)
        {
            Debug.LogError("LevelChooseUI 需要挂在 UI 物体上（RectTransform）。", this);
            enabled = false;
            return;
        }

        if (content == null)
        {
            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            content = (RectTransform)contentObject.transform;
            content.SetParent(rect, false);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = frameSize;
        }
        contentY = content.anchoredPosition.y;

        BuildFrames();

        if (leftButton != null) leftButton.onClick.AddListener(Previous);
        if (rightButton != null) rightButton.onClick.AddListener(Next);
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OnClickSettingsButton());

        index = loop ? Wrap(startIndex) : Mathf.Clamp(startIndex, 0, LevelCount - 1);
        content.anchoredPosition = new Vector2(-index * Step, contentY);
        RefreshButtons();
        UpdateDetails(index, false);
    }

    private void Start()
    {
        if (onLevelFocused != null) onLevelFocused.Invoke(index);
    }

    private void OnDisable()
    {
        scrollRoutine = null;
        leftButton?.onClick.RemoveListener(Previous);
        rightButton?.onClick.RemoveListener(Next);
        settingsButton?.onClick.RemoveListener(() => OnClickSettingsButton());
    }

    private void Update()
    {
        if (keyboard)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Previous();
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Next();
        }

        if (mouseWheel)
        {
            float wheel = Input.mouseScrollDelta.y;
            if (wheel > 0.01f) Previous();
            else if (wheel < -0.01f) Next();
        }
    }

    /// <summary>聚焦下一关。</summary>
    public void Next()
    {
        ScrollTo(index + 1);
    }

    /// <summary>聚焦上一关。</summary>
    public void Previous()
    {
        ScrollTo(index - 1);
    }

    /// <summary>滑动到指定关卡；越界会被夹在首尾，loop 打开时首尾相接。</summary>
    public void ScrollTo(int levelIndex)
    {
        int next = loop ? Wrap(levelIndex) : Mathf.Clamp(levelIndex, 0, LevelCount - 1);

        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        index = next;
        RefreshButtons();
        UpdateDetails(index, true);
        if (onLevelFocused != null) onLevelFocused.Invoke(index);

        float targetX = -index * Step;
        if (scrollDuration <= 0f || !isActiveAndEnabled)
        {
            content.anchoredPosition = new Vector2(targetX, contentY);
            return;
        }

        scrollRoutine = StartCoroutine(ScrollRoutine(content.anchoredPosition.x, targetX));
    }

    /// <summary>开始挑战当前聚焦的关卡（也可以挂到别的按钮上）。</summary>
    public void StartChallenge()
    {
        if (panel != null)
        {
            panel.StartChallenge();
            return;
        }

        LevelDefinitionSO level = CurrentLevel;
        if (level == null)
        {
            Debug.LogWarning("LevelChooseUI：还没有配置关卡，无法开始挑战", this);
            return;
        }

        Loader.LoadScene(level.sceneName);
    }

    /// <summary>按关卡顺序生成胶片框，并把关卡图片放进对应框里。</summary>
    private void BuildFrames()
    {
        int count = LevelCount;
        for (int i = 0; i < count; i++)
        {
            GameObject frameObject = new GameObject("LevelFrame" + i, typeof(RectTransform), typeof(Image));
            RectTransform frameRect = (RectTransform)frameObject.transform;
            frameRect.SetParent(content, false);
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = frameSize;
            frameRect.anchoredPosition = new Vector2(i * Step, 0f);

            Image frameImage = frameObject.GetComponent<Image>();
            frameImage.sprite = frameSprite;
            frameImage.raycastTarget = true;

            LevelDefinitionSO level = LevelAt(i);
            if (level == null || level.preview == null) continue;

            GameObject levelObject = new GameObject("LevelImage" + i, typeof(RectTransform), typeof(Image));
            RectTransform levelRect = (RectTransform)levelObject.transform;
            levelRect.SetParent(frameRect, false);
            levelRect.anchorMin = Vector2.zero;
            levelRect.anchorMax = Vector2.one;
            levelRect.offsetMin = levelImagePadding;
            levelRect.offsetMax = -levelImagePadding;

            Image levelImage = levelObject.GetComponent<Image>();
            levelImage.sprite = level.preview;
            levelImage.preserveAspect = true;
            levelImage.raycastTarget = false;
        }
    }

    /// <summary>更新右侧信息面板。</summary>
    private void UpdateDetails(int levelIndex, bool animate)
    {
        LevelDefinitionSO level = LevelAt(levelIndex);

        if (panel == null) panel = CreatePanel();
        if (panel != null) panel.Show(level);
    }

    private LevelSelectPanel CreatePanel()
    {
        if (panelPrefab == null) return null;

        RectTransform parent = panelParent;
        if (parent == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            parent = canvas != null ? canvas.transform as RectTransform : rect;
        }

        LevelSelectPanel newPanel = Instantiate(panelPrefab, parent, false);
        newPanel.name = panelPrefab.name;
        return newPanel;
    }

    private LevelDefinitionSO LevelAt(int levelIndex)
    {
        if (levels == null || levelIndex < 0 || levelIndex >= levels.Length) return null;
        return levels[levelIndex];
    }

    private IEnumerator ScrollRoutine(float fromX, float toX)
    {
        float time = 0f;
        while (time < scrollDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / scrollDuration));
            content.anchoredPosition = new Vector2(Mathf.Lerp(fromX, toX, t), contentY);
            yield return null;
        }

        content.anchoredPosition = new Vector2(toX, contentY);
        scrollRoutine = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        dragStartPointerX = eventData.position.x;
        dragStartContentX = content.anchoredPosition.x;
        dragSpeed = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragSpeed = eventData.delta.x / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        float x = dragStartContentX + (eventData.position.x - dragStartPointerX);
        if (!loop) x = Mathf.Clamp(x, LastContentX - dragOvershoot, FirstContentX + dragOvershoot);
        content.anchoredPosition = new Vector2(x, contentY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int target = Mathf.RoundToInt(-content.anchoredPosition.x / Step);
        if (Mathf.Abs(dragSpeed) > swipeSpeedThreshold) target += dragSpeed > 0f ? -1 : 1;

        dragSpeed = 0f;
        ScrollTo(target);
    }

    private void RefreshButtons()
    {
        if (leftButton != null) leftButton.interactable = loop || index > 0;
        if (rightButton != null) rightButton.interactable = loop || index < LevelCount - 1;
    }

    private int Wrap(int levelIndex)
    {
        int count = LevelCount;
        return (levelIndex % count + count) % count;
    }
    private void OnClickSettingsButton()
    {
        if (SettingsUI.instance != null)
        {
            SettingsUI.instance.show();
            if(panel!=null)
            {
                panel.Show(null);
            }
        }
    }
}