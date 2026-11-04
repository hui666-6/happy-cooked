using System;
using UnityEngine;

public class WaitingReadyUI : MonoBehaviour
{

    public void Start()
    {
        GameManager.Instance.onLocalPlayerReadyChanged += GameManager_onLocalPlayerReadyChanged;
        GameManager.Instance.onChangState += GameManager_onchangstate;
        Hide();
    }

    private void GameManager_onchangstate(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsCountDownToStart())
        {
            Hide();
        }
    }

    public void GameManager_onLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.isLocalPlayerReady)
        {
            Show();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.onLocalPlayerReadyChanged -= GameManager_onLocalPlayerReadyChanged;
    }
}
