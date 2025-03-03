using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Orbit), typeof(LineRenderer))]
[ExecuteInEditMode]
public class OrbitManager : MonoBehaviour
{
    //
    [SerializeField] bool patching = false;

    private Orbit orbit;

    /// <summary>
    /// Local to parent
    /// </summary>
    private Vector3[] orbitPoints;
    [SerializeField] int orbitDetail = 100;
    [SerializeField] Color orbitColour = Color.red;

    /// <summary>
    /// The point at which an orbit's lines vanish
    /// </summary>
    double minOrbitDisplayDist;
    /// <summary>
    /// The point beyond which an orbit's lines are their max size
    /// </summary>
    double maxOrbitDisplayDist;

    LineRenderer lineRenderer;
    //

    private void Update() {
        if (!Application.isPlaying){
            lineRenderer = gameObject.GetComponent<LineRenderer>();

            orbit = transform.GetComponent<Orbit>();
            
            GetOrbitPoints();
            DrawOrbit();
        }
    }
    private void Start() {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        orbit = transform.GetComponent<Orbit>();
    }

    private void FixedUpdate() {

        if (patching) orbit.patch_conics();
        orbit._physics_process(Time.fixedDeltaTime * CelestialPhysics.get_singleton().get_time_scale());
        
        GetOrbitPoints();
        DrawOrbit();
    }



    void GetOrbitPoints() {
        double timeStep = 2 * Math.PI / orbitDetail;
        double timeStepped = 0;
        orbitPoints = new Vector3[orbitDetail];
        Vector3d startLocalPos = orbit.getLocalPos();

        for(int i = 0; i < orbitDetail; i++) {

            orbit._orbit_process(timeStep);
            timeStepped += timeStep;

            Vector3d localPos = orbit.getLocalPos() - startLocalPos;

            orbitPoints[i] = (Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * CelestialPhysics.get_singleton().get_spaceScale());

        }

        orbit._orbit_process(-timeStepped);
    }

    void DrawOrbit() {
        minOrbitDisplayDist = orbit.get_influence_radius();
        maxOrbitDisplayDist = minOrbitDisplayDist * 10;

        float maxOrbitWidth = (float)(orbit.get_periapsis() * CelestialPhysics.get_singleton().get_spaceScale()) / 100;

        Vector3 camPos;
        if (CelestialPhysics.get_singleton().OrbitDisplayDistanceByEditorCam) camPos =  SceneView.lastActiveSceneView.camera.transform.position;
        else camPos = Camera.main.transform.position;

        float distance = (transform.position - camPos).magnitude - ((float)minOrbitDisplayDist * CelestialPhysics.get_singleton().get_spaceScale());

        float max = (float)(maxOrbitDisplayDist - minOrbitDisplayDist) * CelestialPhysics.get_singleton().get_spaceScale();
        float width = distance / max;
        width = Math.Clamp(width, 0.0f, 1.0f) * maxOrbitWidth;


        lineRenderer.useWorldSpace = false;
        lineRenderer.material = CelestialPhysics.get_singleton().getLineMat();
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.loop = true;
        lineRenderer.enabled = true;

        lineRenderer.positionCount = orbitDetail;
        lineRenderer.SetPositions (orbitPoints);

        lineRenderer.startColor = orbitColour;
        lineRenderer.endColor = orbitColour;
        lineRenderer.widthMultiplier = width;
        
    }

}
