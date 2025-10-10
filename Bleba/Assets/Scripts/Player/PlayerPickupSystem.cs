using System.Collections;
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

    private PickupItem currentTarget;
    private PickupItem leftItem;
    private PickupItem rightItem;

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

        // если новый предмет
        if (found != currentTarget)
        {
            if (currentTarget != null)
                PickupPromptUI.Instance.Hide();

            if (found != null)
            {
                PickupPromptUI.Instance.Show(found.transform);
                Debug.Log($"👀 Можно подобрать: {found.itemName}");
            }

            currentTarget = found;
        }

        // если предмет ушёл из зоны
        if (found == null && currentTarget != null)
        {
            PickupPromptUI.Instance.Hide();
            currentTarget = null;
        }
    }

    void HandlePickup()
    {
        if (currentTarget == null) return;

        if (Input.GetKeyDown(pickupKey))
        {
            // проверяем, есть ли свободная рука
            if (leftItem == null)
            {
                leftItem = currentTarget;
                leftItem.OnPicked(leftHand);
                Debug.Log($"🤲 Взял {leftItem.itemName} в левую руку");
            }
            else if (rightItem == null)
            {
                rightItem = currentTarget;
                rightItem.OnPicked(rightHand);
                Debug.Log($"✋ Взял {rightItem.itemName} в правую руку");
            }
            else
            {
                Debug.Log("👐 Руки заняты! Нельзя подобрать больше предметов.");
                return;
            }

            PickupPromptUI.Instance.Hide();
            currentTarget = null;
        }
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

        Vector3 dropPos = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
        Vector3 dropForce = transform.forward * 1f + Vector3.up * 1f;
        item.OnDropped(dropPos, dropForce);

        item = null;
    }

    // Для отладки
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
