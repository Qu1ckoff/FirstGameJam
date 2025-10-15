using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

[System.Serializable]
public class TimeSaveData
{
    public float currentTime;
    public int currentDay = 0;
    public int currentPay; // текущая сумма аренды
}

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance;

    [Header("Настройки времени")]
    public float dayDurationMinutes = 24f;
    public float currentTime = 8f;
    public int currentDay = 1;

    [Header("Аренда (PayDay)")]
    public int baseRent = 100;           // базовая аренда
    public float rentMultiplier = 1.2f;  // множитель роста аренды
    public int currentPay = 100;         // текущая сумма аренды
    private bool hasPaidToday = false;   // чтобы не списывать несколько раз в день

    [Header("Освещение и небо")]
    public Light sunLight;
    public Gradient ambientColor;
    public AnimationCurve lightIntensityCurve;

    [Header("UI")]
    private Text timeText;
    private Text dayText;
    private Text payDayText;
    private Text nextPayDayText;

    private string savePath => Path.Combine(Application.persistentDataPath, "time.json");
    private float timeScale => 24f / (dayDurationMinutes * 60f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadTime();
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
        HandlePayDay();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeScale;

        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            currentDay++;
            hasPaidToday = false;
            SaveTime();
        }
    }

    private void UpdateLighting()
    {
        if (sunLight == null) return;

        float rotation = (currentTime / 24f) * 340f;
        sunLight.transform.rotation = Quaternion.Euler(rotation - 90f, 170f, 0f);

        float t = Mathf.InverseLerp(6f, 20f, currentTime);
        Color evalColor = ambientColor.Evaluate(t);
        sunLight.color = evalColor;
        sunLight.intensity = lightIntensityCurve.Evaluate(t);
        RenderSettings.ambientLight = evalColor;
        RenderSettings.sun = sunLight;
    }

    private void HandlePayDay()
    {
        // 8:00 утра — время платежа
        if (!hasPaidToday && currentTime >= 8f && currentTime < 8.05f)
        {
            hasPaidToday = true;

            // списание денег
            MoneyManager.Instance?.AddMoney(-currentPay);
            Debug.Log($"💸 PayDay! Списано {currentPay}$");

            // показать PayDay текст
            if (payDayText != null)
                StartCoroutine(ShowPayDayText());

            // рассчитать следующую сумму аренды
            currentPay = Mathf.RoundToInt(currentPay * rentMultiplier);
            SaveTime();
        }
    }

    private IEnumerator ShowPayDayText()
    {
        payDayText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        payDayText.gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneReferences();
        UpdateLighting();
        UpdateUIText();
    }

    private void FindSceneReferences()
    {
        // TimeTxt
        var tObj = GameObject.Find("TimeTxt");
        timeText = tObj ? tObj.GetComponent<Text>() : null;

        // DayTxt
        var dObj = GameObject.Find("DayTxt");
        dayText = dObj ? dObj.GetComponent<Text>() : null;

        // PayDayTxt
        var pObj = GameObject.Find("PayDayTxt");
        payDayText = pObj ? pObj.GetComponent<Text>() : null;
        if (payDayText != null)
            payDayText.gameObject.SetActive(false);

        // NextPayDayTxt
        var nObj = GameObject.Find("NextPayDayTxt");
        nextPayDayText = nObj ? nObj.GetComponent<Text>() : null;

        // Light
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
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            timeText.text = $"{hours:00}:{minutes:00}";
        }

        if (dayText != null)
            dayText.text = $"День {currentDay}";

        if (nextPayDayText != null)
            nextPayDayText.text = $"Завтра спишут: {currentPay}$";
    }

    private void OnApplicationQuit()
    {
        SaveTime();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SaveTime();
    }

    // --- Сохранение и загрузка ---

    private void SaveTime()
    {
        TimeSaveData data = new TimeSaveData
        {
            currentTime = currentTime,
            currentDay = currentDay,
            currentPay = currentPay
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"💾 Время и PayDay сохранены: {currentTime:F2} ч, день {currentDay}, аренда {currentPay}$");
    }

    private void LoadTime()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("🕒 Нет сохранённого времени — начинаем с начала.");
            currentTime = 8f;
            currentDay = 1;
            currentPay = baseRent;
            return;
        }

        string json = File.ReadAllText(savePath);
        TimeSaveData data = JsonUtility.FromJson<TimeSaveData>(json);
        currentTime = data.currentTime;
        currentDay = data.currentDay;
        currentPay = data.currentPay > 0 ? data.currentPay : baseRent;

        Debug.Log($"⏰ Загружено время: {currentTime:F2} ч, день {currentDay}, аренда {currentPay}$");
    }
}
