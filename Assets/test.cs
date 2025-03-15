using UnityEngine;

public class test : MonoBehaviour
{
    private Vector3 prevPos = Vector3.zero;
    private Vector3 prevLPos = Vector3.zero;
    void FixedUpdate()
    {
        Debug.Log((transform.localPosition - prevPos).magnitude);
        // Debug.Log((transform.localPosition).y);
        // Debug.Log((transform.localPosition).z);
        Debug.Log("----------------------------------------");

        prevLPos = transform.localPosition;
        prevPos = transform.position;
    }
}
