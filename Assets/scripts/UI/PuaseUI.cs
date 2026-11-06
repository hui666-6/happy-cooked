using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PuaseUI : MonoBehaviour
{
    [SerializeField] private GameObject uiparent;
    [SerializeField] private Button continuebutton;
    [SerializeField] private Button menubutton;
    [SerializeField] private Button Settingsbutton;
    private void Start()
    {
        hide();
        GameManager.Instance.onLocalGameUnPaused += GameManager_OnGameUnPaused;
        GameManager.Instance.onLocalGamePaused += GameManager_OnGamePaused;
        continuebutton.onClick.AddListener(() => 
        {
            GameManager.Instance.ToggleGame();
        }
        );
        menubutton.onClick.AddListener(() =>
        {
            Loader.load(Loader.scene.GameMenu);
            NetworkManager.Singleton.Shutdown();
        }
        );
        Settingsbutton.onClick.AddListener(() => 
        {
            SettingsUI.instance.show();
        }); 
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        show();
    }

    private void GameManager_OnGameUnPaused(object sender, System.EventArgs e)
    {
       hide() ;
    }

    private void show()
    { 
      uiparent.SetActive(true);
    }
    private void hide()
    {
        uiparent.SetActive(false);
    }

}
