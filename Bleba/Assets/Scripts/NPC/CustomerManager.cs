using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance; // остаётся, чтобы NPC могли обращаться внутри сцены

    [Header("Настройки спавна")]
    [Tooltip("Список возможных NPC-префабов для спавна")]
    public List<GameObject> customerPrefabs = new List<GameObject>();

    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;

    [Tooltip("Задержка перед появлением следующего NPC (в секундах)")]
    public float spawnDelay = 2f;

    private bool customerActive = false; // есть ли сейчас покупатель

    private void Awake()
    {
        // простой синглтон только для этой сцены
        Instance = this;
    }

    private void Start()
    {
        TrySpawnCustomer(); // запускаем первый спавн при старте
    }

    public void TrySpawnCustomer()
    {
        if (customerActive)
            return; // уже есть покупатель — ждём

        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        // Проверяем рабочие часы
        if (GlobalTimeManager.Instance != null)
        {
            float time = GlobalTimeManager.Instance.currentTime;
            if (time < 8f || time >= 20f)
            {
                Debug.Log("🌙 Магазин закрыт — NPC не спавнится.");
                yield break;
            }
        }

        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        if (customerPrefabs == null || customerPrefabs.Count == 0)
        {
            Debug.LogWarning("CustomerManager: список customerPrefabs пуст — некого спавнить!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("CustomerManager: spawnPoint не задан!");
            return;
        }

        GameObject randomPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
        GameObject npc = Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);

        var customer = npc.GetComponent<CustomerNPC>();
        if (customer != null)
        {
            customer.counterPoint = counterPoint;
            customer.exitPoint = exitPoint;
        }

        customerActive = true;
        Debug.Log("🧍 Новый покупатель вошёл в магазин!");
    }

    public void OnCustomerLeft()
    {
        customerActive = false;
        Debug.Log("🚶 Покупатель ушёл. Готовимся к следующему...");
        TrySpawnCustomer();
    }

    // для совместимости со старым кодом NPC
    public void SpawnNextCustomer()
    {
        OnCustomerLeft();
    }
}
