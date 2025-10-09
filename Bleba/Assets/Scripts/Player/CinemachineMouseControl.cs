using Cinemachine;
using UnityEngine;

public class CinemachineMouseControl : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachinePOV pov;
    public float vCamSpeed = 1.25f;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();
    }

    void Update()
    {
        if (pov == null) return;

        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pov.m_HorizontalAxis.m_MaxSpeed = 250f;
            pov.m_VerticalAxis.m_MaxSpeed = vCamSpeed * 100;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pov.m_HorizontalAxis.m_MaxSpeed = 0f;
            pov.m_VerticalAxis.m_MaxSpeed = 0f;
        }
    }
}

