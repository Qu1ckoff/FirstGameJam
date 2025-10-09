using UnityEngine;

public class Eatable : MonoBehaviour
{
    [Header("Eatable Settings")]
    public string trashName = "Fish Bones";
    public GameObject resultFoodPrefab;
    public float eatTime = 2f;
    public float digestionTime = 5f;
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
        // Создаём копию материала, чтобы подсветка не влияла на все объекты с этим материалом
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
        if (state)
        {
            matInstance.color = highlightColor * highlightIntensity;
        }
        else
        {
            matInstance.color = originalColor;
        }
    }
}
