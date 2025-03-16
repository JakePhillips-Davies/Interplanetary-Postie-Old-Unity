using System;
using UnityEngine;

public class ScaleSpaceBody : MonoBehaviour {
    
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    private float scalar;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        scalar =  ScaleSpaceSingleton.Get.GetSpaceScale();

        if (CelestialPhysicsSingleton.get_singleton().SOIGizmo) Gizmos.DrawWireSphere(transform.position, (float)refOrbit.get_influence_radius() * scalar);

        if (CelestialPhysicsSingleton.get_singleton().VelGizmo) {
            Vector3d localVel = refOrbit.GetLocalVel();
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)new Vector3d(localVel.x, localVel.z, -localVel.y) * scalar * 100000);
        }
    }

    public void SetOrbit(Orbit orbit) { refOrbit = orbit; }
}