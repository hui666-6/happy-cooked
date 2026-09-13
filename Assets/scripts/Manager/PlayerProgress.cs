using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>单个关卡的最高分记录。</summary>
[Serializable]
public class LevelHighScore
{
    public string levelKey;
    public int score;
    public int stars;
}

/// <summary>存档文件的数据结构。</summary>
[Serializable]
public class PlayerProgressData
{
    public List<LevelHighScore> highScores = new List<LevelHighScore>();
}

/// <summary>
/// 本地玩家进度：按关卡记录历史最高分，以 JSON 文件保存在 Application.persistentDataPath 下。
/// 关卡标识由 LevelManager.LevelKey 提供，多个关卡共用同一场景时也能区分。
/// </summary>
public static class PlayerProgress
{
    private const string FileName = "playerprogress.json";
    private const string DefaultLevelKey = "Default";

    private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
    private static PlayerProgressData data;
    private static bool isLoaded;

    /// <summary>存档文件路径：persistentDataPath/playerprogress.json。</summary>
    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>当前正在游玩的关卡标识。</summary>
    public static string CurrentLevelKey
    {
        get
        {
            if (LevelManager.Instance != null) return LevelManager.Instance.LevelKey;
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return string.IsNullOrEmpty(sceneName) ? DefaultLevelKey : sceneName;
        }
    }

    /// <summary>读取指定关卡的历史最高分，没有记录时返回 0。</summary>
    public static int GetHighScore(string levelKey)
    {
        if (string.IsNullOrEmpty(levelKey)) return 0;
        LevelHighScore record = FindRecord(levelKey);
        return record != null ? record.score : 0;
    }

    /// <summary>读取当前关卡的历史最高分。</summary>
    public static int GetCurrentLevelHighScore()
    {
        return GetHighScore(CurrentLevelKey);
    }

    /// <summary>读取指定关卡的星级（0~3），没有记录时返回 0。</summary>
    public static int GetStars(string levelKey)
    {
        if (string.IsNullOrEmpty(levelKey)) return 0;

        LevelHighScore record = FindRecord(levelKey);
        return record != null ? Mathf.Clamp(record.stars, 0, 3) : 0;
    }

    /// <summary>读取当前关卡的星级。</summary>
    public static int GetCurrentLevelStars()
    {
        return GetStars(CurrentLevelKey);
    }

    /// <summary>
    /// 提交一次结算分数：只有超过历史最高分时才写入存档文件，返回是否刷新了纪录。
    /// </summary>
    public static bool SubmitScore(string levelKey, int score)
    {
        return SubmitResult(levelKey, score, 0);
    }

    /// <summary>提交一次结算：分数和星级都只保留历史最好成绩，返回是否刷新了最高分。</summary>
    public static bool SubmitResult(string levelKey, int score, int stars)
    {
        if (string.IsNullOrEmpty(levelKey)) return false;

        int clampedStars = Mathf.Clamp(stars, 0, 3);
        bool newRecord = false;

        LevelHighScore record = FindRecord(levelKey);
        if (record == null)
        {
            if (score <= 0 && clampedStars <= 0) return false;
            data.highScores.Add(new LevelHighScore { levelKey = levelKey, score = Mathf.Max(score, 0), stars = clampedStars });
            newRecord = score > 0;
        }
        else
        {
            newRecord = score > record.score;
            if (!newRecord && clampedStars <= record.stars) return false;

            record.score = Mathf.Max(record.score, score);
            record.stars = Mathf.Max(record.stars, clampedStars);
        }

        Save();
        return newRecord;
    }

    /// <summary>清除指定关卡的本地最高分记录。</summary>
    public static void ClearHighScore(string levelKey)
    {
        if (string.IsNullOrEmpty(levelKey)) return;

        LevelHighScore record = FindRecord(levelKey);
        if (record == null) return;

        data.highScores.Remove(record);
        Save();
    }

    /// <summary>丢弃内存缓存并重新读取存档文件（手动改过 json 之后可调用）。</summary>
    public static void Reload()
    {
        isLoaded = false;
        EnsureLoaded();
    }

    private static LevelHighScore FindRecord(string levelKey)
    {
        EnsureLoaded();
        foreach (LevelHighScore record in data.highScores)
        {
            if (record != null && record.levelKey == levelKey) return record;
        }
        return null;
    }

    private static void EnsureLoaded()
    {
        if (isLoaded) return;
        isLoaded = true;
        data = new PlayerProgressData();

        try
        {
            if (!File.Exists(FilePath)) return;
            string json = File.ReadAllText(FilePath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json)) return;

            PlayerProgressData loaded = JsonUtility.FromJson<PlayerProgressData>(json);
            if (loaded == null) return;

            data = loaded;
            if (data.highScores == null) data.highScores = new List<LevelHighScore>();
        }
        catch (Exception e)
        {
            Debug.LogError("读取玩家进度失败，将使用空存档：" + FilePath + "；错误：" + e.Message);
            data = new PlayerProgressData();
        }
    }

    private static void Save()
    {
        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            // 先写临时文件再替换，避免写入过程中断导致存档损坏
            string tempPath = FilePath + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true), Utf8NoBom);
            if (File.Exists(FilePath)) File.Replace(tempPath, FilePath, null);
            else File.Move(tempPath, FilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("保存玩家进度失败：" + FilePath + "；错误：" + e.Message);
        }
    }
}
