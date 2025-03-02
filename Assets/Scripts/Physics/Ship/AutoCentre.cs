using UnityEngine;
using UnityEngine.Experimental.AI;

[ExecuteInEditMode]
public class AutoCentre : MonoBehaviour
{
    [SerializeField] private bool centreInEditMode;

    private void FixedUpdate() {
        Transform world = CelestialPhysics.get_singleton().transform;

        world.position -= transform.position;
    }

    private void OnValidate() {
        if (centreInEditMode) {
            Transform world = CelestialPhysics.get_singleton().transform;

            world.position -= transform.position;
        }
    }
}
