using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EatableResult
{
    [Tooltip("Префаб продукта, который может появиться после поедания")]
    public GameObject prefab;

    [Range(0f, 1f)]
    [Tooltip("Шанс появления этого продукта (0–1)")]
    public float chance = 1f;
}

public class Eatable : MonoBehaviour
{
    [Header("Eatable Settings")]
    public string trashName = "Fish Bones";

    [Tooltip("Возможные продукты, которые могут появиться после поедания")]
    public List<EatableResult> resultFoods = new List<EatableResult>();

    [Tooltip("Время, за которое еда съедается")]
    public float eatTime = 2f;

    [Tooltip("Время 'переваривания' еды")]
    public float digestionTime = 5f;

    [Tooltip("Нагрузка на желудок (например, влияет на усталость игрока)")]
    public float stomachLoad = 10f;

    [Header("Visual Feedback")]
    public Color highlightColor = new Color(0.3f, 1f, 0.3f);
    public float highlightIntensity = 2f;

    private Color originalColor;
    private Material matInstance;
    private bool isHighlighted = false;

    [HideInInspector] public bool canBeEaten = true;

    void Start()
    {
        // Создаём копию материала, чтобы подсветка не влияла на все объекты
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            matInstance = Instantiate(rend.material);
            rend.material = matInstance;
            originalColor = matInstance.color;
        }
    }

    public void Highlight(bool state)
    {
        if (matInstance == null || isHighlighted == state)
            return;

        isHighlighted = state;
        matInstance.color = state
            ? highlightColor * highlightIntensity
            : originalColor;
    }

    /// <summary>
    /// Вызывает появление продуктов согласно их шансам.
    /// </summary>
    public void SpawnResultFoods(Vector3 spawnPos)
    {
        foreach (var entry in resultFoods)
        {
            if (entry.prefab == null) continue;

            float roll = Random.value; // 0..1
            if (roll <= entry.chance)
            {
                Instantiate(entry.prefab, spawnPos, Quaternion.identity);
                Debug.Log($"🍽️ Появился продукт: {entry.prefab.name} (шанс {entry.chance * 100}%)");
            }
        }
    }
}
