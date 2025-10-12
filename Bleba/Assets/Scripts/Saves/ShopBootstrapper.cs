using UnityEngine;

public class ShopBootstrapper : MonoBehaviour
{
    void Awake()
    {
        ShopSaveManager.Load();
    }
}
