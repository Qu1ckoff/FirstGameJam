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
    }

    void Start()
    {
        inventoryUI = FindObjectOfType<BoxInventoryUI>();
        UpdateUI();
    }

    // ============================
    // 📦 ИНВЕНТАРЬ
    // ============================

    public bool AddItem(GameObject item)
    {
        if (storedItems.Count >= capacity)
        {
            Debug.Log("🚫 Коробка переполнена!");
            return false;
        }

        // Отсоединяем физику/трансформ и прячем предмет
        storedItems.Push(item);

        // Деактивируем (скрываем) и прикрепляем к коробке
        var rbItem = item.GetComponent<Rigidbody>();
        if (rbItem != null)
        {
            rbItem.isKinematic = true;
            rbItem.detectCollisions = false;
        }

        item.transform.SetParent(transform, true);
        item.SetActive(false);

        Debug.Log($"🟢 {item.name} помещён в коробку ({storedItems.Count}/{capacity})");
        UpdateUI();
        return true;
    }

    public void TakeItem()
    {
        if (storedItems.Count == 0)
        {
            Debug.Log("📭 Коробка пуста!");
            return;
        }

        GameObject item = storedItems.Pop();

        // Восстанавливаем физику и показываем
        item.transform.SetParent(null, true);
        item.SetActive(true);

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
    }

    // ============================
    // 🎛 UI
    // ============================

    public void UpdateUI()
    {
        if (inventoryUI != null)
            inventoryUI.UpdateCount(storedItems.Count, capacity);
    }

    public void ShowInventoryUI(bool showR = true, bool attached = false)
    {
        if (inventoryUI != null)
            inventoryUI.Show(transform, storedItems.Count, capacity, showR, attached);
    }

    public void HideInventoryUI()
    {
        if (inventoryUI != null)
            inventoryUI.Hide();
    }

    // ============================
    // ⚙️ ФИЗИКА
    // ============================

    public void AttachToPlayer(CarryBoxSystem player, Vector3 attachPoint, float maxDistance)
    {
        if (joint != null)
            Destroy(joint);

        playerTransform = player.transform;
        isAttached = true;
        lastPlayerPos = attachPoint;

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = player.GetComponent<Rigidbody>();
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = player.transform.InverseTransformPoint(attachPoint);
        joint.maxDistance = maxDistance;
        joint.spring = springStrength;
        joint.damper = damping;
        joint.massScale = 2f;

        maxFollowDistance = maxDistance;
    }

    public void DetachFromPlayer()
    {
        if (joint != null)
            Destroy(joint);

        isAttached = false;
        playerTransform = null;
    }

    void FixedUpdate()
    {
        if (!isAttached || playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > maxFollowDistance * 0.98f)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            rb.AddForce(dir * (followForce * 0.5f), ForceMode.Impulse);
        }

        if (distance > maxFollowDistance * 0.7f)
        {
            Vector3 pullDir = (playerTransform.position - transform.position).normalized;
            rb.AddForce(pullDir * followForce, ForceMode.Force);
        }

        Vector3 velocity = rb.velocity;
        if (velocity.magnitude > 0.2f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationLerpSpeed);
        }
    }

    public Vector3 GetRopePoint()
    {
        return transform.position + Vector3.up * 0.3f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Если предмет попадает в триггер коробки — пытаемся положить его
        if (other.CompareTag("PickupItem"))
        {
            AddItem(other.gameObject);
        }
    }
}
