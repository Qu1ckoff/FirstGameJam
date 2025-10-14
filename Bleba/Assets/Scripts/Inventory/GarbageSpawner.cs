using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints;       // Точки спавна
    public List<GameObject> garbagePrefabs;   // Список мусора
    public float spawnInterval = 120f;        // Интервал между спавнами в секундах

    private void Start()
    {
        if (spawnPoints.Count == 0 || garbagePrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ Не заданы точки спавна или префабы мусора!");
            return;
        }

        // Спавн сразу при старте
        SpawnGarbage();

        // Запускаем цикл спавна
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnGarbage();
        }
    }

    private void SpawnGarbage()
    {
        // Выбираем случайную точку
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Выбираем случайный мусор
        GameObject prefab = garbagePrefabs[Random.Range(0, garbagePrefabs.Count)];

        // Спавним объект
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log($"🗑️ Спавн мусора: {prefab.name} в точке {spawnPoint.name}");
    }

    // Для наглядности в сцене
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
                Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}
