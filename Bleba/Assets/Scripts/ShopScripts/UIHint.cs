using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIHint : MonoBehaviour
{
    [Header("Подсказки для действий")]
    public GameObject hintE;
    public GameObject hintQ;
    public GameObject hintR;
    public GameObject hintExit; // 🔹 иконка выхода

    [Header("Прогресс выхода")]
    public Image exitProgressCircle; // 🔹 UI Image с fillAmount

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        HideHint();
        if (exitProgressCircle != null)
            exitProgressCircle.fillAmount = 0f;
    }

    public void ShowHint(Transform target, bool showE)
    {
        if (target == null) return;

        HideHint();
        hintE.SetActive(showE);
        hintQ.SetActive(!showE);

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        if (showE && hintE) hintE.transform.position = screenPos;
        if (!showE && hintQ) hintQ.transform.position = screenPos;
    }

    public void ShowRHint(Transform target)
    {
        if (target == null || hintR == null) return;

        HideHint();
        hintR.SetActive(true);
        hintR.transform.position = cam.WorldToScreenPoint(target.position);
    }

    public void ShowExitHint(Transform target)
    {
        if (target == null || hintExit == null) return;

        HideHint();
        hintExit.SetActive(true);
        hintExit.transform.position = cam.WorldToScreenPoint(target.position);

        if (exitProgressCircle != null)
            exitProgressCircle.fillAmount = 0f;
    }

    public void UpdateExitProgress(float progress)
    {
        if (exitProgressCircle != null)
            exitProgressCircle.fillAmount = Mathf.Clamp01(progress);
    }

    public void HideHint()
    {
        if (hintE) hintE.SetActive(false);
        if (hintQ) hintQ.SetActive(false);
        if (hintR) hintR.SetActive(false);
        if (hintExit) hintExit.SetActive(false);
        if (exitProgressCircle != null) exitProgressCircle.fillAmount = 0f;
    }
}
