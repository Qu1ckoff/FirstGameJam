using UnityEngine;
using UnityEngine.UI;

public class EatPromptUI : MonoBehaviour
{
    public static EatPromptUI Instance;

    [Header("UI References")]
    public RectTransform promptTransform;
    public Image fillImage;

    [Header("Settings")]
    public Vector3 worldOffset = new Vector3(0, 1.0f, 0); // 👈 стало чуть ниже

    private Camera mainCam;
    private Transform target;

    void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        Hide();
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(target.position + worldOffset);

            if (screenPos.z > 0)
                promptTransform.position = screenPos;
            else
                Hide(); // объект вне камеры
        }
    }

    public void Show(Transform newTarget)
    {
        if (target == newTarget) return;

        target = newTarget;
        promptTransform.gameObject.SetActive(true);
        SetProgress(0);
    }

    public void Hide()
    {
        if (promptTransform != null)
            promptTransform.gameObject.SetActive(false);
        target = null;
    }

    public void SetProgress(float progress)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(progress);
    }
}