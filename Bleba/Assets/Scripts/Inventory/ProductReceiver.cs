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

    void Awake()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("❌ ItemDatabase не найден!");
            return;
        }

        GameDataManager.LoadFromDisk();
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
            uiCanvas.position = Camera.main.WorldToScreenPoint(transform.position + uiOffset);
    }

    private void DetectPlayer()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(playerTag);
            if (found != null)
                player = found.transform;
        }

        if (player == null) return;
        playerNearby = Vector3.Distance(transform.position, player.position) <= playerDetectRadius;
    }

    // =====================
    // 💾 Загрузка / сохранение
    // =====================
    private void LoadProductsFromSave()
    {
        storedProducts.Clear();

        var savedIDs = GameDataManager.Shop.products;
        foreach (string id in savedIDs)
        {
            GameObject prefab = ItemDatabase.Instance.GetPrefab(id);
            if (prefab == null)
            {
                Debug.LogWarning($"⚠ Продукт '{id}' не найден в ItemDatabase!");
                continue;
            }

            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            storedProducts.Add(obj);
        }

        UpdateUI();
        Debug.Log($"📦 Receiver: загружено {storedProducts.Count} продуктов");
    }

    public void SaveProgress()
    {
        GameDataManager.SaveShop(GetStoredProductIDs());
    }

    // =====================
    // ➕ Добавление продуктов
    // =====================
    public bool AddProduct(GameObject product)
    {
        if (storedProducts.Count >= maxSlots)
        {
            Debug.Log("🚫 Приёмник переполнен!");
            return false;
        }

        storedProducts.Add(product);
        product.SetActive(false);
        UpdateUI();
        SaveProgress();

        return true;
    }

    public List<string> GetStoredProductIDs()
    {
        List<string> ids = new List<string>();
        foreach (var p in storedProducts)
        {
            var pid = p.GetComponent<ProductID>();
            if (pid != null) ids.Add(pid.id);
        }
        return ids;
    }

    private void UpdateUI()
    {
        if (fillText != null)
            fillText.text = $"{storedProducts.Count}/{maxSlots}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickupItem"))
            AddProduct(other.gameObject);
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }
}
