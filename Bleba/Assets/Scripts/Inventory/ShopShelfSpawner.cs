using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopShelfSpawner : MonoBehaviour
{
    [Header("Слоты для продуктов на полке")]
    public List<Transform> emptySlots;

    void Awake()
    {
        GameDataManager.LoadFromDisk(); // Загружаем всё один раз
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SpawnProducts();
    }

    private void SpawnProducts()
    {
        var products = GameDataManager.Shop.products;
        if (products.Count == 0) return;

        for (int i = 0; i < products.Count && i < emptySlots.Count; i++)
        {
            string id = products[i];
            GameObject prefab = ItemDatabase.Instance.GetPrefab(id);
            if (prefab != null)
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
                if (pid != null)
                    ids.Add(pid.id);
            }
        }
        return ids;
    }

    public void SaveShelfState()
    {
        List<string> shelfProducts = GetShelfProductIDs();
        GameDataManager.SaveShop(shelfProducts);
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
