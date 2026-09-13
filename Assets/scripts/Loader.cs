using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader

{

    public enum scene
    { 
      GameMenu,
      Loading,
      GameScene,
      LevelChoose
    }
    private static scene targetscene;
    private static string targetSceneName;
    public static void load(scene target)
    { 
        Time.timeScale = 1;
       targetscene = target;
       targetSceneName = null;
       SceneManager.LoadScene((int)scene.Loading);
    
    }
    /// <summary>按场景名加载：选关界面用它进入任意关卡场景。</summary>
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Loader.LoadScene：场景名为空，无法加载");
            return;
        }

        Time.timeScale = 1;
        targetSceneName = sceneName;
        SceneManager.LoadScene((int)scene.Loading);
    }

    public static void LoadBack()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            string sceneName = targetSceneName;
            targetSceneName = null;
            SceneManager.LoadScene(sceneName);
            return;
        }

        SceneManager.LoadScene((int)targetscene);
    }
}
