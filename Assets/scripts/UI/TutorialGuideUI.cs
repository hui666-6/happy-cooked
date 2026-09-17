using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 分步新手指导指示框。
/// 在 Inspector 里配置一组步骤，每步包含【任务标题】【任务内容】和【完成条件】。
/// 玩家完成当前步骤后，框里的两个文本会自动切换到下一步。
/// </summary>
public class TutorialGuideUI : MonoBehaviour
{
    public static TutorialGuideUI Instance { get; private set; }

    /// <summary>一个步骤的完成条件类型。</summary>
    public enum TutorialTask
    {
        PickUpObject,        // 拿起物体（可指定具体物体，留空=任意物体）
        PutDownObject,       // 把物体放到空柜台上
        AddIngredientToPlate,// 把食材加到餐盘上（可指定具体食材，留空=任意食材）
        ChopSomething,       // 切了一次菜
        DeliverRecipe,       // 成功交付一个订单
        Custom               // 手动完成：由外部代码调用 CompleteCurrentStep()
    }

    [Serializable]
    public class TutorialStep
    {
        public string title;              // 任务标题
        [TextArea] public string content; // 任务内容
        public TutorialTask task;         // 完成条件
        [Tooltip("PickUpObject / AddIngredientToPlate 使用：指定的物体或食材（如盘子、面包片）。留空表示任意皆可。")]
        public KitchenObjectSO requiredObject;
    }

    [Header("UI 引用")]
    [SerializeField] private GameObject panel;            // 整个指示框（用于显示/隐藏）
    [SerializeField] private TextMeshProUGUI titleText;   // 任务标题文本
    [SerializeField] private TextMeshProUGUI contentText; // 任务内容文本

    [Header("教程步骤（按顺序执行）")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("开始时机")]
    [Tooltip("勾选后，教程会在游戏进入 gameplaying 状态时才开始；否则脚本一启用就开始。")]
    [SerializeField] private bool startOnGamePlaying = true;

    [Header("全部完成后的表现")]
    [SerializeField] private bool hideWhenFinished = true;
    [SerializeField] private string finishedTitle = "教程完成";
    [SerializeField, TextArea] private string finishedContent = "开始你的表演吧！";

    private int currentIndex = 0;
    private bool started = false;

    /// <summary>True while this tutorial should block order spawning (started on playing state and has steps).</summary>
    public bool BlocksOrderGeneration => startOnGamePlaying && steps != null && steps.Count > 0;
    private bool eventTaskDone = false; // 记录“一次性动作类”步骤是否已触发（放下/切菜/交付）

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 TutorialGuideUI，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 订阅一次性动作事件（这些动作发生在某一帧，靠事件捕捉最准确）
        KitchenObjectHolder.ondrop += OnDrop;
        CuttingCounter.onchop += OnChop;
        platekitchenobject.OnAnyIngredientAdded += OnIngredientAdded;
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnRecipeSuccessed += OnRecipeSuccessed;
        }

        if (panel != null) panel.SetActive(false);

        if (startOnGamePlaying && GameManager.Instance != null)
        {
            GameManager.Instance.onchangstate += OnGameStateChanged;
            // 如果订阅时已经在游戏进行中，直接开始
            if (GameManager.Instance.IsGamePlayingState()) BeginTutorial();
        }
        else
        {
            BeginTutorial();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        KitchenObjectHolder.ondrop -= OnDrop;
        CuttingCounter.onchop -= OnChop;
        platekitchenobject.OnAnyIngredientAdded -= OnIngredientAdded;
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnRecipeSuccessed -= OnRecipeSuccessed;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onchangstate -= OnGameStateChanged;
        }
    }

    private void Update()
    {
        if (!started) return;
        if (currentIndex >= steps.Count) return;

        if (IsCurrentStepComplete())
        {
            GoToNextStep();
        }
    }

    /// <summary>判断当前步骤是否已完成。</summary>
    private bool IsCurrentStepComplete()
    {
        TutorialStep step = steps[currentIndex];
        switch (step.task)
        {
            case TutorialTask.PickUpObject:
                // 状态类条件：直接看玩家手上拿了什么，最稳，不受事件时机影响
                if (player.Instance == null || !player.Instance.IsHavekitchenobject())
                    return false;
                if (step.requiredObject == null)
                    return true; // 任意物体
                return player.Instance.GetKitchenObjectSO() == step.requiredObject;

            case TutorialTask.PutDownObject:
            case TutorialTask.AddIngredientToPlate:
            case TutorialTask.ChopSomething:
            case TutorialTask.DeliverRecipe:
                // 动作类条件：由事件回调置位
                return eventTaskDone;

            case TutorialTask.Custom:
            default:
                return false; // 等待外部调用 CompleteCurrentStep()
        }
    }

    // ---------- 事件回调：只在“当前步骤正好等待该动作”时才置位 ----------

    private void OnDrop(object sender, EventArgs e)
    {
        if (IsWaitingFor(TutorialTask.PutDownObject)) eventTaskDone = true;
    }

    private void OnChop(object sender, EventArgs e)
    {
        if (IsWaitingFor(TutorialTask.ChopSomething)) eventTaskDone = true;
    }

    private void OnIngredientAdded(object sender, platekitchenobject.OnIngredientAddedEventArgs e)
    {
        if (!IsWaitingFor(TutorialTask.AddIngredientToPlate)) return;
        // 未指定食材=任意食材都算；指定了就必须匹配（如面包片）
        KitchenObjectSO required = steps[currentIndex].requiredObject;
        if (required == null || e.kitchenObjectSO == required)
        {
            eventTaskDone = true;
        }
    }

    private void OnRecipeSuccessed(object sender, EventArgs e)
    {
        if (IsWaitingFor(TutorialTask.DeliverRecipe)) eventTaskDone = true;
    }

    private bool IsWaitingFor(TutorialTask task)
    {
        return started && currentIndex < steps.Count && steps[currentIndex].task == task;
    }

    // ---------- 流程控制 ----------

    private void OnGameStateChanged(object sender, EventArgs e)
    {
        if (!started && GameManager.Instance.IsGamePlayingState())
        {
            BeginTutorial();
        }
    }

    private void BeginTutorial()
    {
        if (started) return;
        started = true;
        currentIndex = 0;
        eventTaskDone = false;

        if (steps.Count == 0)
        {
            Finish();
            return;
        }
        ShowStep(0);
    }

    /// <summary>供 Custom 步骤或其他脚本手动推进到下一步。</summary>
    public void CompleteCurrentStep()
    {
        if (started && currentIndex < steps.Count)
        {
            GoToNextStep();
        }
    }

    private void GoToNextStep()
    {
        currentIndex++;
        eventTaskDone = false;

        if (currentIndex < steps.Count)
        {
            ShowStep(currentIndex);
        }
        else
        {
            Finish();
        }
    }

    private void ShowStep(int index)
    {
        if (panel != null) panel.SetActive(true);
        TutorialStep step = steps[index];
        if (titleText != null) titleText.text = step.title;
        if (contentText != null) contentText.text = step.content;
    }

    private void Finish()
    {
        if (hideWhenFinished)
        {
            if (panel != null) panel.SetActive(false);
        }
        else
        {
            if (titleText != null) titleText.text = finishedTitle;
            if (contentText != null) contentText.text = finishedContent;
        }
    }
}
