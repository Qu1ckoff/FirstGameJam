using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDigest : MonoBehaviour
{
    [Header("Digest System")]
    public float stomachCapacity = 100f;
    [SerializeField] private float currentStomach = 0f;
    public float CurrentStomach => currentStomach;
    public Transform poopSpawnPoint;
    public KeyCode eatKey = KeyCode.E;

    [Header("Detection Settings")]
    public float detectionRadius = 1.5f;
    public float detectionDistance = 1.5f;
    public LayerMask detectionMask;

    [Header("Аудио эффекты")]
    public AudioClip eatSound;
    public AudioClip poopSound;
    public AudioClip fullStomachSound;
    private AudioSource audioSource;

    [Header("💨 Визуальные эффекты")]
    public GameObject fartEffectPrefab; // 🔥 сюда можно назначить эффект пука (ParticleSystem)
    public float fartEffectLifetime = 3f; // время жизни эффекта

    private bool isEating = false;
    private Eatable currentTarget;
    private List<Coroutine> digestionQueue = new List<Coroutine>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        DetectEatable();
        HandleEating();
    }

    void DetectEatable()
    {
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Collider[] hits = Physics.OverlapSphere(center, detectionRadius, detectionMask);

        Eatable found = null;

        foreach (var h in hits)
        {
            var e = h.GetComponent<Eatable>();
            if (e != null && e.canBeEaten)
            {
                found = e;
                break;
            }
        }

        if (found != currentTarget)
        {
            if (currentTarget != null)
            {
                currentTarget.Highlight(false);
                EatPromptUI.Instance.Hide();
            }

            if (found != null)
            {
                Debug.Log($"👀 Обнаружен съедобный объект: {found.trashName}");
                found.Highlight(true);
                EatPromptUI.Instance.Show(found.transform);
            }

            currentTarget = found;
        }

        if (found == null && currentTarget != null)
        {
            Debug.Log("🔭 В зоне нет съедобных объектов");
            currentTarget.Highlight(false);
            EatPromptUI.Instance.Hide();
            currentTarget = null;
        }
    }

    void HandleEating()
    {
        if (currentTarget == null || isEating) return;

        if (Input.GetKeyDown(eatKey))
        {
            Debug.Log($"🍽️ Начато поедание: {currentTarget.trashName}");
            StartCoroutine(EatRoutine(currentTarget));
        }
    }

    IEnumerator EatRoutine(Eatable target)
    {
        isEating = true;
        float timer = 0f;

        while (timer < target.eatTime)
        {
            if (!Input.GetKey(eatKey))
            {
                Debug.Log($"⏹️ Поедание {target.trashName} прервано");
                isEating = false;
                EatPromptUI.Instance.SetProgress(0);
                yield break;
            }

            timer += Time.deltaTime;
            EatPromptUI.Instance.SetProgress(timer / target.eatTime);
            yield return null;
        }

        Debug.Log($"✅ Закончил есть: {target.trashName}");
        EatPromptUI.Instance.SetProgress(0);
        EatPromptUI.Instance.Hide();

        Eat(target);
        isEating = false;
    }

    void Eat(Eatable target)
    {
        if (currentStomach + target.stomachLoad > stomachCapacity)
        {
            Debug.Log("⚠️ Желудок переполнен!");
            PlaySound(fullStomachSound);
            return;
        }

        Debug.Log($"😋 Съедено: {target.trashName}. Добавляем {target.stomachLoad} в желудок");
        currentStomach += target.stomachLoad;
        target.canBeEaten = false;

        PlaySound(eatSound);

        Destroy(target.gameObject);
        Debug.Log($"🗑️ {target.trashName} удалён со сцены");

        Debug.Log($"🤢 Начинается переваривание {target.trashName} ({target.digestionTime} сек)");
        Coroutine digestion = StartCoroutine(DigestRoutine(target));
        digestionQueue.Add(digestion);
    }

    IEnumerator DigestRoutine(Eatable target)
    {
        yield return new WaitForSeconds(target.digestionTime);

        Debug.Log($"💩 Переварено: {target.trashName}");

        if (poopSpawnPoint != null)
        {
            // 💩 Спавним еду
            target.SpawnResultFoods(poopSpawnPoint.position);

            // 💨 Спавним эффект пука
            if (fartEffectPrefab != null)
            {
                GameObject fartFX = Instantiate(fartEffectPrefab, poopSpawnPoint.position, Quaternion.identity);
                Destroy(fartFX, fartEffectLifetime);
                Debug.Log("💨 Эффект пука создан!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Не задан poopSpawnPoint!");
        }

        currentStomach -= target.stomachLoad;
        currentStomach = Mathf.Max(0, currentStomach);

        PlaySound(poopSound);

        Debug.Log($"🧮 Желудок: {currentStomach}/{stomachCapacity}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
