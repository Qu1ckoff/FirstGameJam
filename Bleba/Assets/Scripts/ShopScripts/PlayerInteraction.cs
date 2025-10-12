using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public UIHint uiHint;

    public Transform handPoint; // точка в руке
    public Transform shoulder;  // объект плеча
    public Vector3 handRotationOnPickup; // локальный rotation при взятии
    public float throwForce = 5f;

    private Vector3 shoulderInitialRotation; // исходный локальный поворот плеча
    private ShelfCell lookedAtCell;
    private GameObject heldItem;

    private void Start()
    {
        if (shoulder != null)
            shoulderInitialRotation = shoulder.localEulerAngles; // сохраняем исходное положение
    }

    void Update()
    {
        CheckLookedAtCell();
        HandleInput();
    }

    void CheckLookedAtCell()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            lookedAtCell = hit.collider.GetComponent<ShelfCell>();
            if (lookedAtCell != null)
            {
                uiHint.ShowHint(lookedAtCell.transform);
            }
        }
        else
        {
            lookedAtCell = null;
            uiHint.HideHint();
        }
    }

    void HandleInput()
    {
        // Взятие предмета в руки
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (lookedAtCell != null && heldItem == null)
            {
                heldItem = lookedAtCell.TakeItemToHand(handPoint, shoulder, handRotationOnPickup);
            }
        }

        // Кладём на полку или бросаем вперед
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (heldItem != null)
            {
                if (lookedAtCell != null)
                {
                    lookedAtCell.PlaceItemFromHand(heldItem);
                    ResetShoulder();
                    heldItem = null;
                }
                else
                {
                    ThrowItem(heldItem);
                    ResetShoulder();
                    heldItem = null;
                }
            }
        }
    }

    void ThrowItem(GameObject item)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb == null) rb = item.AddComponent<Rigidbody>();
        item.transform.parent = null;
        rb.isKinematic = false;
        rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
    }

    // Возвращаем плечо в исходное положение
    void ResetShoulder()
    {
        if (shoulder != null)
            shoulder.localEulerAngles = shoulderInitialRotation;
    }
}
