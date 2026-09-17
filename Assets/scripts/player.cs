using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class player :KitchenObjectHolder
{
    [SerializeField] private float movespeed = 7;
    [SerializeField] private float roatespeed = 10;
    [SerializeField] private gameinput gameinput;
    [SerializeField] private LayerMask counterLayerMask;
    public static player Instance { get; private set; }
    private bool iswalking = false;
    private BaseCounter selectedcounter;
    

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 player，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        gameinput.OnInteractAction += Gameinput_OnInteractAction;
        gameinput.OnOperateAction += Gameinput_OnOperateAction;
    }

    private void Gameinput_OnOperateAction(object sender, System.EventArgs e)
    {
        selectedcounter?.InteractOperate(this);
     
    }

    private void Gameinput_OnInteractAction(object sender, System.EventArgs e)
    {
        selectedcounter?.Interact(this);
    }
    void Update()
    {
        HandleInteraction();
    }
    private void FixedUpdate()
    {
        HandleMovement();
    }
    public bool Iswalking
    {
        get
        {
            return iswalking;
        }
    }

    private void HandleMovement()
    {
        Vector3 direction = gameinput.GetMovementDirectionNormalized();
        transform.position += direction * Time.deltaTime * movespeed; //�����ƶ�
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
            if (hitinfo.transform.TryGetComponent<BaseCounter>(out BaseCounter counter))//�ж������Ƿ���һ����̨,���õ����������counter
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

    public void SetSelectedCounter(BaseCounter counter) //��¼������ײ�Ĺ�̨

    {        
        if (counter != selectedcounter)
        {
            selectedcounter?.CancelSelect(); //�ɵ�ȡ��ѡ��
            counter?.SelectCounter();//�µĸ���
            this.selectedcounter = counter;//���¸�ֵ
        }     
    }
}






