using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class player : NetworkBehaviour, IKitchenObjectHolder
{
    [SerializeField] private float movespeed = 7;
    [SerializeField] private float roatespeed = 10;
    [SerializeField] private float playerheight = 2;
    [SerializeField] private float playerradius = 0.7f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private Transform Holdpoint;
    [SerializeField] private List<Vector3> spawnpositionlist;

    public static player LocalInstance { get; private set; }

    public static event EventHandler OnPickUp;
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickSomething;

    private KitchenObject kitchenObject;
    private bool iswalking = false;
    private BaseCounter selectedcounter;
    RaycastHit[] hits = new RaycastHit[8]; 
   

    private void Start()
    {
        gameinput.Instance.OnInteractAction += Gameinput_OnInteractAction;
        gameinput.Instance.OnOperateAction += Gameinput_OnOperateAction;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        transform.position = spawnpositionlist[(int)OwnerClientId];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }
    private void Gameinput_OnOperateAction(object sender, EventArgs e)
    {
        selectedcounter?.InteractOperate(this);
    }

    private void Gameinput_OnInteractAction(object sender, EventArgs e)
    {
        selectedcounter?.Interact(this);
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        HandleInteraction();
        HandleMovement();
    }

    public bool Iswalking => iswalking;

    public bool IsHavekitchenobject()
    {
        return kitchenObject != null;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObject != null ? kitchenObject.GetKitchenObjectSO() : null;
    }

    public void SetKitchenobject(KitchenObject newKitchenObject)
    {
        if (kitchenObject != newKitchenObject && newKitchenObject != null)
        {
            OnPickUp?.Invoke(this, EventArgs.Empty);
            OnAnyPickSomething?.Invoke(this, EventArgs.Empty);
        }

        kitchenObject = newKitchenObject;

        if (kitchenObject != null)
        {
            kitchenObject.transform.localPosition = Vector3.zero;
        }
    }

    public Transform GetHoldpoint()
    {
        return Holdpoint;
    }

    public void AddKitchenObject(KitchenObject newKitchenObject)
    {
        if (newKitchenObject == null)
        {
            return;
        }

        // 食材是带 NetworkObject 的网络对象，不能 SetParent 到没有 NetworkObject 的 Holdpoint
        // （NGO 会抛 InvalidParentException 并把父级回滚），位置改由 FollowTransform 驱动
        newKitchenObject.SetFollowTarget(GetHoldpoint());
        SetKitchenobject(newKitchenObject);
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public static void ClearStaticData()
    { 
        OnPickUp = null;
        OnAnyPlayerSpawned = null;
        OnAnyPickSomething = null;
    }

    private void HandleMovement()//实现带碰撞的移动，撞墙时会沿墙移动，而不是卡死
    {
        Vector3 inputDirection = gameinput.Instance.GetMovementDirectionNormalized();
        if (inputDirection == Vector3.zero)
        {
            iswalking = false;
            return;
        }

        float moveDistance = movespeed * Time.deltaTime;
        Vector3 moveDirection = inputDirection;
        bool canMove = CanMove(moveDirection, moveDistance);

        if (!canMove)
        {
            // 斜向撞墙时，先尝试只沿 X 轴移动
            Vector3 directionX = new Vector3(inputDirection.x, 0, 0).normalized;
            canMove = Mathf.Abs(inputDirection.x) > 0.5f && CanMove(directionX, moveDistance);
            if (canMove)
            {
                moveDirection = directionX;
            }
            else
            {
                // X 轴也被挡住，再尝试只沿 Z 轴移动
                Vector3 directionZ = new Vector3(0, 0, inputDirection.z).normalized;
                canMove = Mathf.Abs(inputDirection.z) > 0.5f && CanMove(directionZ, moveDistance);
                if (canMove)
                {
                    moveDirection = directionZ;
                }
            }
        }

        // 只要有一个方向能走就移动，三个方向都被挡住时原地不动
        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }
        iswalking = canMove;

        // 朝向始终跟随输入方向，而不是滑动方向：顶着柜台时也能转身面向它，方便交互
        transform.forward = Vector3.Slerp(transform.forward, inputDirection, Time.deltaTime * roatespeed);
    }

    private bool CanMove(Vector3 moveDirection, float moveDistance)
    {
        return !Physics.BoxCast(transform.position, Vector3.one * playerradius, moveDirection, Quaternion.identity, moveDistance, collisionLayerMask);
    }

    private void HandleInteraction()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitinfo, 2f, counterLayerMask))
        {
            if (hitinfo.transform.TryGetComponent<BaseCounter>(out BaseCounter counter))
            {
                SetSelectedCounter(counter);
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    public void SetSelectedCounter(BaseCounter counter)
    {
        if (counter != selectedcounter)
        {
            selectedcounter?.CancelSelect();
            counter?.SelectCounter();
            selectedcounter = counter;
        }
    }

    public NetworkObject GetNetWorkObject()
    {
        return NetworkObject;
    }
}
