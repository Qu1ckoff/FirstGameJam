using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Transition")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;
    public Text loadingText;

    [Header("UI References")]
    public GameObject persistentUIManager;
    public GameObject pauseMenuCanvas;
    public GameObject saveMessageCanvas;
    public GameObject menuCanvas;
    public GameObject pauseMenuToDisableInShop;

    private bool isPaused = false;
    private bool isTransitioning = false;
    private Coroutine loadingDotsCoroutine; // 👈 корутина для троеточий

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (persistentUIManager != null)
        {
            persistentUIManager.SetActive(false);
            DontDestroyOnLoad(persistentUIManager);
        }

        KeepUIAlive();
        EnsureEventSystem();
        HandlePersistentUIDuplicates();
    }

    void Start()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0;
            fadeCanvas.blocksRaycasts = false;
        }
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        if (persistentUIManager != null && !persistentUIManager.activeSelf)
            persistentUIManager.SetActive(true);

        HandlePersistentUIDuplicates();
        UpdateMenuCanvas();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu" && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ======================= UI =======================
    private void KeepUIAlive()
    {
        if (fadeCanvas != null)
            DontDestroyOnLoad(fadeCanvas.transform.root.gameObject);
        if (pauseMenuCanvas != null)
            DontDestroyOnLoad(pauseMenuCanvas.transform.root.gameObject);
        if (saveMessageCanvas != null)
            DontDestroyOnLoad(saveMessageCanvas.transform.root.gameObject);
        if (menuCanvas != null)
            DontDestroyOnLoad(menuCanvas.transform.root.gameObject);
    }

    private void EnsureEventSystem()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
        else if (!existing.gameObject.activeInHierarchy)
        {
            existing.gameObject.SetActive(true);
        }
    }

    private void UpdateMenuCanvas()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMainMenu = currentScene == "MainMenu";
        bool isShopScene = currentScene == "ShopScene"; // ⚠️ Название сцены магазина

        // 🔹 Показываем или скрываем меню
        if (menuCanvas != null)
            menuCanvas.SetActive(isMainMenu);

        // 🔹 Настраиваем курсор
        if (isMainMenu)
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // 🔹 Управляем кнопкой "В меню" (или другим элементом)
        if (pauseMenuToDisableInShop != null)
        {
            if (isShopScene)
                pauseMenuToDisableInShop.SetActive(false);
            else
                pauseMenuToDisableInShop.SetActive(true);
        }
    }


    private void HandlePersistentUIDuplicates()
    {
        string targetName = (persistentUIManager != null) ? persistentUIManager.name : "UIManager";
        var roots = new System.Collections.Generic.List<GameObject>();
        foreach (Transform t in FindObjectsOfType<Transform>())
        {
            if (t.parent == null && t.gameObject.name == targetName)
                roots.Add(t.gameObject);
        }

        GameObject foundPersistent = null;
        foreach (var g in roots)
        {
            if (g.scene.name == "DontDestroyOnLoad")
            {
                foundPersistent = g;
                break;
            }
        }

        if (persistentUIManager != null && foundPersistent == null)
        {
            foreach (var g in roots)
                if (g != persistentUIManager)
                    Destroy(g);
            return;
        }

        if (foundPersistent != null)
        {
            foreach (var g in roots)
                if (g != foundPersistent)
                    Destroy(g);
            if (persistentUIManager == null)
                persistentUIManager = foundPersistent;
            return;
        }

        if (roots.Count > 0 && persistentUIManager == null)
        {
            GameObject toPersist = roots[0];
            DontDestroyOnLoad(toPersist);
            persistentUIManager = toPersist;
            for (int i = 1; i < roots.Count; i++)
                Destroy(roots[i]);
        }
    }

    // ======================= Пауза =======================
    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ResetPauseState()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
    }

    // ======================= Сохранение =======================
    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠ Игрок не найден для сохранения!");
            return;
        }

        // 🔹 Сохраняем полные данные игрока
        var leftItem = player.GetComponent<PlayerPickupSystem>()?.GetLeftItemID();
        var rightItem = player.GetComponent<PlayerPickupSystem>()?.GetRightItemID();
        PlayerSaveManager.Save(player.transform.position, player.transform.rotation, leftItem, rightItem);

        // 🔹 Сохраняем общие данные сцены
        GameData data = new GameData(SceneManager.GetActiveScene().name, player.transform.position);
        SaveSystem.Save(data);

        Debug.Log($"💾 Сохранена игра: {SceneManager.GetActiveScene().name}, позиция {player.transform.position}");
        ShowSaveMessage();
    }


    private void ShowSaveMessage()
    {
        if (saveMessageCanvas == null) return;
        CanvasGroup cg = saveMessageCanvas.GetComponent<CanvasGroup>();
        if (cg != null)
            StartCoroutine(FadeSaveMessage(cg));
    }

    private IEnumerator FadeSaveMessage(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(1.5f);
        float t = 0f;
        while (t < 1f)
        {
            cg.alpha = Mathf.Lerp(1, 0, t);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        cg.alpha = 0;
        cg.gameObject.SetActive(false);
    }

    // ======================= Сцены =======================
    public void StartGame() => LoadScene("GameScene");
    public void LoadMainMenu()
    {
        SaveGame(); // 💾 Сохраняем перед выходом
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        LoadScene("MainMenu");
    }

    public void QuitGame() => Application.Quit();
    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(LoadSceneGeneric(sceneName));
    }

    public void LoadGame()
    {
        GameData data = SaveSystem.Load();
        if (data == null)
        {
            LoadScene("GameScene");
            return;
        }
        if (!isTransitioning)
            StartCoroutine(LoadSceneGeneric(data.sceneName, data.GetPlayerPosition()));
    }

    // ======================= Универсальная загрузка =======================
    private IEnumerator LoadSceneGeneric(string sceneName, Vector3? playerPos = null)
    {
        isTransitioning = true;

        // 🔹 Сохраняем данные игрока перед переходом
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var leftItem = player.GetComponent<PlayerPickupSystem>()?.GetLeftItemID();
            var rightItem = player.GetComponent<PlayerPickupSystem>()?.GetRightItemID();
            PlayerSaveManager.Save(player.transform.position, player.transform.rotation, leftItem, rightItem);
        }

        // 1️⃣ Показываем экран загрузки
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1;
            fadeCanvas.blocksRaycasts = true;
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            if (loadingDotsCoroutine != null)
                StopCoroutine(loadingDotsCoroutine);
            loadingDotsCoroutine = StartCoroutine(AnimateLoadingText());
        }

        yield return null;

        // 2️⃣ Загружаем сцену асинхронно
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        // 3️⃣ После загрузки настраиваем UI
        EnsureEventSystem();
        HandlePersistentUIDuplicates();
        UpdateMenuCanvas();
        if (sceneName != "MainMenu")
            ResetPauseState();

        // 4️⃣ Восстанавливаем позицию игрока, если есть данные
        if (playerPos.HasValue)
        {
            yield return null;
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = playerPos.Value;
        }

        // 5️⃣ Завершаем загрузку — убираем экран и анимацию
        yield return new WaitForSecondsRealtime(1f);

        if (loadingDotsCoroutine != null)
        {
            StopCoroutine(loadingDotsCoroutine);
            loadingDotsCoroutine = null;
        }

        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        // 6️⃣ Плавное исчезновение фейда
        float t = 0f;
        while (t < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;
        isTransitioning = false;
    }

    // ======================= Анимация троеточий =======================
    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Загрузка";
        int dotCount = 0;
        while (true)
        {
            loadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }
}
