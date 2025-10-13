using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechBubble : MonoBehaviour
{
    [Header("Настройки пузыря")]
    public GameObject bubbleObject; // фоновый GameObject (Image и т.д.)
    public Text speechText;         // обычный UI.Text (не TMP)
    public float defaultDisplayTime = 3f; // время по умолчанию (если нужно)

    private Coroutine hideRoutine;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        if (bubbleObject != null)
            bubbleObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (bubbleObject != null && bubbleObject.activeSelf && cam != null)
        {
            // Поворачиваем пузырь к камере (спрайт лицом к камере)
            bubbleObject.transform.LookAt(bubbleObject.transform.position + cam.transform.rotation * Vector3.forward);
        }
    }

    /// <summary>
    /// Показать фразу.
    /// Если displayTime &gt; 0 — показываем заданное время, затем скрываем.
    /// Если displayTime == 0 — показываем постоянно до вызова Hide().
    /// Если displayTime &lt; 0 — используем defaultDisplayTime.
    /// </summary>
    public void ShowPhrase(string phrase, float displayTime = -1f)
    {
        if (bubbleObject == null || speechText == null)
        {
            Debug.LogWarning("SpeechBubble: bubbleObject или speechText не назначены.");
            return;
        }

        speechText.text = phrase;
        bubbleObject.SetActive(true);

        // Отмена старого скрытия
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        float dt = displayTime;
        if (displayTime < 0f) dt = defaultDisplayTime;

        if (Mathf.Approximately(dt, 0f))
        {
            // 0 — показываем постоянно (пока не вызван HidePhrase)
            return;
        }
        else
        {
            hideRoutine = StartCoroutine(HideAfter(dt));
        }
    }

    IEnumerator HideAfter(float t)
    {
        yield return new WaitForSeconds(t);
        HidePhrase();
        hideRoutine = null;
    }

    public void HidePhrase()
    {
        if (bubbleObject != null) bubbleObject.SetActive(false);

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }
}
