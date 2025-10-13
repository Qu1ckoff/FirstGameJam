using UnityEngine;

public class UIHint : MonoBehaviour
{
    [Header("Подсказки для действий")]
    public GameObject hintE;
    public GameObject hintQ;
    public GameObject hintR; // 🔹 новая подсказка для NPC

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        HideHint();
    }

    public void ShowHint(Transform target, bool showE)
    {
        if (target == null) return;

        hintE.SetActive(showE);
        hintQ.SetActive(!showE);
        if (hintR != null) hintR.SetActive(false);

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        if (showE && hintE) hintE.transform.position = screenPos;
        if (!showE && hintQ) hintQ.transform.position = screenPos;
    }

    // 🔹 отдельный метод для NPC
    public void ShowRHint(Transform target)
    {
        if (target == null || hintR == null) return;

        HideHint();
        hintR.SetActive(true);
        hintR.transform.position = cam.WorldToScreenPoint(target.position);
    }

    public void HideHint()
    {
        if (hintE != null) hintE.SetActive(false);
        if (hintQ != null) hintQ.SetActive(false);
        if (hintR != null) hintR.SetActive(false);
    }
}
