using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductReceiver : MonoBehaviour
{
    [Header("Настройки")]
    public int maxSlots = 8;
    public float playerDetectRadius = 3f;
    public string playerTag = "Player";

    [Header("Состояние")]
    public List<GameObject> storedProducts = new List<GameObject>();

    [Header("UI")]
    public RectTransform uiCanvas;
    public Text fillText;
    public Vector3 uiOffset = new Vector3(0, 2f, 0);
    private CanvasGroup uiGroup;
    private float fadeSpeed = 5f;
    private Transform player;
    private bool playerNearby = false;

    [Header("Список префабов продуктов")]
    public List<ProductEntry> productPrefabs;

    [System.Serializable]
    public struct ProductEntry
    {
        public string id;
        public GameObject prefab;
    }

    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        foreach (var entry in productPrefabs)
            prefabDict[entry.id] = entry.prefab;

        // Загружаем продукты из сохранения
        ShopSaveManager.Load();
        LoadProductsFromSave();
    }

    void Start()
    {
        if (uiCanvas != null)
        {
            uiGroup = uiCanvas.GetComponent<CanvasGroup>() ?? uiCanvas.gameObject.AddComponent<CanvasGroup>();
            uiGroup.alpha = 0f;
        }
        UpdateUI();
    }

    void Update()
    {
        DetectPlayer();
        if (uiGroup != null)
        {
            float targetAlpha = playerNearby ? 1f : 0f;
            uiGroup.alpha = Mathf.Lerp(uiGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    void LateUpdate()
    {
        if (uiCanvas != null && Camera.main != null)
        {
            uiCanvas.position = Camera.main.WorldToScreenPoint(transform.position + uiOffset);
        }
    }

    private void DetectPlayer()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(playerTag);
            if (found != null) player = found.transform;
        }
        if (player == null) return;
        playerNearby = Vector3.Distance(transform.position, player.position) <= playerDetectRadius;
    }
    private void LoadProductsFromSave()
    {
        storedProducts.Clear();

        foreach (string id in ShopSaveManager.GetProducts())
        {
            if (!prefabDict.ContainsKey(id))
            {
                Debug.LogWarning($"⚠ Префаб с ID '{id}' не найден!");
                continue;
            }
            AddProductByID(id);
        }

        UpdateUI();
        Debug.Log($"📂 Приёмник загружен: {storedProducts.Count} продуктов");
    }
    private void AddProductByID(string id)
    {
        if (prefabDict.TryGetValue(id, out GameObject prefab))
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            storedProducts.Add(obj);
        }
    }

    public bool AddProduct(GameObject product)
    {
        if (storedProducts.Count >= maxSlots) return false;

        storedProducts.Add(product);
        product.SetActive(false);
        UpdateUI();
        SaveProgress();
        return true;
    }

    private void UpdateUI()
    {
        if (fillText != null)
            fillText.text = $"{storedProducts.Count}/{maxSlots}";
    }
    public List<string> GetStoredProductIDs()
    {
        List<string> ids = new List<string>();

        // Очищаем список от уничтоженных объектов
        storedProducts.RemoveAll(p => p == null);

        foreach (var p in storedProducts)
        {
            if (p == null) continue; // на всякий случай ещё проверим
            var pid = p.GetComponent<ProductID>();
            if (pid != null) ids.Add(pid.id);
        }

        return ids;
    }

    public void SaveProgress()
    {
        ShopSaveManager.Save(GetStoredProductIDs());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickupItem"))
            AddProduct(other.gameObject);
    }

    private bool hasSaved = false;

    private void OnDisable()
    {
        AutoSave();
    }

    private void OnApplicationQuit()
    {
        AutoSave();
    }

    private void AutoSave()
    {
        if (hasSaved) return;
        hasSaved = true;

        Debug.Log("💾 Автосохранение ProductReceiver при выходе/смене сцены...");
        SaveProgress();
    }

}
