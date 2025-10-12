using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/save.json";

    public static void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Игра сохранена в {savePath}");
    }

    public static GameData Load()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("⚠️ Сохранение не найдено!");
            return null;
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);
        Debug.Log("📂 Игра загружена");
        return data;
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑 Сохранение удалено");
        }
    }

    public static bool HasSave() => File.Exists(savePath);
}
