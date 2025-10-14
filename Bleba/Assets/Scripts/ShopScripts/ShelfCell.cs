using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShelfCell : MonoBehaviour
{
    [Header("Текущий предмет в ячейке")]
    public GameObject currentItem;

    [Header("Ссылка на спавнер полки (автоматически подставится)")]
    public ShopShelfSpawner parentShelf;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Автоматически находим спавнер, если не задан вручную
        if (parentShelf == null)
            parentShelf = GetComponentInParent<ShopShelfSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentItem == null && other.CompareTag("PickupItem"))
        {
            currentItem = other.gameObject;
            currentItem.transform.position = transform.position;
            currentItem.transform.rotation = Quaternion.identity;

            Rigidbody rb = currentItem.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            if (InventoryManager.Instance.GetHeldItem() == currentItem)
                InventoryManager.Instance.ClearHeldItem();

            // 💾 сохраняем текущее состояние полки
            parentShelf?.SaveShelfState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Если текущий предмет покидает триггер, очищаем ячейку
        if (other.gameObject == currentItem)
        {
            currentItem = null;
            parentShelf?.SaveShelfState(); // 💾
        }
    }

    public GameObject TakeItemToHand(Transform handPoint)
    {
        if (currentItem == null) return null;

        GameObject item = currentItem;
        currentItem = null; // очищаем ячейку сразу при взятии

        item.transform.SetParent(handPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        InventoryManager.Instance.SetHeldItem(item);

        // 💾 сохраняем текущее состояние
        parentShelf?.SaveShelfState();

        return item;
    }

    public bool PlaceItemFromHand(GameObject item)
    {
        if (currentItem != null || item == null) return false;

        currentItem = item;
        item.transform.SetParent(null);
        item.transform.position = transform.position;
        item.transform.rotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        InventoryManager.Instance.ClearHeldItem();

        // 💾 сохраняем текущее состояние
        parentShelf?.SaveShelfState();

        return true;
    }
}
