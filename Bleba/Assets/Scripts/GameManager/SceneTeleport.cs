// SceneTeleport.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTeleport : MonoBehaviour
{
    public string targetSceneName;
    public GameObject interactUI;
    public Image progressImage;
    public float holdTime = 1.5f;
    public string playerTag = "Player";

    private bool playerInRange = false;
    private Transform player;
    private float holdProgress = 0f;

    void Start()
    {
        if (interactUI != null) interactUI.SetActive(false);
        if (progressImage != null) progressImage.fillAmount = 0f;
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKey(KeyCode.E))
        {
            holdProgress += Time.deltaTime;
            if (progressImage != null)
                progressImage.fillAmount = holdProgress / holdTime;

            if (holdProgress >= holdTime)
            {
                holdProgress = 0f;
                TransferReceiverData();
                TeleportToScene();
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            holdProgress = 0f;
            if (progressImage != null)
                progressImage.fillAmount = 0f;
        }
    }

    private void TeleportToScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("⚠ Не указано название целевой сцены!");
            return;
        }

        if (interactUI != null) interactUI.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }
    private void TransferReceiverData()
    {
        var receiver = FindObjectOfType<ProductReceiver>();
        var shelfSpawner = FindObjectOfType<ShopShelfSpawner>();

        List<string> allProducts = new List<string>();

        if (receiver != null)
            allProducts.AddRange(receiver.GetStoredProductIDs());

        if (shelfSpawner != null)
            allProducts.AddRange(shelfSpawner.GetShelfProductIDs());

        // Сохраняем весь список в JSON
        ShopSaveManager.Save(allProducts);

        // Передаем только продукты из приёмника в магазин для спавна
        ShopData.productsToSell = receiver != null ? new List<string>(receiver.GetStoredProductIDs()) : new List<string>();

        Debug.Log($"📦 Приёмник и полки сохранены перед телепортом: {allProducts.Count} продуктов всего");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            player = other.transform;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            holdProgress = 0f;
            if (progressImage != null)
                progressImage.fillAmount = 0f;
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
}
