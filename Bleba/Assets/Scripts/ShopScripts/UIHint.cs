using UnityEngine;
using UnityEngine.UI;

public class UIHint : MonoBehaviour
{
    public Canvas canvas;

    private Camera mainCamera;
    private Transform target;

    private void Awake()
    {
        mainCamera = Camera.main;
        canvas.gameObject.SetActive(false);
    }

    public void ShowHint(Transform targetTransform)
    {
        target = targetTransform;
        canvas.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        canvas.gameObject.SetActive(false);
        target = null;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + Vector3.up * 0.5f);
            canvas.transform.position = screenPos;
        }
    }
}
