using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Настройки спавна")]
    [Tooltip("Список возможных NPC-префабов для спавна")]
    public List<GameObject> customerPrefabs = new List<GameObject>();

    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;

    [Tooltip("Задержка перед появлением следующего NPC")]
    public float spawnDelay = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnNextCustomer();
    }

    public void SpawnNextCustomer()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (customerPrefabs == null || customerPrefabs.Count == 0)
        {
            Debug.LogWarning("CustomerManager: список customerPrefabs пуст — некого спавнить!");
            yield break;
        }

        // Выбираем случайный префаб
        GameObject randomPrefab = customerPrefabs[Random.Range(0, customerPrefabs.Count)];

        // Спавним NPC
        GameObject npc = Instantiate(randomPrefab, spawnPoint.position, Quaternion.identity);

        // Настраиваем точки
        var customer = npc.GetComponent<CustomerNPC>();
        if (customer != null)
        {
            customer.counterPoint = counterPoint;
            customer.exitPoint = exitPoint;
        }
        else
        {
            Debug.LogWarning($"Префаб {randomPrefab.name} не содержит CustomerNPC!");
        }
    }
}
