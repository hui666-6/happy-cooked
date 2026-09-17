using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningUI : MonoBehaviour
{
    [SerializeField] private GameObject warningui;
    [SerializeField] private Animator progressbar;
    private const string IS_FLICKER = "Flicker";
    private bool iswarning=false;
    private float warningrate = 0.2f;
    private float warningtime = 0;
    
    private void Update()
    {    if (iswarning)
        {
            warningtime += Time.deltaTime;
            if (warningtime >= warningrate)
            {
                warningtime = 0;
                SoundManager.instance.warningsound();
            }
        }

        
    }
    public void show()
    {
        if (iswarning == false)
        {
            iswarning = true;
            warningui.SetActive(true);
            progressbar.SetBool(IS_FLICKER, true);
        }
    }
    public void hide()
    {
        iswarning=false;
        warningui.SetActive(false);
        progressbar.SetBool(IS_FLICKER, false);

    }
}
