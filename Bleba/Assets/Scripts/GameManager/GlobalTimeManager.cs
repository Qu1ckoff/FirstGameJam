using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance;

    [Header("Настройки времени")]
    [Tooltip("Продолжительность суток в минутах реального времени.")]
    public float dayDurationMinutes = 24f;
    [Tooltip("Текущее игровое время (0–24). 8 = 8:00 утра.")]
    public float currentTime = 8f;

    [Header("Освещение и небо")]
    [Tooltip("Основной источник света (Directional Light).")]
    public Light sunLight;
    public Gradient ambientColor;
    public AnimationCurve lightIntensityCurve;

    [Header("UI")]
    private Text timeText; // обычный UnityEngine.UI.Text, не TMP

    private float timeScale => 24f / (dayDurationMinutes * 60f);

    private void Awake()
    {
        // Гарантия единственного экземпляра
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        FindSceneReferences();
    }

    private void Update()
    {
        UpdateTime();
        UpdateLighting();
        UpdateUIText();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeScale;
        if (currentTime >= 24f)
            currentTime -= 24f;
    }

    private void UpdateLighting()
    {
        if (sunLight == null) return;

        // вращаем солнце по небу
        float rotation = (currentTime / 24f) * 340f;
        sunLight.transform.rotation = Quaternion.Euler(rotation - 90f, 170f, 0f);

        // плавно меняем цвет и интенсивность
        float t = Mathf.InverseLerp(6f, 20f, currentTime);
        Color evalColor = ambientColor.Evaluate(t);
        sunLight.color = evalColor;
        sunLight.intensity = lightIntensityCurve.Evaluate(t);
        RenderSettings.ambientLight = evalColor;
        RenderSettings.sun = sunLight; // важно: Unity Lighting использует этот параметр
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        UpdateLighting(); // обновляем освещение под текущее время
    }

    private void FindSceneReferences()
    {
        // Ищем TimeTxt, если есть
        GameObject txtObj = GameObject.Find("TimeTxt");
        timeText = txtObj ? txtObj.GetComponent<Text>() : null;

        // Ищем Directional Light в новой сцене
        Light sceneLight = FindObjectOfType<Light>();
        if (sceneLight != null)
        {
            sunLight = sceneLight;
            RenderSettings.sun = sunLight;
        }
        else
        {
            Debug.LogWarning($"⚠ На сцене {SceneManager.GetActiveScene().name} нет Directional Light!");
        }
    }

    private void UpdateUIText()
    {
        if (timeText == null) return;

        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        timeText.text = $"{hours:00}:{minutes:00}";
    }
}
