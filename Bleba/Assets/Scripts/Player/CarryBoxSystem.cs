using UnityEngine;

public class CarryBoxSystem : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 2f;
    public float detectionDistance = 2f;
    public LayerMask boxMask;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.R;
    public KeyCode takeItemKey = KeyCode.E;

    [Header("Rope Settings")]
    public float ropeLength = 3f;
    public LineRenderer ropeRenderer;
    public Transform ropeAttachPoint;
    public int ropeSegments = 10;

    [Header("View")]
    [Tooltip("Угол (в градусах), в пределах которого игрок считается 'смотрящим' на объект")]
    public float viewAngleDeg = 60f;

    private CarryBox currentBox;
    private CarryBox attachedBox;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        DetectBox();
        HandleAttachDetach();
        HandleBoxInteraction();
        UpdateRope();
    }

    bool IsBoxInLookZone(CarryBox box)
    {
        if (box == null) return false;

        Vector3 dirToBox = (box.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToBox);
        float threshold = Mathf.Cos(viewAngleDeg * Mathf.Deg2Rad);
        return dot >= threshold;
    }

    void DetectBox()
    {
        CarryBox found = null;

        if (attachedBox != null)
        {
            // При привязке: всегда показываем счётчик
            attachedBox.ShowInventoryUI(true, true); // showAttachButton, attached = true

            // Панель действий видна только если смотрим
            bool looking = IsBoxInLookZone(attachedBox);
            if (BoxInventoryUI.Instance != null)
            {
                if (looking)
                    BoxInventoryUI.Instance.ShowActionPanelOnly(true);
                else
                    BoxInventoryUI.Instance.HideActionPanelOnly();
            }

            currentBox = attachedBox;
            return;
        }

        // Не привязан — ищем коробку в зоне
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Collider[] hits = Physics.OverlapSphere(center, detectionRadius, boxMask);

        foreach (var h in hits)
        {
            var box = h.GetComponent<CarryBox>();
            if (box != null)
            {
                found = box;
                break;
            }
        }

        if (found != currentBox)
        {
            if (currentBox != null)
                currentBox.HideInventoryUI();
            currentBox = found;
        }

        if (currentBox != null)
        {
            bool showAttachButton = true;
            currentBox.ShowInventoryUI(showAttachButton, false);
        }
        else
        {
            if (BoxInventoryUI.Instance != null)
                BoxInventoryUI.Instance.Hide();
        }
    }

    void HandleAttachDetach()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (attachedBox == null && currentBox != null)
            {
                AttachBox(currentBox);
            }
            else if (attachedBox != null)
            {
                DetachBox();
            }
        }
    }

    void HandleBoxInteraction()
    {
        if (!Input.GetKeyDown(takeItemKey)) return;

        // Если игрок смотрит на привязанную коробку — берем из нее (приоритет)
        if (attachedBox != null && IsBoxInLookZone(attachedBox))
        {
            attachedBox.TakeItem();
            return;
        }

        // Если игрок не привязан или не смотрит на коробку — проверяем nearby/currentBox (в зоне)
        if (currentBox != null && IsBoxInLookZone(currentBox))
        {
            currentBox.TakeItem();
            return;
        }

        // Иначе — пробуем подобрать предмет с пола (через PlayerPickupSystem)
        TryPickupFromGround();
    }

    void TryPickupFromGround()
    {
        // ищем ближайший предмет в небольшом радиусе впереди игрока
        float pickupRadius = 1.2f;
        Vector3 center = transform.position + transform.forward * 1.0f;
        Collider[] hits = Physics.OverlapSphere(center, pickupRadius);

        // пытаемся сначала найти PickupItem и использовать PlayerPickupSystem
        PlayerPickupSystem pps = GetComponent<PlayerPickupSystem>();

        foreach (var h in hits)
        {
            var pickup = h.GetComponent<PickupItem>();
            if (pickup == null) continue;

            // если у нас есть PlayerPickupSystem — используем его метод (корректное поведение: attach to hand)
            if (pps != null)
            {
                bool picked = pps.TryPickupItem(pickup);
                if (picked)
                {
                    // успешно подобрали через систему — прекращаем поиск
                    return;
                }
                else
                {
                    // не удалось (руки заняты) — сообщим и прекращаем
                    Debug.Log("Руки заняты — нельзя подобрать предмет с пола.");
                    return;
                }
            }
            else
            {
                // fallback: если нет PlayerPickupSystem — элегантно привяжем предмет к игроку в левую руку (простая логика)
                Transform leftHand = transform.Find("LeftHand"); // если есть объект руки в иерархии
                if (leftHand != null)
                {
                    pickup.OnPicked(leftHand);
                    Debug.Log($"Picked {pickup.itemName} (fallback)");
                    return;
                }

                // иначе как крайний случай — просто актив/деактив (не желаемое поведение)
                pickup.gameObject.SetActive(false);
                return;
            }
        }

        Debug.Log("Нет предметов для подбора вокруг.");
    }


    void AttachBox(CarryBox box)
    {
        if (box == null) return;
        attachedBox = box;
        attachedBox.AttachToPlayer(this, ropeAttachPoint.position, ropeLength);
        Debug.Log("📦 Коробка прикреплена к игроку");
    }

    void DetachBox()
    {
        if (attachedBox != null)
        {
            attachedBox.DetachFromPlayer();
            attachedBox = null;

            if (BoxInventoryUI.Instance != null)
                BoxInventoryUI.Instance.Hide();

            if (ropeRenderer != null)
                ropeRenderer.enabled = false;

            Debug.Log("🪢 Коробка отпущена");
        }
    }

    void UpdateRope()
    {
        if (attachedBox == null || ropeRenderer == null) return;

        ropeRenderer.enabled = true;

        Vector3 start = ropeAttachPoint.position;
        Vector3 end = attachedBox.GetRopePoint();

        ropeRenderer.positionCount = ropeSegments + 1;

        for (int i = 0; i <= ropeSegments; i++)
        {
            float t = i / (float)ropeSegments;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y -= Mathf.Sin(t * Mathf.PI) * 0.2f;
            ropeRenderer.SetPosition(i, pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
