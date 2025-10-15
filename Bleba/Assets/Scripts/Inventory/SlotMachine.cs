using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SlotMachine : MonoBehaviour
{
    [Header("Основные настройки")]
    public List<float> multipliers = new List<float>() { 0.1f, 0.25f, 0.5f, 1f, 2f, 5f };
    public float spinDuration = 6f;
    public int baseBet = 50;

    [Header("UI элементы")]
    public GameObject uiPanel;
    public Text betText;
    public Text moneyText;
    public Text[] reelTexts;

    [Header("Подсказка")]
    public RectTransform pressQHintUI;
    public Vector3 hintOffset = new Vector3(0, 2f, 0);

    [Header("Игрок")]
    public string playerTag = "Player";

    [Header("Аудио")]
    public AudioClip spinSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private AudioSource audioSource;
    private bool playerNearby = false;
    private bool isSpinning = false;
    private int currentBet;
    private float currentMultiplier = 1f;

    private void Start()
    {
        currentBet = baseBet;
        uiPanel.SetActive(false);
        if (pressQHintUI != null)
            pressQHintUI.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        UpdateBetText();
        UpdateMoneyText();
    }

    private void Update()
    {
        // 🔹 Обновляем подсказку над автоматом
        if (pressQHintUI != null && pressQHintUI.gameObject.activeSelf)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + hintOffset);
            pressQHintUI.position = screenPos;
        }

        // 🔹 Взаимодействие
        if (playerNearby)
        {
            if (!uiPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
            {
                uiPanel.SetActive(true);
                UpdateMoneyText();
            }

            if (uiPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    DecreaseBet();
                if (Input.GetKeyDown(KeyCode.E))
                    IncreaseBet();
                if (Input.GetKeyDown(KeyCode.Space))
                    Spin();
                if (Input.GetKeyDown(KeyCode.Escape))
                    uiPanel.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
            if (pressQHintUI != null)
                pressQHintUI.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
            if (pressQHintUI != null)
                pressQHintUI.gameObject.SetActive(false);
            uiPanel.SetActive(false);
        }
    }

    public void IncreaseBet()
    {
        currentBet += 10;
        UpdateBetText();
    }

    public void DecreaseBet()
    {
        currentBet = Mathf.Max(baseBet, currentBet - 10);
        UpdateBetText();
    }

    private void UpdateBetText()
    {
        if (betText != null)
            betText.text = $"Ставка: {currentBet}$";
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null && MoneyManager.Instance != null)
            moneyText.text = $"Баланс: {MoneyManager.Instance.money}$";
    }

    public void Spin()
    {
        if (isSpinning) return;

        if (MoneyManager.Instance.money < currentBet)
        {
            Debug.Log("❌ Недостаточно денег для ставки!");
            return;
        }

        MoneyManager.Instance.AddMoney(-currentBet);
        UpdateMoneyText();
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        if (spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float startTime = Time.realtimeSinceStartup;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed = Time.realtimeSinceStartup - startTime;

            // 🔹 Делаем плавное замедление — от 0.05f до 0.3f
            float t = elapsed / spinDuration;
            float delay = Mathf.Lerp(0.05f, 0.3f, t);

            for (int i = 0; i < reelTexts.Length; i++)
            {
                float randomMult = multipliers[Random.Range(0, multipliers.Count)];
                reelTexts[i].text = $"{randomMult}x";
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        // 🔹 Останавливаем вращение и звук
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // 🔹 Выбираем финальный множитель
        currentMultiplier = multipliers[Random.Range(0, multipliers.Count)];
        for (int i = 0; i < reelTexts.Length; i++)
            reelTexts[i].text = $"{currentMultiplier}x";

        // 🔹 Рассчитываем награду
        int reward = Mathf.RoundToInt(currentBet * currentMultiplier);

        // 🔹 Проигрыш, если множитель < 1
        if (currentMultiplier < 1f)
        {
            if (loseSound != null)
                audioSource.PlayOneShot(loseSound);

            MoneyManager.Instance.AddMoney(reward);
            Debug.Log($"🎰 Выпало {currentMultiplier}x — проигрыш.");
        }
        else
        {
            if (winSound != null)
                audioSource.PlayOneShot(winSound);

            MoneyManager.Instance.AddMoney(reward);
            Debug.Log($"🎰 Выпало {currentMultiplier}x! Получено {reward}$");
        }

        UpdateMoneyText();
        isSpinning = false;
    }
}
