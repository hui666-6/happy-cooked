using Unity.Netcode;
using UnityEngine;


public class StoveCounter : BaseCounter

{
    [SerializeField] private FryingRecipeSO fryingrecipelist;
    [SerializeField] private FryingRecipeSO burningrecipelist;
    [SerializeField] private StoveCouonterVisual stoveCouonterVisual;
    [SerializeField] private FryingProgressBarui progressbarUI;
    [SerializeField] private AudioSource sound;

    private WarningUI warningUI;
    private float warningtimenormalize = 0.5f;
    // 服务器用来推进逻辑，客户端用来算进度条的分母，由 RPC 同步到每个端
    private FryingRecipe fryingrecipe;
    private FryingRecipe burningrecipe;

    // 只有服务器写，所有端读
    private NetworkVariable<StoveState> stoveState = new NetworkVariable<StoveState>(StoveState.Idle);
    private NetworkVariable<float> fryingTime = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTime = new NetworkVariable<float>(0f);
    public enum StoveState
    {
        Idle,
        Frying,
        Bruning
    }

    private void Awake()
    {
        warningUI = GetComponent<WarningUI>();
    }

    public override void OnNetworkSpawn()
    {
        fryingTime.OnValueChanged += OnNetWorkFryingTime_Valuechanged;
        burningTime.OnValueChanged += OnNetWorkBurningTime_Valuechanged;
        stoveState.OnValueChanged += OnStoveState_ValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        fryingTime.OnValueChanged -= OnNetWorkFryingTime_Valuechanged;
        burningTime.OnValueChanged -= OnNetWorkBurningTime_Valuechanged;
        stoveState.OnValueChanged -= OnStoveState_ValueChanged;
    }

    private void OnNetWorkFryingTime_Valuechanged(float perviousVlaue, float newValue)
    {
        // 停火或进入烧焦阶段后不再画炸制进度，避免重置计时时把进度条又显示出来
        if (stoveState.Value != StoveState.Frying) return;
        float fryingTimeMax = fryingrecipe == null ? 1 : fryingrecipe.fryingTime;
        progressbarUI.UpdateProgress(newValue / fryingTimeMax);
    }
    private void OnNetWorkBurningTime_Valuechanged(float perviousVlaue, float newValue)
    {
        if (stoveState.Value != StoveState.Bruning) return;
        float burningTimeMax = burningrecipe == null ? 1 : burningrecipe.fryingTime;
        float progress = newValue / burningTimeMax;
        progressbarUI.UpdateProgress(progress);
        // 警告在各端根据同步过来的进度显示，用归一化进度比较
        if (progress > warningtimenormalize)
        {
            warningUI.show();
        }
    }
    private void OnStoveState_ValueChanged(StoveState currentStoveState, StoveState newStoveState)
    {
        // 客户端不能写 NetworkVariable，这里只负责刷新表现
        UpdateStateVisual(newStoveState);
    }


    public override void Interact(player player)
    {
        if (player.IsHavekitchenobject()) //玩家手上有食材
        {
            if (IsHavekitchenobject() == false) //炉子上没有食材  则将食材的父物体设置为炉子 
            {
                if (fryingrecipelist.TryGetFryingRecipe(
                  player.GetKitchenObject().GetKitchenObjectSO(), out FryingRecipe fryingRecipe))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenobjectParent(this);
                    OnInteractServerRpc(fryingrecipelist.GetFryingRecipeIndex(fryingRecipe));
                }
                else if (burningrecipelist.TryGetFryingRecipe(
                    player.GetKitchenObject().GetKitchenObjectSO(), out FryingRecipe bruningrecipe))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenobjectParent(this);
                    // 放上来的是熟食，下标属于 burningrecipelist，要走单独的 RPC
                    OnInteractBurningServerRpc(burningrecipelist.GetFryingRecipeIndex(bruningrecipe));
                }
            }
            else
            {
                Debug.Log("炉子上已经有食物了");
                return;
            }
        }
        else
        {
            if (IsHavekitchenobject() == false)
            {
                return;
            }
            else//玩家手上没有食材，炉子上有食材  则将食材的父物体设置为玩家
            {
                // 先让服务器停火再转移，否则服务器继续计时，到点会去销毁玩家手上的食材
                TurnToIdleServerRpc();
                KitchenObject kitchenObject = GetKitchenObject();
                kitchenObject.SetKitchenobjectParent(player);

            }

        }
    }
    private void Update()
    {
        if (!IsServer)
            return;

        // 食材已经被拿走（停火请求还没处理）时不推进，避免对空食材操作
        if (!IsHavekitchenobject())
            return;

        switch (stoveState.Value)
        {
            case StoveState.Idle:
                break;
            case StoveState.Frying:
                fryingTime.Value += Time.deltaTime;
                if (fryingTime.Value >= fryingrecipe.fryingTime)
                {
                    KitchenObjectSO fryingOutput = fryingrecipe.output;
                    KitchenObject.DestoryKitchenObject(GetKitchenObject());
                    KitchenObject.CreateKitchenObject(fryingOutput, this);
                    // 用配方产物查表，不依赖新食材是否已经挂到炉子上
                    if (burningrecipelist.TryGetFryingRecipe(fryingOutput, out FryingRecipe newburningRecipe))
                    {
                        StartBurning(newburningRecipe);
                    }
                    else
                    {
                        turntoidle();
                    }


                }
                break;
            case StoveState.Bruning:

                burningTime.Value += Time.deltaTime;

                if (burningTime.Value >= burningrecipe.fryingTime)
                {
                    // 烧焦阶段的产物在 burningrecipe 里
                    KitchenObjectSO burningOutput = burningrecipe.output;
                    KitchenObject.DestoryKitchenObject(GetKitchenObject());
                    KitchenObject.CreateKitchenObject(burningOutput, this);
                    turntoidle();

                }

                break;
            default:
                break;

        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void OnInteractServerRpc(int fryingRecipeIndex)
    {
        StartFrying(fryingrecipelist.GetFryingRecipeFromList(fryingRecipeIndex), fryingRecipeIndex);
    }
    [ServerRpc(RequireOwnership = false)]
    public void OnInteractBurningServerRpc(int burningRecipeIndex)
    {
        StartBurning(burningrecipelist.GetFryingRecipeFromList(burningRecipeIndex));
    }
    [ServerRpc(RequireOwnership = false)]
    private void TurnToIdleServerRpc()
    {
        turntoidle();
    }
    [ClientRpc]
    public void OnInteractClientRpc(int fryingRecipeIndex)
    {
        fryingrecipe = fryingrecipelist.GetFryingRecipeFromList(fryingRecipeIndex);
    }
    [ClientRpc]
    private void SetBurningRecipeClientRpc(int burningRecipeIndex)
    {
        burningrecipe = burningrecipelist.GetFryingRecipeFromList(burningRecipeIndex);
    }

    // 仅服务器调用
    private void StartFrying(FryingRecipe fryingrecipe, int fryingRecipeIndex)
    {
        // 查表失败时不要进入 Frying，否则 Update 里会拿 null 配方做除法
        if (fryingrecipe == null)
        {
            Debug.LogWarning("没取到炸制配方，无法开始炸");
            return;
        }
        // 服务器自己也要记一份（独立服务器不执行 ClientRpc）
        this.fryingrecipe = fryingrecipe;
        OnInteractClientRpc(fryingRecipeIndex);
        fryingTime.Value = 0;
        stoveState.Value = StoveState.Frying;
    }
    // 仅服务器调用
    private void StartBurning(FryingRecipe burningrecipe)
    {
        if (burningrecipe == null)
        {
            Debug.LogWarning("这个食材不可以放在烤炉上");
            return;
        }
        this.burningrecipe = burningrecipe;
        SetBurningRecipeClientRpc(burningrecipelist.GetFryingRecipeIndex(burningrecipe));
        burningTime.Value = 0;
        stoveState.Value = StoveState.Bruning;
    }
    // 仅服务器调用
    private void turntoidle()
    {
        stoveState.Value = StoveState.Idle;
    }

    // 所有端执行：根据同步过来的状态刷新表现
    private void UpdateStateVisual(StoveState state)
    {
        switch (state)
        {
            case StoveState.Idle:
                progressbarUI.hide();
                stoveCouonterVisual.HideStoveEffect();
                sound.Pause();
                warningUI.hide();
                break;
            case StoveState.Frying:
                warningUI.hide();
                stoveCouonterVisual.ShowStoveEffect();
                sound.Play();
                break;
            case StoveState.Bruning:
                stoveCouonterVisual.ShowStoveEffect();
                if (!sound.isPlaying) sound.Play();
                break;
        }
    }
}
