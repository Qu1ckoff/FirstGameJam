using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CarryBoxSaveData
{
    public List<string> itemIDs = new List<string>();
    public Vector3 position;
    public Quaternion rotation;
}

public static class CarryBoxSaveManager
{
    private static string savePath => Path.Combine(Application.persistentDataPath, "carrybox.json");
    private static CarryBoxSaveData data = new CarryBoxSaveData();

    public static bool HasSave => File.Exists(savePath);

    public static void Load()
    {
        if (!File.Exists(savePath))
        {
            data = new CarryBoxSaveData();
            return;
        }

        string json = File.ReadAllText(savePath);
        data = JsonUtility.FromJson<CarryBoxSaveData>(json);
    }

    public static void Save(List<string> itemIDs, Vector3 position, Quaternion rotation)
    {
        data = new CarryBoxSaveData
        {
            itemIDs = new List<string>(itemIDs),
            position = position,
            rotation = rotation
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 CarryBox сохранён: {itemIDs.Count} предметов, позиция {position}");
    }

    public static List<string> GetItemIDs() => new List<string>(data.itemIDs);
    public static Vector3 GetPosition() => data.position;
    public static Quaternion GetRotation() => data.rotation;
}

[RequireComponent(typeof(Rigidbody))]
public class CarryBox : MonoBehaviour
{
    private Rigidbody rb;
    private SpringJoint joint;

    [Header("Physics Settings")]
    public float followForce = 15f;
    public float maxFollowDistance = 3f;
    public float springStrength = 40f;
    public float damping = 6f;
    public float rotationLerpSpeed = 4f;
    public float bounceForce = 3f;

    private Vector3 lastPlayerPos;
    private bool isAttached = false;
    private Transform playerTransform;

    [Header("Inventory Settings")]
    public int capacity = 5;
    public Transform itemSpawnPoint;
    private Stack<GameObject> storedItems = new Stack<GameObject>();
    private BoxInventoryUI inventoryUI;

    [Header("Словарь предметов (ID -> Prefab)")]
    public List<ProductReceiver.ProductEntry> itemPrefabs;
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    [Header("Стартовая позиция (если нет сохранения)")]
    public Vector3 startPosition = Vector3.zero;
    public Vector3 startRotation = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Заполняем словарь ID → Prefab
        foreach (var entry in itemPrefabs)
        {
            if (!prefabDict.ContainsKey(entry.id))
                prefabDict.Add(entry.id, entry.prefab);
        }

        // Загружаем предметы и позицию
        CarryBoxSaveManager.Load();
        LoadItemsFromSave();
    }

    void Start()
    {
        inventoryUI = FindObjectOfType<BoxInventoryUI>();
        UpdateUI();
    }

    // ============================
    // 📦 ИНВЕНТАРЬ
    // ============================

    private void LoadItemsFromSave()
    {
        storedItems.Clear();

        // Если сохранения нет — используем стартовую позицию
        if (!CarryBoxSaveManager.HasSave || CarryBoxSaveManager.GetPosition() == Vector3.zero)
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(startRotation);
            Debug.Log($"🧺 CarryBox установлена в стартовую позицию {startPosition}");
        }
        else
        {
            transform.position = CarryBoxSaveManager.GetPosition();
            transform.rotation = CarryBoxSaveManager.GetRotation();
        }

        foreach (string id in CarryBoxSaveManager.GetItemIDs())
        {
            if (!prefabDict.ContainsKey(id))
            {
                Debug.LogWarning($"⚠ Префаб с ID '{id}' не найден для CarryBox!");
                continue;
            }

            GameObject obj = Instantiate(prefabDict[id]);
            obj.SetActive(false);
            obj.transform.SetParent(transform, true);
            storedItems.Push(obj);
        }

        UpdateUI();
        Debug.Log($"📦 CarryBox загружена: {storedItems.Count} предметов, позиция {transform.position}");
    }

    public bool AddItem(GameObject item)
    {
        if (storedItems.Count >= capacity)
        {
            Debug.Log("🚫 Коробка переполнена!");
            return false;
        }

        storedItems.Push(item);

        var rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
        {
            rbItem.isKinematic = true;
            rbItem.detectCollisions = false;
        }

        item.transform.SetParent(transform, true);
        item.SetActive(false);

        UpdateUI();
        SaveBoxProgress();
        Debug.Log($"🟢 {item.name} помещён в коробку ({storedItems.Count}/{capacity})");
        return true;
    }

    public void TakeItem()
    {
        if (storedItems.Count == 0)
        {
            Debug.Log("📭 Коробка пуста!");
            return;
        }

        GameObject item = storedItems.Pop();
        item.transform.SetParent(null, true);
        item.SetActive(true);

        var rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
        {
            rbItem.isKinematic = false;
            rbItem.detectCollisions = true;
        }

        Vector3 spawnOffset = transform.forward * 1f + transform.right * 0.5f + Vector3.up * 0.5f;
        Vector3 spawnPos = itemSpawnPoint != null ? itemSpawnPoint.position + spawnOffset : transform.position + spawnOffset;
        item.transform.position = spawnPos;

        if (rbItem != null)
            rbItem.AddForce(transform.forward * 0.5f + Vector3.up * 0.5f, ForceMode.Impulse);

        UpdateUI();
        SaveBoxProgress();
        Debug.Log($"🔵 {item.name} извлечён ({storedItems.Count}/{capacity})");
    }

    public void SaveBoxProgress()
    {
        List<string> ids = new List<string>();
        foreach (var item in storedItems)
        {
            var pid = item.GetComponent<ProductID>();
            if (pid != null) ids.Add(pid.id);
        }

        CarryBoxSaveManager.Save(ids, transform.position, transform.rotation);
    }

    // ============================
    // 🎛 UI
    // ============================

    public void UpdateUI()
    {
        if (inventoryUI != null)
            inventoryUI.UpdateCount(storedItems.Count, capacity);
    }

    public void ShowInventoryUI(bool showR = true, bool attached = false)
    {
        if (inventoryUI != null)
            inventoryUI.Show(transform, storedItems.Count, capacity, showR, attached);
    }

    public void HideInventoryUI()
    {
        if (inventoryUI != null)
            inventoryUI.Hide();
    }

    // ============================
    // ⚙️ ФИЗИКА
    // ============================

    public void AttachToPlayer(CarryBoxSystem player, Vector3 attachPoint, float maxDistance)
    {
        if (joint != null)
            Destroy(joint);

        playerTransform = player.transform;
        isAttached = true;
        lastPlayerPos = attachPoint;

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = player.GetComponent<Rigidbody>();
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = player.transform.InverseTransformPoint(attachPoint);
        joint.maxDistance = maxDistance;
        joint.spring = springStrength;
        joint.damper = damping;
        joint.massScale = 2f;

        maxFollowDistance = maxDistance;
    }

    public void DetachFromPlayer()
    {
        if (joint != null)
            Destroy(joint);

        isAttached = false;
        playerTransform = null;
    }

    void FixedUpdate()
    {
        if (!isAttached || playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > maxFollowDistance * 0.98f)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            rb.AddForce(dir * (followForce * 0.5f), ForceMode.Impulse);
        }

        if (distance > maxFollowDistance * 0.7f)
        {
            Vector3 pullDir = (playerTransform.position - transform.position).normalized;
            rb.AddForce(pullDir * followForce, ForceMode.Force);
        }

        Vector3 velocity = rb.velocity;
        if (velocity.magnitude > 0.2f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
        }
    }

    public Vector3 GetRopePoint()
    {
        return transform.position + Vector3.up * 0.3f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickupItem"))
        {
            AddItem(other.gameObject);
        }
    }

    private bool hasSaved = false;

    private void OnDisable() => AutoSave();
    private void OnApplicationQuit() => AutoSave();

    private void AutoSave()
    {
        if (hasSaved) return;
        hasSaved = true;

        Debug.Log("💾 Автосохранение CarryBox при выходе/смене сцены...");
        SaveBoxProgress();
    }
}
