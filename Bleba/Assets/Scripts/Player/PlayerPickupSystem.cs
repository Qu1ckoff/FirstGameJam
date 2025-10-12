using UnityEngine;
using System.Collections.Generic;

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

    private PickupItem currentTarget;
    private PickupItem leftItem;
    private PickupItem rightItem;

    void Awake()
    {
        GameDataManager.LoadFromDisk();

        // Восстановление предметов из сохранения
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("❌ Не найден ItemDatabase!");
            return;
        }

        var pdata = GameDataManager.Player;

        if (!string.IsNullOrEmpty(pdata.leftItemID))
        {
            var prefab = ItemDatabase.Instance.GetPrefab(pdata.leftItemID);
            if (prefab != null)
            {
                var obj = Instantiate(prefab);
                var pickup = obj.GetComponent<PickupItem>();
                TryPickupItem(pickup, leftHand, true);
            }
        }

        if (!string.IsNullOrEmpty(pdata.rightItemID))
        {
            var prefab = ItemDatabase.Instance.GetPrefab(pdata.rightItemID);
            if (prefab != null)
            {
                var obj = Instantiate(prefab);
                var pickup = obj.GetComponent<PickupItem>();
                TryPickupItem(pickup, rightHand, true);
            }
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
                PickupPromptUI.Instance?.Show(found.transform);

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
            TryPickupItem(currentTarget);
    }

    public bool TryPickupItem(PickupItem item, Transform hand = null, bool isRestore = false)
    {
        if (item == null || !item.canBePicked) return false;

        if (leftItem == null)
        {
            leftItem = item;
            leftItem.OnPicked(hand ?? leftHand);
        }
        else if (rightItem == null)
        {
            rightItem = item;
            rightItem.OnPicked(hand ?? rightHand);
        }
        else
        {
            Debug.Log("👐 Руки заняты!");
            return false;
        }

        if (!isRestore)
            PickupPromptUI.Instance?.Hide();

        UpdateCarryUI();
        return true;
    }

    void HandleDrop()
    {
        if (Input.GetKeyDown(dropKey))
        {
            if (rightItem != null)
                DropItem(ref rightItem);
            else if (leftItem != null)
                DropItem(ref leftItem);
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
        CarryIndicatorUI.Instance?.SetVisible(hasItem);
    }

    private void OnApplicationQuit()
    {
        string leftID = leftItem != null ? leftItem.GetComponent<ProductID>()?.id : "";
        string rightID = rightItem != null ? rightItem.GetComponent<ProductID>()?.id : "";
        GameDataManager.SavePlayer(transform.position, transform.rotation, leftID, rightID);
    }
}
