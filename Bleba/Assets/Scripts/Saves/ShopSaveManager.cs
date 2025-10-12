using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class ShopSaveData
{
    public List<string> products = new List<string>();
}

public static class ShopSaveManager
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "shopdata.json");
    private static ShopSaveData data = new ShopSaveData();

    public static void Load()
    {
        if (!File.Exists(savePath))
        {
            data = new ShopSaveData();
            return;
        }

        string json = File.ReadAllText(savePath);
        data = JsonUtility.FromJson<ShopSaveData>(json);

        // Загружаем список продуктов в ShopData
        ShopData.productsToSell = new List<string>(data.products);
    }

    public static void Save(List<string> products)
    {
        data = new ShopSaveData { products = new List<string>(products) };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Данные сохранены: {products.Count} продуктов");
    }

    public static List<string> GetProducts() => new List<string>(data.products);
}
