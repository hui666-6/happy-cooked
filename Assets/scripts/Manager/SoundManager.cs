using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundsSO audiocliprefsSO;
    private int volume = 5;
    private const string SOUNDMANAGER_VOLUME = "SoundManagerVolume";
    public static SoundManager instance { get; private set; }
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("场景中已存在 SoundManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        instance = this;
        LoadVolume();
    }
    private void Start()
    {
        // OrderManager 只存在于游戏关卡场景；在主菜单/选关等场景没有它，
        // 做空判断后 SoundManager 就能安全地存在于这些场景（供设置界面调音量）。
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnRecipeSuccessed += Instance_OnRecipeSuccessed;
            OrderManager.Instance.OnRecipeFailed += Instance_OnRecipeFailed;
        }
        // 下面这些是静态事件，订阅本身不依赖任何实例，非关卡场景里也不会触发，安全。
        CuttingCounter.onchop += CuttingCounter_onchop;
        KitchenObjectHolder.ondrop += KitchenObjectHolder_ondrop;
        KitchenObjectHolder.onpickup += KitchenObjectHolder_onpickup;
        TrashCounter.onobjecttransh += TrashCounter_onobjecttransh;
    }

    private void TrashCounter_onobjecttransh(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.trash);
    }

    private void KitchenObjectHolder_onpickup(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.pickup);
    }

    private void KitchenObjectHolder_ondrop(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.drop);
    }

    private void CuttingCounter_onchop(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.chop);
    }

    private void Instance_OnRecipeFailed(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.deliveryfail);
    }

    private void Instance_OnRecipeSuccessed(object sender, System.EventArgs e)
    {
        playSound(audiocliprefsSO.deliversuccess);
    }

    private void playSound(AudioClip[]clips,float volumemutipler=1.0f)
    {
        playsound(clips, Camera.main.transform.position);

    }
    private void playsound(AudioClip[] clips, Vector3 position, float volumemutipler = 0.2f)
    {
        if (volume == 0) return;
        int index=Random.Range(0,clips.Length);
        AudioSource.PlayClipAtPoint(clips[index], position, volumemutipler*(volume/10.0f));
    }
   public void stepsound(float volumemutipler = 0.2f)
   {
        playSound(audiocliprefsSO.footstep, volumemutipler = 0.2f);
   }
    public void countdownsound()
    {
        playSound(audiocliprefsSO.warning);
    }
    public void warningsound(float volumemutipler = 0.2f)
    { 
        playSound(audiocliprefsSO.warning,  volumemutipler = 0.2f); 
    }
    public void ChangeVolume()
    {
        volume++;
        if (volume > 10)
        {
            volume = 0;
        }
        SaveVolume();

    }
    public int GetVolume()
    { 
      return volume;
    }
    private void SaveVolume()
    {
        PlayerPrefs.SetInt(SOUNDMANAGER_VOLUME,volume);
    }
    private void LoadVolume()
    {
        volume = PlayerPrefs.GetInt(SOUNDMANAGER_VOLUME, volume);
    }
}
