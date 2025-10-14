using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MoneySaveData
{
    public int money;
}

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    [Header("UI")]
    public Text moneyText; // обычный UI Text

    [Header("Settings")]
    public int money = 0;

    private string savePath => Path.Combine(Application.persistentDataPath, "money.json");

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMoney();
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    private void OnEnable()
    {
        // Подписка на событие смены сцены
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SaveMoney();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"💵 Деньги: {money}");
        UpdateMoneyUI();
        SaveMoney();
    }

    public void SetMoney(int amount)
    {
        money = amount;
        UpdateMoneyUI();
        SaveMoney();
    }
    private void UpdateMoneyUI()
    {
        // Если UI текст не задан вручную, ищем его
        if (moneyText == null)
        {
            GameObject go = GameObject.Find("MoneyTxt");
            if (go != null)
                moneyText = go.GetComponent<Text>();
        }

        if (moneyText != null)
            moneyText.text = money.ToString() + " $";
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Обновляем UI после загрузки сцены
        moneyText = null; // сбрасываем ссылку, чтобы заново найти текст
        UpdateMoneyUI();
    }

    private void SaveMoney()
    {
        MoneySaveData data = new MoneySaveData { money = this.money };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("💾 Деньги сохранены");
    }

    private void LoadMoney()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        MoneySaveData data = JsonUtility.FromJson<MoneySaveData>(json);
        money = data.money;
        Debug.Log($"💰 Деньги загружены: {money}");
    }
}
