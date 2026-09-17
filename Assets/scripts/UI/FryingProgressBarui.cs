using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FryingProgressBarui : ProgressBarUI
{
    // Start is called before the first frame update
    Animator animator;
    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        animator.writeDefaultValuesOnDisable = true;
    }
    
}
