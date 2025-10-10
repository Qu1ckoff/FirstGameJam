using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName;
    public bool canBePicked = true;

    [Header("Attachment")]
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    public void OnPicked(Transform hand)
    {
        canBePicked = false;
        transform.SetParent(hand);
        transform.localPosition = localPositionOffset;
        transform.localEulerAngles = localRotationOffset;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    public void OnDropped(Vector3 dropPosition, Vector3 dropForce)
    {
        transform.SetParent(null);
        transform.position = dropPosition;
        canBePicked = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(dropForce, ForceMode.Impulse);
        }
    }
}
