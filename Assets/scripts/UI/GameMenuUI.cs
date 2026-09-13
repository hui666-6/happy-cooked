using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    [SerializeField]private Button startbutton;
    [SerializeField]private Button quitbutton;
    private void Start()
    {
        startbutton.onClick.AddListener(()=>
        {
            Loader.load(Loader.scene.LevelChoose);
        }
        );
        quitbutton.onClick.AddListener(() => 
        {
          Application.Quit();
        });
    }
}
