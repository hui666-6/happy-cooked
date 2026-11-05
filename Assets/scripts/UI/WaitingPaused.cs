using System;
using UnityEngine;

public class WaitingPaused : MonoBehaviour
{

    void Start()
    {
        GameManager.Instance.onMultiplayerPaused += GameManager_onMultiplayerPaused;
        GameManager.Instance.onMultiplayerUnPaused += GameManager_onMultiplayerUnPaused;
        Hide();
    }

    private void GameManager_onMultiplayerUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_onMultiplayerPaused(object sender, EventArgs e)
    {
        if (GameManager.Instance.isLocalPlayerPaused)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    void OnDestroy()
    {
        GameManager.Instance.onMultiplayerPaused -= GameManager_onMultiplayerPaused;
        GameManager.Instance.onMultiplayerUnPaused -= GameManager_onMultiplayerUnPaused;
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
