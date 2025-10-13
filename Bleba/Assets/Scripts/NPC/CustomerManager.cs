using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("Настройки спавна")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;
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

    System.Collections.IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        GameObject npc = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);

        var customer = npc.GetComponent<CustomerNPC>();
        customer.counterPoint = counterPoint;
        customer.exitPoint = exitPoint;
    }
}
