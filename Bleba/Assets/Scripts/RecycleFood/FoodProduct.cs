using UnityEngine;

public class FoodProduct : MonoBehaviour
{
    public string foodName = "Бургер из мусора";
    public bool canBePickedUp = true;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
