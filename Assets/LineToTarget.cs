using UnityEngine;

public class LineToTarget : MonoBehaviour
{
    [SerializeField] private Orbit main;
    [SerializeField] private Orbit target;

    Vector3 distance;

    void OnDrawGizmos()
    {
        distance = (Vector3)(target.GetWorldPos() - main.GetWorldPos());
        Debug.Log(distance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, new(distance.x, distance.z, -distance.y));
        Gizmos.DrawWireSphere(new(distance.x, distance.z, -distance.y), 50f);
    }
}
