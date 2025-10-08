using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;                     // игрок
    public Vector3 offset = new Vector3(0f, 3f, -6f); // смещение камеры от игрока

    [Header("Camera Settings")]
    public float followSpeed = 10f;              // скорость следовани€
    public float rotationSpeed = 2f;             // чувствительность вращени€ мыши
    public float minYAngle = -20f;               // ограничение по вертикали
    public float maxYAngle = 60f;

    [Header("Spring (Soft Follow) Settings")]
    public float positionSmoothTime = 0.15f;     // плавность движени€ (чем выше Ч тем "т€желее")
    public float rotationSmoothTime = 0.1f;      // плавность поворота
    public float springiness = 0.5f;             // сила "отставани€" при поворотах

    private float yaw;   // горизонтальный угол
    private float pitch; // вертикальный угол

    private Vector3 currentVelocity;             // вспомогательный вектор дл€ SmoothDamp
    private Quaternion targetRotation;           // желаемый поворот
    private Vector3 springOffset;                // текущий УпружинныйФ сдвиг

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // === ¬ращение только при удержании ѕ ћ ===
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed;
            pitch -= mouseY * rotationSpeed;
            pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // === ѕлавное позиционирование и УпружинаФ ===
        targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        // смещение с лЄгким "запаздыванием"
        springOffset = Vector3.Lerp(springOffset, offset, Time.deltaTime / springiness);
        Vector3 desiredPosition = target.position + targetRotation * springOffset;

        // м€гкое движение к позиции
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

        // м€гкий поворот камеры к игроку
        Quaternion lookRotation = Quaternion.LookRotation((target.position + Vector3.up * 1.5f) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / rotationSmoothTime);
    }
}
