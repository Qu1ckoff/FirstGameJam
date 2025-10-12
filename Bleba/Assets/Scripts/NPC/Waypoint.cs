using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Следующие точки, куда можно пойти из этой")]
    public List<Waypoint> nextWaypoints;

    [Tooltip("Если true — это точка входа в магазин")]
    public bool isShopEntrance = false;

    public Waypoint GetNext()
    {
        if (nextWaypoints == null || nextWaypoints.Count == 0) return null;
        return nextWaypoints[Random.Range(0, nextWaypoints.Count)];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isShopEntrance ? Color.green : Color.yellow;
        foreach (var next in nextWaypoints)
        {
            if (next != null)
                Gizmos.DrawLine(transform.position, next.transform.position);
        }
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
