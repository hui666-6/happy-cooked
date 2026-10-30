using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ContainerCounterVisual : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private ContainCounter containCounter;

    void Awake()
    {
        containCounter.OnContainerCounterInteract+=OnInteract_ContainerCouter;
    }
    void Start()
    {
        anim =GetComponent<Animator>();
    }
   public void  OnInteract_ContainerCouter(object sender, EventArgs e)
    {
        anim.SetTrigger("OpenClose");        
    }
    void OnDestroy()
    {
        containCounter.OnContainerCounterInteract-=OnInteract_ContainerCouter;
    }
}
