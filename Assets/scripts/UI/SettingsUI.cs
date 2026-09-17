using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class SettingsUI : MonoBehaviour
{   public static SettingsUI instance { get; private set; }
    [SerializeField] private GameObject uiparent;
    [SerializeField] private Button musicbutton;
    [SerializeField] private Button soundsbutton;
    [SerializeField] private TextMeshProUGUI soundtext;
    [SerializeField] private TextMeshProUGUI musictext;
    [SerializeField] private Button Exitbutton;


    [SerializeField] private Button forwardbutton;
    [SerializeField] private Button backbutton;
    [SerializeField] private Button leftbutton;
    [SerializeField] private Button rightbutton;
    [SerializeField] private Button getbutton;
    [SerializeField] private Button cutbutton;
    [SerializeField] private Button pausebutton;

    [SerializeField] private TextMeshProUGUI forward;
    [SerializeField] private TextMeshProUGUI back;
    [SerializeField] private TextMeshProUGUI left;
    [SerializeField] private TextMeshProUGUI right;
    [SerializeField] private TextMeshProUGUI get;
    [SerializeField] private TextMeshProUGUI cut;
    [SerializeField] private TextMeshProUGUI pause;

    [SerializeField] private GameObject rebinding;
    private readonly Dictionary<Button, UnityAction> buttonListeners = new Dictionary<Button, UnityAction>();
    public event EventHandler OnCloseButtonClicked;

    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("场景中已存在 SettingsUI，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    void Start()
    {
        hide();
        UpdateVisual();
        AddButtonListener(musicbutton, () => 
        {
            MusicManager.instance.OnChangeVolume();
            UpdateVisual();
        });
        AddButtonListener(soundsbutton, () =>
        {
            SoundManager.instance.ChangeVolume();
            UpdateVisual();
        });
        AddButtonListener(Exitbutton, () =>
        {
            hide();
            OnCloseButtonClicked?.Invoke(this, EventArgs.Empty);
        });
        AddRebindingListener(forwardbutton, gameinput.BindingType.forward);
        AddRebindingListener(backbutton, gameinput.BindingType.back);
        AddRebindingListener(leftbutton, gameinput.BindingType.left);
        AddRebindingListener(rightbutton, gameinput.BindingType.right);
        AddRebindingListener(getbutton, gameinput.BindingType.get);
        AddRebindingListener(cutbutton, gameinput.BindingType.cut);
        AddRebindingListener(pausebutton, gameinput.BindingType.pause);
     
    }
     
    /// <summary>设置面板当前是否处于打开状态，供计时逻辑判断是否应暂停。</summary>
    public bool IsOpen { get; private set; }

   public void show()
    { 
      uiparent.SetActive(true);
      IsOpen = true;
    }
    private void hide()
    {
      uiparent.SetActive(false);
      IsOpen = false;
      
    }
    private void UpdateVisual()
    {
        soundtext.text="音效:"+SoundManager.instance.GetVolume();
        musictext.text="音乐:"+MusicManager.instance.GetVolume();
        forward.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.forward);
        back.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.back);
        left.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.left);
        right.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.right);
        get.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.get);
        cut.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.cut);
        pause.text = gameinput.Instance.GetBindingDisplayString(gameinput.BindingType.pause);
    }
    private void Rebinding(gameinput.BindingType bindingType)
    {
        rebinding.SetActive(true);
        gameinput.Instance.ReBinding( bindingType , () => 
        {
            rebinding.SetActive(false);
            UpdateVisual();
        });
    }

    private void AddRebindingListener(Button button, gameinput.BindingType bindingType)
    {
        AddButtonListener(button, () => Rebinding(bindingType));
    }

    private void AddButtonListener(Button button, UnityAction listener)
    {
        button.onClick.AddListener(listener);
        buttonListeners.Add(button, listener);
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<Button, UnityAction> buttonListener in buttonListeners)
        {
            buttonListener.Key.onClick.RemoveListener(buttonListener.Value);
        }

        buttonListeners.Clear();
    }
 
}
