using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
public class CustomerNPC : MonoBehaviour
{
    [Header("Настройки NPC")]
    public List<string> productList;
    public Transform counterPoint;
    public Transform exitPoint;
    public float waitTimeAtCounter = 10f;

    [Header("Фразы NPC (списки вариантов)")]
    public List<string> requestPhrases = new List<string>();
    public List<string> correctPhrases = new List<string>();
    public List<string> wrongPhrases = new List<string>();
    public List<string> leavePhrases = new List<string>();

    [Header("Звуки и эффекты")]
    [Tooltip("Звук, если NPC доволен покупкой")]
    public AudioClip happySound;

    [Tooltip("Звук, если NPC недоволен (дали не то)")]
    public AudioClip angrySound;

    [Tooltip("Источник звука (если не указан, создаётся автоматически)")]
    public AudioSource audioSource;

    [Header("Поведение")]
    public float responseDisplayTime = 2f;
    public float arriveDistance = 1f;

    private NavMeshAgent agent;
    public string wantedProduct;
    private bool receivedItem = false;
    private bool waiting = false;
    private bool requestShown = false;

    private SpeechBubble bubble;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        bubble = GetComponentInChildren<SpeechBubble>();

        // Если источника нет — создаём
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D-звук
            audioSource.maxDistance = 10f;
        }

        PickRandomProduct();
        GoToCounter();
    }

    void PickRandomProduct()
    {
        if (productList == null || productList.Count == 0)
        {
            Debug.LogWarning("У NPC пустой список продуктов!");
            wantedProduct = "";
            return;
        }

        wantedProduct = productList[Random.Range(0, productList.Count)];
    }

    void GoToCounter()
    {
        if (counterPoint != null)
            agent.SetDestination(counterPoint.position);
    }

    private void Update()
    {
        if (!requestShown && counterPoint != null && Vector3.Distance(transform.position, counterPoint.position) <= arriveDistance)
        {
            requestShown = true;
            ShowRequest();
            waiting = true;
            StartCoroutine(WaitAtCounter());
        }
    }

    void ShowRequest()
    {
        if (string.IsNullOrEmpty(wantedProduct)) return;

        string phraseTemplate = GetRandomPhrase(requestPhrases, "Можно мне {0}?");
        string phrase = string.Format(phraseTemplate, wantedProduct);

        if (bubble != null) bubble.ShowPhrase(phrase, 0f);
        else Debug.Log($"NPC request: {phrase}");
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
            string phrase = GetRandomPhrase(leavePhrases, "Что за магазины пошли, ужас!");
            if (bubble != null) bubble.ShowPhrase(phrase, responseDisplayTime);
            else Debug.Log(phrase);

            yield return new WaitForSeconds(responseDisplayTime);
            GoToExit();
        }
    }

    void GoToExit()
    {
        if (exitPoint != null)
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
            receivedItem = true;

            string phrase = GetRandomPhrase(correctPhrases, "Спасибо!");
            if (bubble != null) bubble.ShowPhrase(phrase, responseDisplayTime);
            else Debug.Log(phrase);

            // 💰 Выдаём игроку деньги
            MoneyManager.Instance.AddMoney(product.cost);

            // 🎵 Проигрываем довольный звук
            if (happySound != null)
                audioSource.PlayOneShot(happySound);

            Destroy(other.gameObject);
            StartCoroutine(DelayedExitAfterResponse());
        }
        else
        {
            string phrase = GetRandomPhrase(wrongPhrases, "Это не то, что мне нужно!");
            if (bubble != null) bubble.ShowPhrase(phrase, responseDisplayTime);
            else Debug.Log(phrase);

            // 😡 Звук недовольства
            if (angrySound != null)
                audioSource.PlayOneShot(angrySound);

            Destroy(other.gameObject);
        }
    }

    IEnumerator DelayedExitAfterResponse()
    {
        yield return new WaitForSeconds(responseDisplayTime);
        GoToExit();
    }

    public void ForceLeave()
    {
        if (receivedItem) return;

        receivedItem = true;
        string phrase = GetRandomPhrase(leavePhrases, "Что за магазины пошли, ужас!");
        if (bubble != null) bubble.ShowPhrase(phrase, responseDisplayTime);
        else Debug.Log(phrase);

        // 😡 Звук недовольства
        if (angrySound != null)
            audioSource.PlayOneShot(angrySound);

        if (exitPoint != null)
            transform.rotation = Quaternion.LookRotation((exitPoint.position - transform.position).normalized);

        StartCoroutine(DelayedExitAfterResponse());
    }

    private string GetRandomPhrase(List<string> list, string fallback)
    {
        if (list != null && list.Count > 0)
            return list[Random.Range(0, list.Count)];
        else
            return fallback;
    }
}
