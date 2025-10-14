using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.8f;
    public float rotationSpeed = 10f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 1.5f;

    [Header("UI")]
    public Slider staminaSlider;
    public Image staminaFill;

    [Header("References")]
    public Transform cameraTransform; // 🔹 ссылка на камеру

    private float currentStamina;
    private float staminaTimer;
    private bool isSprinting;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentStamina = maxStamina;

        if (staminaSlider != null)
            staminaSlider.value = 1f;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Загружаем позицию игрока
        PlayerSaveManager.Load();
        var pdata = PlayerSaveManager.GetData();
        transform.position = pdata.position;
        transform.rotation = pdata.rotation;
    }

    void Update()
    {
        MoveInput();
        HandleStamina();
        UpdateUI();
        HideUI();
    }

    void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    void MoveInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 🔹 движение в локальных координатах камеры
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // убираем влияние наклона камеры по вертикали
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // направление движения = вперёд камеры * W/S + вправо камеры * A/D
        moveDirection = (camForward * v + camRight * h).normalized;
    }

    void MovePlayer()
    {
        float currentSpeed = moveSpeed;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && moveDirection.magnitude > 0;

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
            currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            staminaTimer = 0f;
        }

        Vector3 move = moveDirection * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void RotatePlayer()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void HandleStamina()
    {
        if (!isSprinting && currentStamina < maxStamina)
        {
            staminaTimer += Time.deltaTime;
            if (staminaTimer >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
    }

    void UpdateUI()
    {
        if (staminaSlider != null)
        {
            float percent = currentStamina / maxStamina;
            staminaSlider.value = percent;

            if (staminaFill != null)
            {
                // создаём градиент от голубого -> жёлтого -> красного
                Color low = Color.red;
                Color mid = Color.yellow;
                Color high = new Color(0f, 0.7f, 1f);

                if (percent > 0.5f)
                {
                    staminaFill.color = Color.Lerp(mid, high, (percent - 0.5f) * 2f);
                }
                else
                {
                    staminaFill.color = Color.Lerp(low, mid, percent * 2f);
                }
            }
        }
    }

    void HideUI()
    {
        if (staminaSlider != null)
        {
            GameObject slider = staminaSlider.gameObject;
            if (currentStamina == maxStamina)
            {
                slider.SetActive(false);
            }
            else
            {
                slider.SetActive(true);
            }
        }
    }
    void OnDisable()
    {
        // 💾 Сохраняем данные игрока при выгрузке сцены
        if (!ShopShelfSpawner.isExitingScene) // если используешь общий флаг выхода
        {
            PlayerSaveManager.Save(
                transform.position,
                transform.rotation,
                "", "" // можно добавить сюда ID предметов в руках, если нужно
            );
        }
    }
}
