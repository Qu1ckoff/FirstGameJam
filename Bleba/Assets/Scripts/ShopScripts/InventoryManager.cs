using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private GameObject heldItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetHeldItem(GameObject item)
    {
        heldItem = item;
    }

    public void ClearHeldItem()
    {
        heldItem = null;
    }

    public GameObject GetHeldItem()
    {
        return heldItem;
    }
}
