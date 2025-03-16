using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Orbit), typeof(LineRenderer))]
public class OrbitManager : MonoBehaviour
{
    //
    [SerializeField] bool patching = false;

    public Orbit orbit { get; private set; }

    /// <summary>
    /// Local to parent
    /// </summary>
    private List<Vector3> orbitPoints;
    [SerializeField] int orbitDetail = 100;
    [SerializeField] Color orbitColour = Color.red;

    LineRenderer lineRenderer;
    List<Orbit> celestialBodies;

    int patchDepth;
    //

    public void EditorUpdate() { // for drawing orbits in editor
        if (!Application.isPlaying && transform.parent.TryGetComponent<Orbit>(out var a)){
            Setup();
            orbit.EditorUpdate();
            DrawOrbit();
        }
    }
    private void Start() {
        Setup();
    }
    
    public void Setup() {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        orbit = transform.GetComponent<Orbit>();
    }
    public void DrawOrbit() {
        if (patching) GetOrbitPointsPatched();
        else GetOrbitPoints();
        DrawOrbitPoints();
    }
    public void ProcessOrbit(double time) {
        orbit._physics_process(time);
        if (patching) orbit.patch_conics();

        DrawOrbit();
    }

    /// <summary>
    /// Intended for use when predicting orbit lines
    /// </summary>
    /// <param name="time"></param>
    public void ProcessOrbitGhost(double time) {
        orbit._physics_process(time, false);
        if (patching) 
            if (orbit.patch_conics()) {
                orbit.SetOrbitStartTime(time);
                patchDepth++;
            }
    }





    /*
            Draw those orbits!
    */
    private void GetOrbitPointsPatched() {
        if (this.orbit.get_eccentricity() < 0) return;

        Orbit.OrbitInfo orbitInfo = new(){
            mass = orbit.get_mass(),
            mu = orbit.get_mu(),
            periapsis = orbit.get_periapsis(),
            eccentricity = orbit.get_eccentricity(),
            longitude_of_ascending_node = orbit.get_longitude_of_ascending_node(),
            longitude_of_perigee = orbit.get_longitude_of_perigee(),
            inclination = orbit.get_inclination(),
            clockwise = orbit.get_clockwise(),
            true_anomaly = orbit.get_true_anomaly(),
            mean_anomaly = orbit.get_mean_anomaly(),
            orbitStartTime = orbit.GetOrbitStartTime(),
            localPos = orbit.GetLocalPos(),
            localVel = orbit.GetLocalVel(),
            parent = orbit.transform.parent
        };

        double start = orbit.get_true_anomaly();
        double step = start;

        double startTime = UniversalTimeSingleton.Get.time;
        
        orbitPoints = new();
        Vector3d startLocalPos = orbit.GetLocalPos();

        Orbit parent = orbit.transform.parent.GetComponent<Orbit>();

        patchDepth = 1;

        while (patchDepth <= CelestialPhysicsSingleton.get_singleton().patchDepthLimit)
        {   

            if (orbit.get_eccentricity() < 1) {
    
                double stepSize = (2 * Mathd.PI / orbitDetail) / orbit.get_mean_motion_from_keplerian();

                for(int i = 0; i <= orbitDetail; i++) {

                    int currentPatch = patchDepth;

                    CelestialPhysicsSingleton.get_singleton().ProcessCelestialPhysics(startTime + step);
                    step += stepSize;

                    if(patchDepth != currentPatch) { break; }

                    Vector3d relativeToOGParent = orbit.GetWorldPos() - parent.GetWorldPos();
                    Vector3d localPos = relativeToOGParent - startLocalPos;
                    orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));
    
                    if (i == (orbitDetail - 1)) patchDepth += CelestialPhysicsSingleton.get_singleton().patchDepthLimit;

                }
        
    
            } else {
    
    			double endTrueAnomaly = Mathd.Acos( -1 / orbit.get_eccentricity());
    			double range = endTrueAnomaly - start;
    			double stepSize = (range / (orbitDetail - 1)) / orbit.get_mean_motion_from_keplerian();
    
    			for (int i = 0; i < orbitDetail; i++) {

                    CelestialPhysicsSingleton.get_singleton().ProcessCelestialPhysics(startTime + step);
                    step += stepSize;

                    Vector3d relativeToOGParent = orbit.GetWorldPos() - parent.GetWorldPos();
                    Vector3d localPos = relativeToOGParent - startLocalPos;
                    orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

                    if (i == (orbitDetail - 1)) patchDepth += CelestialPhysicsSingleton.get_singleton().patchDepthLimit;
    			
                }
            }
        }

        CelestialPhysicsSingleton.get_singleton().ProcessCelestialPhysics(UniversalTimeSingleton.Get.time);

        orbit.InitialiseFromOrbitInfo(orbitInfo);


    }

    private void GetOrbitPoints() {
        if (this.orbit.get_eccentricity() < 0) return;

        double start = orbit.get_true_anomaly();
        double step = start;
        
        orbitPoints = new();
        Vector3d startLocalPos = orbit.GetLocalPos();


        if (orbit.get_eccentricity() < 1) {

            double stepSize = 2 * Mathd.PI / orbitDetail;

            for(int i = 0; i < orbitDetail; i++) {

                Vector3d localPos = orbit.GetCartesianAtTrueAnomaly(start + step, false).localPos - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

                step += stepSize;

            }


        } else {

            double end = Mathd.Acos( -1 / orbit.get_eccentricity());
            double range = end - start;
            double stepSize = range / (orbitDetail - 1);

            for (int i = 0; i < orbitDetail; i++) {
                
                Vector3d localPos = orbit.GetCartesianAtTrueAnomaly(start + step, false).localPos - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

                if (i < (orbitDetail - 2)) step += stepSize;
            
            }
        }

        orbit._physics_process(UniversalTimeSingleton.Get.time);

    }

    private void DrawOrbitPoints() {

        Vector3 camPos;
        camPos = Camera.main.transform.localPosition;

        float distance = camPos.magnitude;

        float width = distance / 300;


        lineRenderer.useWorldSpace = false;
        lineRenderer.material = CelestialPhysicsSingleton.get_singleton().getLineMat();
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.enabled = true;
        lineRenderer.loop = !patching;

        lineRenderer.positionCount = orbitPoints.Count;
        lineRenderer.SetPositions(orbitPoints.ToArray());

        lineRenderer.startColor = orbitColour;
        lineRenderer.endColor = orbitColour;
        lineRenderer.widthMultiplier = width;
        
    }

}
