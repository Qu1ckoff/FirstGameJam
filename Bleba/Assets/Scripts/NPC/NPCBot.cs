using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCBot : MonoBehaviour
{
    public Waypoint currentWaypoint;
    public float decisionDelay = 0.5f;
    [Range(0f, 1f)] public float shopVisitChance = 0.1f;
    public float shopWaitTime = 4f;

    private NavMeshAgent agent;
    private bool goingToShop = false;
    private bool inShop = false;
    private bool isDeciding = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = 0.2f; // не в упор, а чуть раньше
        agent.autoBraking = false;     // отключаем торможение перед точкой

        if (currentWaypoint != null)
            GoToWaypoint(currentWaypoint);
    }

    void Update()
    {
        if (inShop || isDeciding || agent.pathPending) return;

        // Если почти дошли до цели
        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                StartCoroutine(DecideNextMoveSmooth());
            }
        }
    }

    IEnumerator DecideNextMoveSmooth()
    {
        isDeciding = true;
        yield return new WaitForSeconds(decisionDelay);

        // Вероятность пойти в магазин
        if (!goingToShop && Random.value < shopVisitChance)
        {
            Waypoint shopPoint = FindNearestShopEntrance();
            if (shopPoint != null)
            {
                goingToShop = true;
                GoToWaypoint(shopPoint);
                isDeciding = false;
                yield break;
            }
        }

        // Переход к следующей точке
        Waypoint next = currentWaypoint.GetNext();
        if (next != null)
        {
            GoToWaypoint(next);
        }

        isDeciding = false;
    }

    void GoToWaypoint(Waypoint wp)
    {
        currentWaypoint = wp;
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(wp.transform.position);
        }
    }

    Waypoint FindNearestShopEntrance()
    {
        Waypoint[] all = FindObjectsOfType<Waypoint>();
        Waypoint nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in all)
        {
            if (!wp.isShopEntrance) continue;

            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = wp;
            }
        }

        return nearest;
    }

    public void EnterShop()
    {
        if (!goingToShop) return;

        inShop = true;
        agent.isStopped = true;
        StartCoroutine(ShopRoutine());
    }

    public void ExitShop()
    {
        inShop = false;
        goingToShop = false;
    }

    private IEnumerator ShopRoutine()
    {
        yield return new WaitForSeconds(shopWaitTime);
        inShop = false;
        goingToShop = false;

        Waypoint nearest = FindNearestWaypoint();
        if (nearest != null)
        {
            currentWaypoint = nearest;
            agent.isStopped = false;
            GoToWaypoint(nearest);
        }
    }

    Waypoint FindNearestWaypoint()
    {
        Waypoint[] all = FindObjectsOfType<Waypoint>();
        Waypoint nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in all)
        {
            float dist = Vector3.Distance(transform.position, wp.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = wp;
            }
        }

        return nearest;
    }
}
