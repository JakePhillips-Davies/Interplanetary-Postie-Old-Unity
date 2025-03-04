using UnityEngine;

[ExecuteInEditMode]
public class AutoCentre : MonoBehaviour
{
    [SerializeField] private bool centreInEditMode;
    [SerializeField] private GameObject focus;

    private void FixedUpdate() {
        transform.position -= focus.transform.position;
    }

    private void Update() {
        if (centreInEditMode && !Application.isPlaying) {
            transform.position -= focus.transform.position;
        }
    }
}
