using UnityEngine;

[ExecuteInEditMode]
public class MatchRotation : MonoBehaviour {
    [SerializeField] private Transform target;

    private void LateUpdate() {
        if (target != null)
            transform.rotation = target.rotation;
    }
}