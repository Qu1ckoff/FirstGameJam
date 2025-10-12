using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public float minVertical = -30f; // вниз
    public float maxVertical = 60f;  // вверх
    public float minHorizontal = -45f; // влево
    public float maxHorizontal = 45f;  // вправо

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Vector3 angles = transform.localEulerAngles;
        horizontalRotation = angles.y;
        verticalRotation = angles.x;
        if (verticalRotation > 180) verticalRotation -= 360; // корректировка
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        verticalRotation = Mathf.Clamp(verticalRotation, minVertical, maxVertical);
        horizontalRotation = Mathf.Clamp(horizontalRotation, minHorizontal, maxHorizontal);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }
}
