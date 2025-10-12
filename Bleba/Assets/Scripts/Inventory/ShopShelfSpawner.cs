using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopShelfSpawner : MonoBehaviour
{
    [Header("Слоты для продуктов на полке")]
    public List<Transform> emptySlots;

    [Header("Список префабов")]
    public List<ProductReceiver.ProductEntry> productPrefabs;

    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        foreach (var entry in productPrefabs)
            prefabDict[entry.id] = entry.prefab;

        // Загружаем список продуктов для магазина
        ShopSaveManager.Load();
        ShopData.productsToSell = ShopSaveManager.GetProducts();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnProducts();
    }

    private void SpawnProducts()
    {
        var products = ShopData.productsToSell;
        if (products.Count == 0) return;

        for (int i = 0; i < products.Count && i < emptySlots.Count; i++)
        {
            string id = products[i];
            if (prefabDict.TryGetValue(id, out GameObject prefab))
                Instantiate(prefab, emptySlots[i].position, emptySlots[i].rotation, emptySlots[i]);
        }

        Debug.Log($"📦 Спавнено {products.Count} продуктов на полках!");
    }

    public List<string> GetShelfProductIDs()
    {
        List<string> ids = new List<string>();
        foreach (var slot in emptySlots)
        {
            if (slot.childCount > 0)
            {
                var pid = slot.GetChild(0).GetComponent<ProductID>();
                if (pid != null) ids.Add(pid.id);
            }
        }
        return ids;
    }
    public void SaveShelfState()
    {
        List<string> shelfProducts = GetShelfProductIDs(); // текущие продукты на полках
        ShopSaveManager.Save(shelfProducts); // сохраняем один список продуктов
        ShopData.productsToSell = new List<string>(shelfProducts);
        Debug.Log($"💾 Состояние магазина сохранено: {shelfProducts.Count} продуктов на полках");
    }
    public void TakeProductFromShelf(Transform slot)
    {
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
            SaveShelfState();
        }
    }

}
