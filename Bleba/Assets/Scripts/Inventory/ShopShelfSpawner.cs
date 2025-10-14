using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopShelfSpawner : MonoBehaviour
{
    [Header("Ячейки полки (назначай вручную в инспекторе, порядок — как хочешь, верхняя -> нижняя и т.д.)")]
    public List<ShelfCell> shelfCells = new List<ShelfCell>();

    [Header("Список префабов (id -> prefab)")]
    public List<ProductReceiver.ProductEntry> productPrefabs;

    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    // флаг для защиты автосейва (как раньше)
    public static bool isExitingScene = false;

    private void Awake()
    {
        prefabDict.Clear();
        foreach (var entry in productPrefabs)
        {
            if (!string.IsNullOrEmpty(entry.id) && entry.prefab != null)
                prefabDict[entry.id] = entry.prefab;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (!isExitingScene) AutoSave();
    }

    private void OnApplicationQuit()
    {
        if (!isExitingScene) AutoSave();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShopData.productsToSell == null || ShopData.productsToSell.Count == 0)
        {
            ShopSaveManager.Load();
            ShopData.productsToSell = ShopSaveManager.GetProducts();
        }

        SpawnProductsIntoCells();
    }

    /// <summary>
    /// Спавним продукты строго в список shelfCells (в порядке shelfCells).
    /// Если ячейка уже занята — пропускаем её.
    /// </summary>
    private void SpawnProductsIntoCells()
    {
        var products = ShopData.productsToSell;
        if (products == null || products.Count == 0)
        {
            Debug.Log("⚠ ShopShelfSpawner: Нет продуктов для спавна — список пуст.");
            return;
        }

        if (shelfCells == null || shelfCells.Count == 0)
        {
            Debug.LogWarning("⚠ ShopShelfSpawner: shelfCells не настроен в инспекторе, нечего спавнить.");
            return;
        }

        int spawnIndex = 0;
        for (int i = 0; i < shelfCells.Count && spawnIndex < products.Count; i++)
        {
            ShelfCell cell = shelfCells[i];
            if (cell == null)
            {
                Debug.LogWarning($"⚠ ShopShelfSpawner: shelfCells[{i}] == null, пропускаю.");
                continue;
            }

            // Если в ячейке уже есть объект — пропускаем (не затираем)
            if (cell.currentItem != null)
            {
                continue;
            }

            string id = products[spawnIndex];

            if (!prefabDict.TryGetValue(id, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"⚠ ShopShelfSpawner: префаб с id '{id}' не найден в productPrefabs. Пропускаю.");
                spawnIndex++; // всё равно переходим к следующему id
                continue;
            }

            // Спавним как ребёнка именно ячейки (чтобы не падал)
            GameObject spawned = Instantiate(prefab, cell.transform);

            // жестко позиционируем внутри ячейки
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;

            // Ставим физику в кинематик, чтобы предметы не падали и не отталкивали друг друга
            var rb = spawned.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //rb.isKinematic = true;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // если есть Collider и хотим убрать столкновения с полкой — можно отключить, но обычно не нужно
            // var col = spawned.GetComponent<Collider>();
            // if (col != null) col.enabled = true;

            // Привязываем к ячейке
            cell.currentItem = spawned;

            spawnIndex++;
        }

        Debug.Log($"📦 ShopShelfSpawner: попытка спавна продуктов завершена. (productsToSell.Count = {products.Count}, shelfCells.Count = {shelfCells.Count})");
    }

    public List<string> GetShelfProductIDs()
    {
        List<string> ids = new List<string>();

        if (shelfCells == null || shelfCells.Count == 0)
        {
            ShelfCell[] found = GetComponentsInChildren<ShelfCell>(true);
            foreach (var c in found)
            {
                if (c != null && c.currentItem != null)
                {
                    var pid = c.currentItem.GetComponent<ProductID>();
                    if (pid != null) ids.Add(pid.id);
                }
            }
            return ids;
        }

        foreach (var cell in shelfCells)
        {
            if (cell == null || cell.currentItem == null) continue;
            var pid = cell.currentItem.GetComponent<ProductID>();
            if (pid != null) ids.Add(pid.id);
        }

        return ids;
    }

    public void SaveShelfState()
    {
        List<string> shelfProducts = GetShelfProductIDs();
        ShopSaveManager.Save(shelfProducts);
        ShopData.productsToSell = new List<string>(shelfProducts);
        Debug.Log($"💾 ShopShelfSpawner: Состояние магазина сохранено: {shelfProducts.Count} продуктов на полках");
    }

    private bool hasSaved = false;

    private void AutoSave()
    {
        if (hasSaved) return;
        hasSaved = true;

        Debug.Log("💾 ShopShelfSpawner: автосохранение перед завершением...");
        SaveShelfState();
    }
}
