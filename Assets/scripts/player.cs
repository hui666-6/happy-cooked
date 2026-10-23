using System;
using Unity.Netcode;
using UnityEngine;

public class player : NetworkBehaviour, IKitchenObjectHolder
{
    [SerializeField] private float movespeed = 7;
    [SerializeField] private float roatespeed = 10;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform Holdpoint;


    public static player LocalInstance { get; private set; }

    public static event EventHandler OnPickUp;
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickSomething;

    private KitchenObject kitchenObject;
    private bool iswalking = false;
    private BaseCounter selectedcounter;

   

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

        newKitchenObject.transform.SetParent(Holdpoint);
        SetKitchenobject(newKitchenObject);
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public void Destorykitchenobject()
    {
        if (kitchenObject == null)
        {
            return;
        }

        Destroy(kitchenObject.gameObject);
        ClearKitchenObject();
    }

    public  void creatkitchenobject(GameObject kitchenobjectprefab)
    {
    //   KitchenGameMultiPlayer kitchenGameMultiPlayer = KitchenGameMultiPlayer.Instance;
    //   kitchenGameMultiPlayer.CreateKitchenObjectServerRpc(kitchenobjectprefab); 
    }

    public static void ClearStaticData()
    { 
        OnPickUp = null;
        OnAnyPlayerSpawned = null;
        OnAnyPickSomething = null;
    }

    private void HandleMovement()
    {
        Vector3 direction = gameinput.Instance.GetMovementDirectionNormalized();

        transform.position += direction * Time.deltaTime * movespeed;
        iswalking = direction != Vector3.zero;

        if (direction != Vector3.zero && transform.forward != direction)
        {
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * roatespeed);
        }
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
