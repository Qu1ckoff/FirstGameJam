using UnityEngine;
using UnityEngine.UI;

public class BoxPromptUI : MonoBehaviour
{
    public static BoxPromptUI Instance;

    [Header("UI References")]
    public RectTransform promptRoot;   // UI-элемент (дочерний объект Canvas)
    public Image keyIcon;              // Иконка клавиши "R"

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Transform target;

    void Awake()
    {
        Instance = this;

        if (promptRoot == null)
        {
            Debug.LogError("BoxPromptUI: promptRoot не назначен в инспекторе!");
            enabled = false;
            return;
        }

        parentCanvas = promptRoot.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("BoxPromptUI: promptRoot должен быть внутри Canvas!");
            enabled = false;
            return;
        }

        canvasRect = parentCanvas.transform as RectTransform;

        // Скрываем иконку по умолчанию
        promptRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null) return;

        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + Vector3.up * 1.5f);

        // Если коробка за камерой — скрываем иконку
        if (screenPos.z < 0)
        {
            promptRoot.gameObject.SetActive(false);
            return;
        }

        // Преобразуем экранную позицию в координаты Canvas
        Vector2 localPoint;
        Camera uiCamera = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : cam);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out localPoint);
        promptRoot.localPosition = localPoint;

        if (!promptRoot.gameObject.activeSelf)
            promptRoot.gameObject.SetActive(true);
    }

    public void Show(Transform targetTransform)
    {
        target = targetTransform;

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(true);
    }

    public void Hide()
    {
        target = null;
        if (promptRoot != null)
            promptRoot.gameObject.SetActive(false);
    }
}
