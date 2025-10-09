using UnityEngine;
using UnityEngine.UI;

public class StomachUI : MonoBehaviour
{
    public static StomachUI Instance;

    [Header("UI References")]
    public Image fillImage;

    [Header("Visual Settings")]
    public float fillLerpSpeed = 5f;
    public Color emptyColor = new Color(0.2f, 0.8f, 0.2f);
    public Color fullColor = new Color(0.8f, 0.3f, 0.1f);

    [Header("References")]
    public PlayerDigest digestSystem;

    void Awake()
    {
        Instance = this;

        if (digestSystem == null)
            digestSystem = FindObjectOfType<PlayerDigest>();
    }

    void Update()
    {
        if (digestSystem == null || fillImage == null) return;

        float current = digestSystem.CurrentStomach;
        float max = digestSystem.stomachCapacity;

        float targetPercent = Mathf.Clamp01(current / max);

        // плавный переход заливки
        float lerped = Mathf.Lerp(fillImage.fillAmount, targetPercent, Time.deltaTime * fillLerpSpeed);
        fillImage.fillAmount = lerped;

        // плавный переход цвета
        fillImage.color = Color.Lerp(emptyColor, fullColor, lerped);
    }
}
