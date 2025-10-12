using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemEntry
{
    public string id;
    public GameObject prefab;
}

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Список предметов (ID → Prefab)")]
    public List<ItemEntry> items = new List<ItemEntry>();
    private Dictionary<string, GameObject> itemDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Синглтон, чтобы не уничтожался при смене сцен
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        itemDict.Clear();
        foreach (var entry in items)
        {
            if (entry != null && !itemDict.ContainsKey(entry.id))
                itemDict.Add(entry.id, entry.prefab);
        }

        Debug.Log($"📦 ItemDatabase загружена: {itemDict.Count} предметов");
    }

    // 🔹 Метод получения префаба по ID
    public GameObject GetPrefab(string id)
    {
        if (itemDict.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogWarning($"⚠ Предмет с ID '{id}' не найден в ItemDatabase!");
        return null;
    }

    // 🔹 Проверка наличия
    public bool HasItem(string id) => itemDict.ContainsKey(id);
}
