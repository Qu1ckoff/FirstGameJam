using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public float interactDistance = 3f;
    public Transform handPoint;
    public float throwForce = 5f;
    public UIHint uiHint;

    [Header("Выход из магазина")]
    public string citySceneName = "CityScene";
    public float exitHoldTime = 2f;

    private float exitHoldTimer = 0f;
    private bool lookingAtExit = false;
    private Transform exitZoneTransform;

    private ShelfCell lookedAtCell;
    private GameObject lookedAtItem;
    private CustomerNPC lookedAtNPC;
    private GameObject heldItem;

    void Update()
    {
        CheckLookedAtObject();
        HandleInput();
        HandleExitProgress();
    }

    void CheckLookedAtObject()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactDistance, ~0, QueryTriggerInteraction.Collide);

        lookedAtItem = null;
        lookedAtCell = null;
        lookedAtNPC = null;
        lookingAtExit = false;
        exitZoneTransform = null;

        uiHint.HideHint();

        if (hits.Length == 0) return;
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("PickupItem"))
            {
                lookedAtItem = h.collider.gameObject;
                uiHint.ShowHint(lookedAtItem.transform, true);
                return;
            }
        }

        foreach (var h in hits)
        {
            var shelf = h.collider.GetComponent<ShelfCell>();
            if (shelf != null)
            {
                lookedAtCell = shelf;
                if (heldItem == null && shelf.currentItem != null)
                    uiHint.ShowHint(shelf.transform, true);
                else if (heldItem != null)
                    uiHint.ShowHint(shelf.transform, false);
                else
                    uiHint.HideHint();
                return;
            }
        }

        foreach (var h in hits)
        {
            var npc = h.collider.GetComponent<CustomerNPC>();
            if (npc != null)
            {
                lookedAtNPC = npc;
                uiHint.ShowRHint(npc.transform);
                return;
            }
        }

        // 🔹 Проверяем зону выхода
        foreach (var h in hits)
        {
            if (h.collider.CompareTag("ExitZone"))
            {
                lookingAtExit = true;
                exitZoneTransform = h.collider.transform;
                uiHint.ShowExitHint(exitZoneTransform);
                return;
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem != null) return;
            if (lookedAtItem != null)
                PickupItemDirectly(lookedAtItem);
            else if (lookedAtCell != null && lookedAtCell.currentItem != null)
            {
                heldItem = lookedAtCell.TakeItemToHand(handPoint);
                lookedAtCell.currentItem = null;
            }
        }

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (lookedAtNPC != null)
            {
                lookedAtNPC.ForceLeave();
                uiHint.HideHint();
            }
        }
    }

    void HandleExitProgress()
    {
        if (!lookingAtExit)
        {
            exitHoldTimer = 0f;
            uiHint.UpdateExitProgress(0f);
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            exitHoldTimer += Time.deltaTime;
            uiHint.UpdateExitProgress(exitHoldTimer / exitHoldTime);

            if (exitHoldTimer >= exitHoldTime)
            {
                SceneManager.LoadScene(citySceneName);
            }
        }
        else
        {
            exitHoldTimer = 0f;
            uiHint.UpdateExitProgress(0f);
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
