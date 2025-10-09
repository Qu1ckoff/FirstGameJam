using UnityEngine;

public class SlimeTrail : MonoBehaviour
{
    [Header("Slime Trail Settings")]
    public GameObject slimePrefab;   // Префаб лужицы
    public float spawnInterval = 0.3f;  // как часто спавнить (сек)
    public float puddleLifetime = 5f;   // сколько живёт лужа
    public float puddleFadeTime = 1.5f; // за сколько секунд исчезает
    public Vector3 offset = new Vector3(0, 0.01f, 0); // чтобы не мерцало с землёй

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnSlime();
        }
    }

    void SpawnSlime()
    {
        if (slimePrefab == null) return;

        Vector3 spawnPos = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
            spawnPos.y = col.bounds.min.y + 0.02f;
        else
            spawnPos.y -= 0.9f;

        spawnPos += Vector3.up * offset.y;

        // Берём поворот префаба и добавляем рандомный поворот вокруг Y
        Quaternion baseRot = slimePrefab.transform.rotation;
        Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Quaternion finalRot = baseRot * randomRot;

        GameObject puddle = Instantiate(slimePrefab, spawnPos, finalRot);

        float scale = Random.Range(0.6f, 1.2f);
        puddle.transform.localScale = new Vector3(scale, scale, scale);

        StartCoroutine(FadeAndDestroy(puddle));
    }


    System.Collections.IEnumerator FadeAndDestroy(GameObject puddle)
    {
        Renderer rend = puddle.GetComponentInChildren<Renderer>();
        Color startColor = rend.material.color;
        Color endColor = startColor;
        endColor.a = 0f;

        yield return new WaitForSeconds(puddleLifetime - puddleFadeTime);

        float t = 0f;
        while (t < puddleFadeTime)
        {
            t += Time.deltaTime;
            rend.material.color = Color.Lerp(startColor, endColor, t / puddleFadeTime);
            yield return null;
        }

        Destroy(puddle);
    }
}
