using UnityEngine;
using UnityEngine.UI;

public class BoxInventoryUI : MonoBehaviour
{
    public static BoxInventoryUI Instance;

    [Header("UI References")]
    public RectTransform uiRoot;      // корневой объект
    public Text countText;            // текст вместимости
    public GameObject actionPanel;    // панель кнопок
    public GameObject eButton;        // кнопка "E" Ч достать
    public GameObject qButton;        // кнопка "Q" Ч положить
    public GameObject rButton;        // кнопка "R" Ч прив€зать/отв€зать

    [Header("Positioning")]
    public Vector3 worldOffset = new Vector3(0, 2f, 0);

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Transform target;

    void Awake()
    {
        Instance = this;
        parentCanvas = uiRoot.GetComponentInParent<Canvas>();
        canvasRect = parentCanvas.transform as RectTransform;
        uiRoot.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (uiRoot.gameObject.activeSelf)
                uiRoot.gameObject.SetActive(false);
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            uiRoot.gameObject.SetActive(false);
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localPoint
        );

        uiRoot.localPosition = localPoint;
        if (!uiRoot.gameObject.activeSelf)
            uiRoot.gameObject.SetActive(true);
    }

    // --- основной метод ---
    public void Show(Transform box, int current, int max, bool showR = true, bool attached = false)
    {
        target = box;
        uiRoot.gameObject.SetActive(true);
        UpdateCount(current, max);

        if (actionPanel != null)
        {
            // ≈сли коробка прив€зана, то панель включаетс€ только когда "смотрим" на неЄ
            actionPanel.SetActive(!attached || showR);
        }

        if (rButton != null)
            rButton.SetActive(showR);
    }

    public void UpdateCount(int current, int max)
    {
        if (countText != null)
            countText.text = $"{current} / {max}";
    }

    public void Hide()
    {
        target = null;
        uiRoot.gameObject.SetActive(false);
    }

    // --- новый метод: скрывает только панель кнопок ---
    public void HideActionPanelOnly()
    {
        if (actionPanel != null)
            actionPanel.SetActive(false);
    }

    // --- новый метод: показать только панель действий (не трога€ корневой uiRoot и текст) ---
    public void ShowActionPanelOnly(bool showR = true)
    {
        if (actionPanel == null) return;

        actionPanel.SetActive(true);

        // если есть отдельна€ кнопка R Ч решаем, показывать ли еЄ
        if (rButton != null)
            rButton.SetActive(showR);
    }

}
