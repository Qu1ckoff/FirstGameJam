//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;

//[System.Serializable]
//public class PlayerSaveData
//{
//    public Vector3 position;
//    public Quaternion rotation;
//    public string leftItemID;
//    public string rightItemID;
//}

//public static class PlayerSaveManager
//{
//    private static string savePath => Path.Combine(Application.persistentDataPath, "player.json");
//    private static PlayerSaveData data = new PlayerSaveData();

//    public static void Load()
//    {
//        if (!File.Exists(savePath))
//        {
//            data = new PlayerSaveData();
//            return;
//        }

//        string json = File.ReadAllText(savePath);
//        data = JsonUtility.FromJson<PlayerSaveData>(json);
//    }

//    public static void Save(Vector3 position, Quaternion rotation, string leftItemID, string rightItemID)
//    {
//        data = new PlayerSaveData
//        {
//            position = position,
//            rotation = rotation,
//            leftItemID = leftItemID,
//            rightItemID = rightItemID
//        };

//        string json = JsonUtility.ToJson(data, true);
//        File.WriteAllText(savePath, json);
//        Debug.Log($"💾 Игрок сохранён: позиция {position}, предметы в руках [{leftItemID}, {rightItemID}]");
//    }

//    public static PlayerSaveData GetData() => data;
//}
