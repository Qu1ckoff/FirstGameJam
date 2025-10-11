using UnityEngine;
using UnityEngine.UI;

public class CarryIndicatorUI : MonoBehaviour
{
    public static CarryIndicatorUI Instance;

    [Header("UI References")]
    public Image carryIcon;              // иконка руки с предметом
    public float fadeSpeed = 5f;         // скорость появления/исчезновения
    public float popScale = 1.2f;        // масштаб "подпрыгивания"
    public float popSpeed = 8f;          // скорость анимации масштаба

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool targetVisible = false;
    private float currentScale = 1f;
    private float targetScale = 1f;

    void Awake()
    {
        Instance = this;

        rectTransform = carryIcon.GetComponent<RectTransform>();

        canvasGroup = carryIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = carryIcon.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.one * 0.9f;
    }

    void Update()
    {
        // Плавное появление / исчезновение
        float targetAlpha = targetVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Плавное возвращение масштаба к 1
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * popSpeed);
        rectTransform.localScale = Vector3.one * currentScale;
    }

    public void SetVisible(bool visible)
    {
        if (visible && !targetVisible)
        {
            // Анимация "всплывания" при появлении
            currentScale = popScale; // чуть больше
            targetScale = 1f;
        }

        targetVisible = visible;
    }
}
