using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public float interactDistance = 3f;
    public Transform handPoint;
    public float throwForce = 5f;
    public UIHint uiHint;

    private ShelfCell lookedAtCell;
    private GameObject lookedAtItem;
    private GameObject heldItem;

    void Update()
    {
        CheckLookedAtObject();
        HandleInput();
    }
    void CheckLookedAtObject()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, ~0, QueryTriggerInteraction.Collide);

        // Сбрасываем текущие объекты
        lookedAtItem = null;
        lookedAtCell = null;
        uiHint.HideHint();

        if (hits.Length == 0) return;

        // Сортируем по расстоянию
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // 1. Проверяем сначала PickupItem
        foreach (var h in hits)
        {
            if (h.collider.CompareTag("PickupItem"))
            {
                lookedAtItem = h.collider.gameObject;
                uiHint.ShowHint(lookedAtItem.transform, true); // E — взять предмет
                return;
            }
        }

        // 2. Проверяем полки
        foreach (var h in hits)
        {
            var shelf = h.collider.GetComponent<ShelfCell>();
            if (shelf != null)
            {
                lookedAtCell = shelf;

                if (heldItem == null && shelf.currentItem != null)
                {
                    // Руки пусты, на полке есть предмет — показываем E
                    uiHint.ShowHint(shelf.transform, true);
                }
                else if (heldItem != null)
                {
                    // Руки заняты — показываем Q
                    uiHint.ShowHint(shelf.transform, false);
                }
                else
                {
                    // Руки пусты, полка пустая — не показываем подсказку
                    uiHint.HideHint();
                }

                return;
            }
        }
    }

    void HandleInput()
    {
        // --- Взять предмет ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem != null) return;

            if (lookedAtItem != null)
            {
                PickupItemDirectly(lookedAtItem);
            }
            else if (lookedAtCell != null && lookedAtCell.currentItem != null)
            {
                // Берём предмет с полки
                heldItem = lookedAtCell.TakeItemToHand(handPoint);

                // Важное исправление: очищаем ячейку сразу после взятия
                lookedAtCell.currentItem = null;
            }
        }

        // --- Положить / бросить предмет ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (heldItem == null) return;

            if (lookedAtCell != null)
            {
                if (lookedAtCell.PlaceItemFromHand(heldItem))
                    heldItem = null;
            }
            else
            {
                ThrowItem(heldItem);
                heldItem = null;
                InventoryManager.Instance.ClearHeldItem();
            }
        }
    }

    void PickupItemDirectly(GameObject item)
    {
        if (heldItem != null) return;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        item.transform.SetParent(handPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        heldItem = item;
        InventoryManager.Instance.SetHeldItem(item);
    }

    void ThrowItem(GameObject item)
    {
        if (item == null) return;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null)
            rb = item.AddComponent<Rigidbody>();

        item.transform.parent = null;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
    }
}
