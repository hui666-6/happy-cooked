using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerreadyUI : MonoBehaviour
{

    private void Start()
    {
        Hide();
        
    }
    public void Show()
    { 
     gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
