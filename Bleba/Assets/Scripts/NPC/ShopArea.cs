using UnityEngine;

public class ShopArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        NPCBot bot = other.GetComponent<NPCBot>();
        if (bot != null)
        {
            bot.EnterShop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NPCBot bot = other.GetComponent<NPCBot>();
        if (bot != null)
        {
            bot.ExitShop();
        }
    }
}
