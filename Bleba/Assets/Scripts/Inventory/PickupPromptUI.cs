using UnityEngine;
using UnityEngine.UI;

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance;

    [Header("UI References")]
    // Перетаскивай сюда RectTransform небольшого UI-элемента (child of your main Canvas),
    // а не сам Canvas. Этот объект будет включаться/выключаться.
    public RectTransform promptRoot;
    public Image keyIcon; // опционально: иконка "Е"

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Transform target;

    void Awake()
    {
        Instance = this;

        if (promptRoot == null)
        {
            Debug.LogError("PickupPromptUI: promptRoot не назначен в инспекторе!");
            enabled = false;
            return;
        }

        parentCanvas = promptRoot.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("PickupPromptUI: promptRoot должен быть внутри Canvas!");
            enabled = false;
            return;
        }

        canvasRect = parentCanvas.transform as RectTransform;

        // Скрываем сам prompt (не весь Canvas!)
        promptRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null) return;

        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + Vector3.up * 1.5f);

        // если цель за камерой — скрываем подсказку
        if (screenPos.z < 0)
        {
            promptRoot.gameObject.SetActive(false);
            return;
        }

        // Преобразуем экранную позицию в локальную позицию внутри Canvas
        Vector2 localPoint;
        Camera uiCamera = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : cam);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out localPoint);

        promptRoot.localPosition = localPoint;

        if (!promptRoot.gameObject.activeSelf)
            promptRoot.gameObject.SetActive(true);
    }

    public void Show(Transform targetTransform)
    {
        target = targetTransform;
        if (promptRoot != null)
        {
            promptRoot.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        target = null;
        if (promptRoot != null)
            promptRoot.gameObject.SetActive(false);
    }
}
