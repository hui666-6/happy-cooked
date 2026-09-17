using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class gameinput : MonoBehaviour
{
    private Gamecontrol gamecontrol;
    public event EventHandler OnInteractAction;//�¼�ί��
    public event EventHandler OnOperateAction;
    public event EventHandler OnPause;
    private  const string GAMEINPUT_BINDINGS = "gameinput_binding";
    public static gameinput Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中已存在 gameinput，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gamecontrol = new Gamecontrol();
        gamecontrol.player.Enable();
        if (PlayerPrefs.HasKey(GAMEINPUT_BINDINGS))
        {
            gamecontrol.LoadBindingOverridesFromJson(PlayerPrefs.GetString(GAMEINPUT_BINDINGS));
        }
        gamecontrol.player.Interact.performed += Interact_performed;
        gamecontrol.player.Operate.performed += Operate_performed;
        gamecontrol.player.Pause.performed += Pause_performed;
    }
    public enum BindingType
    { 
      forward,
      back,
      left,
      right,
      get,
      cut,
      pause
    
    }
    public string GetBindingDisplayString(BindingType bindingType)
    {
        switch (bindingType)
        { 
            case BindingType.forward:
              return gamecontrol.player.move.bindings[1].ToDisplayString();
            case BindingType.back:
                return gamecontrol.player.move.bindings[2].ToDisplayString();
            case BindingType.left:
                return gamecontrol.player.move.bindings[3].ToDisplayString();
            case BindingType.right:
                return gamecontrol.player.move.bindings[4].ToDisplayString();
            case BindingType.get:
                return gamecontrol.player.Interact.bindings[0].ToDisplayString();
            case BindingType.cut:
                return gamecontrol.player.Operate.bindings[0].ToDisplayString();
            case BindingType.pause:
                return gamecontrol.player.Pause.bindings[0].ToDisplayString();
            default:
                break;
        }
        return"";    
    
    }
    public void ReBinding(BindingType bindingType,Action Oncomplete)
    {
        gamecontrol.player.Disable();
        InputAction inputAction = null;
        int index = -1;
        switch (bindingType) 
        {
            case BindingType.forward:
                index = 1;
                inputAction = gamecontrol.player.move;
                break;
            case BindingType.back:
                index = 2;
                inputAction= gamecontrol.player.move;
                break;
            case BindingType.left:
                index = 3;
                inputAction = gamecontrol.player.move;
                break;
            case BindingType.right:
                index = 4;
                inputAction = gamecontrol.player.move;
                break;
            case BindingType.get:
                index = 0;
                inputAction = gamecontrol.player.Interact;
                break;
            case BindingType.cut:
                index = 0;
                inputAction = gamecontrol.player.Operate;
                break;  
            case BindingType.pause:
                index = 0;
                inputAction = gamecontrol.player.Pause;
                break;
                default:
                break;
        }
        inputAction.PerformInteractiveRebinding(index).OnComplete(callback =>
        {
            callback.Dispose();
            gamecontrol.player.Enable();
            Oncomplete?.Invoke(); //Oncomplete���޲����޷���ֵ���͵�ί��
            PlayerPrefs.SetString(GAMEINPUT_BINDINGS, gamecontrol.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();

        }).Start();
    
    }
  
    private void OnDestroy()
    {
        gamecontrol.player.Interact.performed -= Interact_performed;
        gamecontrol.player.Operate.performed -= Operate_performed;
        gamecontrol.player.Pause.performed -= Pause_performed;
        gamecontrol.Dispose();

    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPause?.Invoke(this, EventArgs.Empty);
    }

    private void Operate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnOperateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this,EventArgs.Empty);
    }

    public Vector3 GetMovementDirectionNormalized()
    
    {
        Vector2 inputvector2 = gamecontrol.player.move.ReadValue<Vector2>();
        Vector3 direction = new Vector3(inputvector2.x, 0, inputvector2.y);
        direction = direction.normalized;
        return direction;


    }
  
}
