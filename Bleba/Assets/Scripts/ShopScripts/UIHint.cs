using UnityEngine;

public class UIHint : MonoBehaviour
{
    [Header("Подсказки для действий")]
    public GameObject hintE; // Взять (E)
    public GameObject hintQ; // Положить/Бросить (Q)

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        HideHint();
    }

    /// <summary>
    /// Показываем подсказку на объекте target. showE = true -> E, false -> Q
    /// </summary>
    public void ShowHint(Transform target, bool showE)
    {
        if (target == null) return;

        hintE.SetActive(showE);
        hintQ.SetActive(!showE);

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        if (showE && hintE != null)
            hintE.transform.position = screenPos;

        if (!showE && hintQ != null)
            hintQ.transform.position = screenPos;
    }

    /// <summary>
    /// Скрыть все подсказки
    /// </summary>
    public void HideHint()
    {
        if (hintE != null) hintE.SetActive(false);
        if (hintQ != null) hintQ.SetActive(false);
    }
}
