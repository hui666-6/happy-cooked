using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image progressImage;
  
    public virtual void Awake()
  {
   
  }

    public void show()
    { 
      gameObject.SetActive(true);
    }
    public void hide()
    { 
      gameObject.SetActive(false);
    }
    
    public void UpdateProgress(float progerss)
    {
      show();
     progressImage.fillAmount=progerss;
        if (progerss == 1)
        {
            Invoke("hide", 0.5f);
            
        }
    }
}
