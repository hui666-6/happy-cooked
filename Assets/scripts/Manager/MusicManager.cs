using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.WSA;

public class MusicManager : MonoBehaviour
{   public static MusicManager instance { get; private set; }
    private AudioSource musicSource;
    private float originalvolume;
    private int volume=5;
    private const string MUSICMANAGER_VOLUME = "MusicManagerVolume";
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("场景中已存在 MusicManager，销毁重复实例：" + name);
            Destroy(gameObject);
            return;
        }
        instance = this;
        LoadVolume();
    }
    private void Start()
    {
        musicSource = GetComponent<AudioSource>();
        originalvolume=musicSource.volume;
        UpdateVolume();
    }
    public void OnChangeVolume()
    {
        volume++;
        if (volume > 10)
        {
            volume = 0;
            
        }
          SaveVolume();
          UpdateVolume();
    }
    private void UpdateVolume()
    { 
        if (volume == 0)
        {
            musicSource.enabled = false;
        }
        else
        {
            musicSource.enabled = true;
            musicSource.volume = originalvolume * (volume / 10.0f);
        }

    }
    public int  GetVolume()
    { 
      return volume;
    }
    private void SaveVolume()
    {
        PlayerPrefs.SetInt(MUSICMANAGER_VOLUME, volume);
    }
    private void LoadVolume()
    { 
      volume=PlayerPrefs.GetInt(MUSICMANAGER_VOLUME, volume);
    }
}
