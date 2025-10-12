using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
<<<<<<< HEAD
        GameDataManager.LoadFromDisk();

        // восстановление
        transform.position = GameDataManager.CarryBox.position;
        transform.rotation = GameDataManager.CarryBox.rotation;

        foreach (string id in GameDataManager.CarryBox.itemIDs)
        {
            GameObject prefab = ItemDatabase.Instance.GetPrefab(id);
            if (prefab != null)
            {
                var obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                storedItems.Push(obj);
            }
        }
=======
>>>>>>> parent of 3d1ef82c (0.0.11)
    }

    void Start()
    {
        inventoryUI = FindObjectOfType<BoxInventoryUI>();
        UpdateUI();
    }

<<<<<<< HEAD
=======
    // ============================
    // 📦 ИНВЕНТАРЬ
    // ============================

>>>>>>> parent of 3d1ef82c (0.0.11)
    public bool AddItem(GameObject item)
    {
        if (storedItems.Count >= capacity) return false;

        // Отсоединяем физику/трансформ и прячем предмет
        storedItems.Push(item);
<<<<<<< HEAD
=======

        // Деактивируем (скрываем) и прикрепляем к коробке
        var rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
        {
            rbItem.isKinematic = true;
            rbItem.detectCollisions = false;
        }

        item.transform.SetParent(transform, true);
>>>>>>> parent of 3d1ef82c (0.0.11)
        item.SetActive(false);
        item.transform.SetParent(transform);

<<<<<<< HEAD
        UpdateUI();
        SaveBox();
=======
        Debug.Log($"🟢 {item.name} помещён в коробку ({storedItems.Count}/{capacity})");
        UpdateUI();
>>>>>>> parent of 3d1ef82c (0.0.11)
        return true;
    }

    public void TakeItem()
    {
        if (storedItems.Count == 0) return;

<<<<<<< HEAD
        var item = storedItems.Pop();
        item.transform.SetParent(null);
=======
        GameObject item = storedItems.Pop();

        // Восстанавливаем физику и показываем
        item.transform.SetParent(null, true);
>>>>>>> parent of 3d1ef82c (0.0.11)
        item.SetActive(true);
        item.transform.position = itemSpawnPoint.position;

<<<<<<< HEAD
        SaveBox();
        UpdateUI();
    }

    public void SaveBox()
    {
        List<string> ids = new List<string>();
        foreach (var item in storedItems)
        {
            var pid = item.GetComponent<ProductID>();
            if (pid != null)
                ids.Add(pid.id);
        }

        GameDataManager.SaveCarryBox(transform.position, transform.rotation, ids);
=======
        var rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
        {
            rbItem.isKinematic = false;
            rbItem.detectCollisions = true;
        }

        Vector3 spawnPos = itemSpawnPoint != null ?
            itemSpawnPoint.position :
            transform.position + transform.forward * 1f + Vector3.up * 0.5f;

        item.transform.position = spawnPos;

        // небольшой импульс, чтобы предмет "вылетел"
        rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
            rbItem.AddForce(transform.forward * 0.4f + Vector3.up * 0.5f, ForceMode.Impulse);

        Debug.Log($"🔵 {item.name} извлечён из коробки ({storedItems.Count}/{capacity})");
        UpdateUI();
>>>>>>> parent of 3d1ef82c (0.0.11)
    }

    void UpdateUI()
    {
        inventoryUI?.UpdateCount(storedItems.Count, capacity);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если предмет попадает в триггер коробки — пытаемся положить его
        if (other.CompareTag("PickupItem"))
            AddItem(other.gameObject);
    }
<<<<<<< HEAD

    void OnApplicationQuit() => SaveBox();
=======
>>>>>>> parent of 3d1ef82c (0.0.11)
}
