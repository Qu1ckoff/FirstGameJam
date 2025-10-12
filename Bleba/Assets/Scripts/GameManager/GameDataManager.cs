using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Универсальный менеджер для сохранений.
/// Хранит всё в одном JSON: игрок, коробка, магазин и т.д.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData player = new PlayerSaveData();
    public CarryBoxSaveData carryBox = new CarryBoxSaveData();
    public ShopSaveData shop = new ShopSaveData();
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public string leftItemID = "";
    public string rightItemID = "";
}

[System.Serializable]
public class CarryBoxSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public List<string> itemIDs = new List<string>();
}

[System.Serializable]
public class ShopSaveData
{
    public List<string> products = new List<string>();
}

public static class GameDataManager
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "game_save.json");
    private static GameSaveData data = new GameSaveData();

    // ==============================
    // 🔹 Общие методы
    // ==============================
    public static void SaveToDisk()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Игра сохранена ({savePath})");
    }

    public static void LoadFromDisk()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("📁 Нет файла сохранения, создаём новый.");
            data = new GameSaveData();
            return;
        }

        string json = File.ReadAllText(savePath);
        data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log($"📂 Игра загружена из {savePath}");
    }

    // ==============================
    // 🔸 Подсистемы
    // ==============================

    // --- Игрок ---
    public static PlayerSaveData Player => data.player;
    public static void SavePlayer(Vector3 pos, Quaternion rot, string leftID, string rightID)
    {
        data.player.position = pos;
        data.player.rotation = rot;
        data.player.leftItemID = leftID;
        data.player.rightItemID = rightID;
        SaveToDisk();
    }

    // --- Коробка ---
    public static CarryBoxSaveData CarryBox => data.carryBox;
    public static void SaveCarryBox(Vector3 pos, Quaternion rot, List<string> ids)
    {
        data.carryBox.position = pos;
        data.carryBox.rotation = rot;
        data.carryBox.itemIDs = new List<string>(ids);
        SaveToDisk();
    }

    // --- Магазин ---
    public static ShopSaveData Shop => data.shop;
    public static void SaveShop(List<string> products)
    {
        data.shop.products = new List<string>(products);
        SaveToDisk();
    }
}
