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
    public Transform cameraTransform;

    [Header("Start Position (если нет сохранения)")]
    public Vector3 startPosition = Vector3.zero;
    public Vector3 startRotation = Vector3.zero;

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

        // 🧭 Загружаем сохранённую позицию игрока
        PlayerSaveManager.Load();
        var pdata = PlayerSaveManager.GetData();

        // Если сохранения нет — ставим игрока в заданное стартовое положение
        if (pdata == null || pdata.position == Vector3.zero && pdata.rotation == Quaternion.identity)
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.Euler(startRotation);
            Debug.Log($"🧍 Игрок стартует с позиции {startPosition}");
        }
        else
        {
            transform.position = pdata.position;
            transform.rotation = pdata.rotation;
            Debug.Log($"📦 Игрок восстановлен из сохранения: {pdata.position}");
        }
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

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

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
                Color low = Color.red;
                Color mid = Color.yellow;
                Color high = new Color(0f, 0.7f, 1f);

                if (percent > 0.5f)
                    staminaFill.color = Color.Lerp(mid, high, (percent - 0.5f) * 2f);
                else
                    staminaFill.color = Color.Lerp(low, mid, percent * 2f);
            }
        }
    }

    void HideUI()
    {
        if (staminaSlider != null)
        {
            GameObject slider = staminaSlider.gameObject;
            slider.SetActive(currentStamina < maxStamina);
        }
    }

    void OnDisable()
    {
        // 💾 Сохраняем позицию при выходе
        PlayerSaveManager.Save(
            transform.position,
            transform.rotation,
            "", ""
        );
    }
}
