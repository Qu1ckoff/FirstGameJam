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
    }

    void Start()
    {
        inventoryUI = FindObjectOfType<BoxInventoryUI>();
        UpdateUI();
    }

    public bool AddItem(GameObject item)
    {
        if (storedItems.Count >= capacity) return false;

        storedItems.Push(item);
        item.SetActive(false);
        item.transform.SetParent(transform);

        UpdateUI();
        SaveBox();
        return true;
    }

    public void TakeItem()
    {
        if (storedItems.Count == 0) return;

        var item = storedItems.Pop();
        item.transform.SetParent(null);
        item.SetActive(true);
        item.transform.position = itemSpawnPoint.position;

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
    }

    void UpdateUI()
    {
        inventoryUI?.UpdateCount(storedItems.Count, capacity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickupItem"))
            AddItem(other.gameObject);
    }

    void OnApplicationQuit() => SaveBox();
}
