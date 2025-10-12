using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShelfCell : MonoBehaviour
{
    public GameObject currentItem;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentItem == null && other.CompareTag("PickupItem"))
        {
            currentItem = other.gameObject;
            currentItem.transform.position = transform.position;
            currentItem.transform.rotation = Quaternion.identity;

            Rigidbody rb = currentItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            if (InventoryManager.Instance.GetHeldItem() == currentItem)
                InventoryManager.Instance.ClearHeldItem();
        }
    }

    public GameObject TakeItemToHand(Transform handPoint, Transform shoulder, Vector3 handRotation)
    {
        if (currentItem == null) return null;

        GameObject item = currentItem;
        currentItem = null;

        item.transform.position = handPoint.position;
        item.transform.parent = handPoint;
        item.transform.localEulerAngles = handRotation; // задаём локальный поворот

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Поворачиваем плечо, если передано
        if (shoulder != null)
            shoulder.localEulerAngles = handRotation;

        InventoryManager.Instance.SetHeldItem(item);
        item.SetActive(true);

        return item;
    }

    public void PlaceItemFromHand(GameObject item)
    {
        if (currentItem != null || item == null) return;

        currentItem = item;
        item.transform.parent = null;
        item.transform.position = transform.position;
        item.transform.rotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        InventoryManager.Instance.ClearHeldItem();
    }
}
