using System;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleSpaceBody : MonoBehaviour {
    
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    [field: SerializeField] public CelestialObject refCO { get; private set; }
    [field: SerializeField] public OrbitLineRenderer orbitLineRenderer { get; private set; }
    private float scalar;

    private void Start() {
        if (orbitLineRenderer != null)
            refCO.SetOrbitLineRenderer(refCO.scaleSpaceBody.orbitLineRenderer);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        scalar =  ScaleSpaceSingleton.Get.GetSpaceScale();

        if (SpaceControllerSingleton.Get.SOIGizmo) Gizmos.DrawWireSphere(transform.position, (float)refOrbit.get_influence_radius() * scalar);

        if (SpaceControllerSingleton.Get.VelGizmo) {
            Vector3d localVel = refOrbit.GetLocalVel();
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)new Vector3d(localVel.x, localVel.z, -localVel.y) * scalar * 100000);
        }
    }

    public void SetOrbit(Orbit orbit) { refOrbit = orbit; }
    public void SetCelestialObject(CelestialObject co) { refCO = co; }
}