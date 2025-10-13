using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
public class CustomerNPC : MonoBehaviour
{
    [Header("Настройки NPC")]
    public List<string> productList;          // Словарь продуктов (в инспекторе)
    public Transform counterPoint;            // Точка у прилавка
    public Transform exitPoint;               // Точка выхода (туда он уходит)
    public float waitTimeAtCounter = 10f;     // Сколько стоит у прилавка

    [Header("Фразы NPC")]
    public string requestPhrase = "Можно мне {0}?";
    public string correctPhrase = "Спасибо!";
    public string wrongPhrase = "Это не то, что мне нужно!";
    public string leavePhrase = "Что за магазины пошли, ужас!";

    private NavMeshAgent agent;
    private string wantedProduct;
    private bool receivedItem = false;
    private bool waiting = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickRandomProduct();
        GoToCounter();
    }

    void PickRandomProduct()
    {
        if (productList.Count == 0)
        {
            Debug.LogWarning("У NPC пустой список продуктов!");
            return;
        }

        wantedProduct = productList[Random.Range(0, productList.Count)];
        Say(string.Format(requestPhrase, wantedProduct));
    }

    void GoToCounter()
    {
        agent.SetDestination(counterPoint.position);
    }

    private void Update()
    {
        if (!waiting && Vector3.Distance(transform.position, counterPoint.position) < 1f)
        {
            waiting = true;
            StartCoroutine(WaitAtCounter());
        }
    }

    IEnumerator WaitAtCounter()
    {
        float timer = 0f;
        while (timer < waitTimeAtCounter && !receivedItem)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!receivedItem)
        {
            Say(leavePhrase);
            GoToExit();
        }
    }

    void GoToExit()
    {
        agent.SetDestination(exitPoint.position);
        StartCoroutine(DestroyAfterReach(exitPoint.position));
    }

    IEnumerator DestroyAfterReach(Vector3 point)
    {
        while (Vector3.Distance(transform.position, point) > 1.5f)
            yield return null;

        CustomerManager.Instance.SpawnNextCustomer();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (receivedItem) return;
        if (!other.CompareTag("PickupItem")) return;

        ProductID product = other.GetComponent<ProductID>();
        if (product == null) return;

        if (product.id == wantedProduct)
        {
            Say(correctPhrase);
            MoneyManager.Instance.AddMoney(product.cost);
            receivedItem = true;
            Destroy(other.gameObject);
            GoToExit();
        }
        else
        {
            Say(wrongPhrase);
            Destroy(other.gameObject);
        }
    }

    void Say(string phrase)
    {
        Debug.Log($"NPC: {phrase}");
        // Здесь можно добавить UI-облако с текстом, например SpeechBubble.Show(phrase)
    }
}
