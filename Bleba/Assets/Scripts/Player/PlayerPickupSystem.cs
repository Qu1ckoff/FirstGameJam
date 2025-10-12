using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float detectionRadius = 2f;
    public float detectionDistance = 2f;
    public LayerMask pickupMask;

    [Header("Hands")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Input")]
    public KeyCode pickupKey = KeyCode.Q;
    public KeyCode dropKey = KeyCode.G;

    [Header("Словарь предметов (ID -> Prefab)")]
    public List<ProductReceiver.ProductEntry> itemPrefabs;
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    private PickupItem currentTarget;
    private PickupItem leftItem;
    private PickupItem rightItem;
    void Awake()
    {
        // Заполняем словарь ID → Prefab
        foreach (var entry in itemPrefabs)
            prefabDict[entry.id] = entry.prefab;

        // Восстановление предметов в руках
        PlayerSaveManager.Load();
        var pdata = PlayerSaveManager.GetData();

        if (!string.IsNullOrEmpty(pdata.leftItemID) && prefabDict.ContainsKey(pdata.leftItemID))
        {
            GameObject leftObj = Instantiate(prefabDict[pdata.leftItemID]);
            PickupItem leftPickup = leftObj.GetComponent<PickupItem>();
            if (leftPickup != null)
                TryPickupItem(leftPickup, leftHand, true);
        }

        if (!string.IsNullOrEmpty(pdata.rightItemID) && prefabDict.ContainsKey(pdata.rightItemID))
        {
            GameObject rightObj = Instantiate(prefabDict[pdata.rightItemID]);
            PickupItem rightPickup = rightObj.GetComponent<PickupItem>();
            if (rightPickup != null)
                TryPickupItem(rightPickup, rightHand, true);
        }
    }

    void Update()
    {
        DetectPickup();
        HandlePickup();
        HandleDrop();
    }

    void DetectPickup()
    {
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Collider[] hits = Physics.OverlapSphere(center, detectionRadius, pickupMask);

        PickupItem found = null;

        foreach (var h in hits)
        {
            var item = h.GetComponent<PickupItem>();
            if (item != null && item.canBePicked)
            {
                found = item;
                break;
            }
        }

        if (found != currentTarget)
        {
            if (currentTarget != null)
                PickupPromptUI.Instance?.Hide();

            if (found != null)
            {
                PickupPromptUI.Instance?.Show(found.transform);
                Debug.Log($"👀 Можно подобрать: {found.itemName}");
            }

            currentTarget = found;
        }

        if (found == null && currentTarget != null)
        {
            PickupPromptUI.Instance?.Hide();
            currentTarget = null;
        }
    }

    void HandlePickup()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupItem(currentTarget);
        }
    }

    // Публичный метод: попытаться подобрать предмет программно (используется CarryBoxSystem)
    // Возвращает true если успешно подобрали (предмет "помещается в руку"), false — если не получилось.
    public bool TryPickupItem(PickupItem item)
    {
        if (item == null || !item.canBePicked) return false;

        // Если уже в руках — нельзя
        if (leftItem != null && rightItem != null)
        {
            Debug.Log("👐 Руки заняты! Нельзя подобрать больше предметов.");
            // Сюда можно добавить всплывашку BusyHandsUI.Instance?.ShowTemporary();
            return false;
        }

        // Сначала в левую рука, потом в правую
        if (leftItem == null)
        {
            leftItem = item;
            leftItem.OnPicked(leftHand);
            Debug.Log($"🤲 Взял {leftItem.itemName} в левую руку");
            PickupPromptUI.Instance?.Hide();
            UpdateCarryUI();
            return true;
        }
        else if (rightItem == null)
        {
            rightItem = item;
            rightItem.OnPicked(rightHand);
            Debug.Log($"✋ Взял {rightItem.itemName} в правую руку");
            PickupPromptUI.Instance?.Hide();
            UpdateCarryUI();
            return true;
        }

        return false;
    }

    private bool TryPickupItem(PickupItem item, Transform hand, bool isRestore = false)
    {
        if (item == null || !item.canBePicked) return false;

        if (leftItem == null)
        {
            leftItem = item;
            leftItem.OnPicked(hand);
            if (!isRestore) PickupPromptUI.Instance?.Hide();
            UpdateCarryUI();
            return true;
        }
        else if (rightItem == null)
        {
            rightItem = item;
            rightItem.OnPicked(hand);
            if (!isRestore) PickupPromptUI.Instance?.Hide();
            UpdateCarryUI();
            return true;
        }

        return false;
    }

    void HandleDrop()
    {
        if (Input.GetKeyDown(dropKey))
        {
            if (rightItem != null)
            {
                DropItem(ref rightItem);
                Debug.Log("🫳 Выбросил предмет из правой руки");
            }
            else if (leftItem != null)
            {
                DropItem(ref leftItem);
                Debug.Log("🫴 Выбросил предмет из левой руки");
            }
            else
            {
                Debug.Log("🤷 Нечего выбрасывать");
            }
        }
    }

    void DropItem(ref PickupItem item)
    {
        if (item == null) return;

        Vector3 dropPos = transform.position + transform.forward * 0.4f + Vector3.up * 0.5f;
        Vector3 dropForce = transform.forward * 0.75f + Vector3.up * 1f;
        item.OnDropped(dropPos, dropForce);

        item = null;
        UpdateCarryUI();
    }

    void UpdateCarryUI()
    {
        bool hasItem = leftItem != null || rightItem != null;
        CarryIndicatorUI.Instance?.SetVisible(hasItem); // если у тебя есть такой UI
    }

    // Для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }

    private void OnApplicationQuit()
    {
        string leftID = leftItem != null ? leftItem.GetComponent<ProductID>()?.id : "";
        string rightID = rightItem != null ? rightItem.GetComponent<ProductID>()?.id : "";
        PlayerSaveManager.Save(transform.position, transform.rotation, leftID, rightID);
    }
}
