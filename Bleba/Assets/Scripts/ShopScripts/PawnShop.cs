using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PawnShop : MonoBehaviour
{
    [Header("Настройки ломбарда")]
    [Tooltip("Коэффициент выкупа — доля от цены товара (например, 0.25 = 25%)")]
    [Range(0f, 1f)]
    public float buyRate = 0.25f;

    [Tooltip("Включить звуковой эффект при продаже")]
    public AudioSource sellSound;

    [Tooltip("Текст всплывающего сообщения при продаже (опционально)")]
    public GameObject sellPopupPrefab;

    private void Reset()
    {
        // Чтобы не забыть включить триггер
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PickupItem"))
            return;

        ProductID product = other.GetComponent<ProductID>();
        if (product == null)
        {
            Debug.LogWarning($"⚠ Объект {other.name} не имеет ProductID!");
            return;
        }

        int reward = Mathf.RoundToInt(product.cost * buyRate);
        if (reward <= 0)
        {
            Debug.Log($"🪙 {product.name} слишком дешёвый, не принимаем.");
            return;
        }

        // Добавляем деньги игроку
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(reward);
        else
            Debug.LogWarning("💸 MoneyManager не найден на сцене!");

        // Звуковой эффект
        if (sellSound != null)
            sellSound.Play();

        // Всплывающий текст (опционально)
        if (sellPopupPrefab != null)
        {
            GameObject popup = Instantiate(sellPopupPrefab, other.transform.position + Vector3.up * 1.5f, Quaternion.identity);
            var textMesh = popup.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
                textMesh.text = $"+{reward}$";
            Destroy(popup, 1.5f);
        }

        // Удаляем предмет
        Destroy(other.gameObject);

        Debug.Log($"🏦 {product.id} продан за {reward}$ (1/4 от {product.cost})");
    }
}
