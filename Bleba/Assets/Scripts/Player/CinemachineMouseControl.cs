using Cinemachine;
using UnityEngine;

public class CinemachineMouseControl : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;

    [Header("Настройки чувствительности камеры")]
    [Range(0.1f, 10f)] public float sensitivity = 1.25f;

    [Header("Ограничения по углам (опционально)")]
    public bool clampVertical = true;
    public float minVerticalAngle = -40f;
    public float maxVerticalAngle = 70f;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (pov == null) return;

        // Всегда активное управление камерой
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * 1f;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * 1f;

        pov.m_HorizontalAxis.Value += mouseX;
        pov.m_VerticalAxis.Value -= mouseY; // инвертируем ось Y, чтобы было привычно

        if (clampVertical)
        {
            pov.m_VerticalAxis.Value = Mathf.Clamp(pov.m_VerticalAxis.Value, minVerticalAngle, maxVerticalAngle);
        }

        // Убедимся, что скорость не ограничена
        pov.m_HorizontalAxis.m_MaxSpeed = 300f;
        pov.m_VerticalAxis.m_MaxSpeed = 300f;

        // Если игрок открыл меню (например, Time.timeScale == 0), возвращаем курсор
        if (Time.timeScale == 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
