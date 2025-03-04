using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Orbit), typeof(LineRenderer))]
[ExecuteInEditMode]
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
    Orbit clone;
    //

    public void EditorUpdate() { // for drawing orbits in editor
        if (!Application.isPlaying && transform.parent.TryGetComponent<Orbit>(out var a)){
            Setup();
            DrawOrbit();
            orbit.EditorUpdate();
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
        //if (patching) GetOrbitPointsPatched();
        //else 
            GetOrbitPoints();
        DrawOrbitPoints();
    }
    public void ProcessOrbit() {
        if (patching) orbit.patch_conics();
        orbit._physics_process(Time.fixedDeltaTime * CelestialPhysics.get_singleton().get_time_scale());

        DrawOrbit();
    }





    /*
            Draw those orbits!
    */

    private bool DrawPatching(Orbit orbit, ref double startingTrueAnomaly) {
        if (orbit.patch_conics()){
            return true;
        }
        return false;
    }

    // private void GetOrbitPointsPatched() {
    //     if (this.orbit.get_eccentricity() < 0) return;

    //     clone.init(this.orbit);

    //     double startingTrueAnomaly = clone.get_true_anomaly();
    //     double totalStepped = 0;
        
    //     orbitPoints = new();
    //     Vector3d startLocalPos = clone.getLocalPos();
    //     Orbit parent = clone.parent;
    //     orbitPoints.Add(Vector3.zero);

    //     int depth = 1;

    //     while (depth <= CelestialPhysics.get_singleton().patchDepthLimit)
    //     {
    //         if (clone.get_eccentricity() < 1) {
    
    //             double trueAnomalyStepSize = (2 * Math.PI / orbitDetail) / clone.get_mean_motion_from_keplerian();

    //             for(int i = 0; i < orbitDetail; i++) {
    
    //                 if (patching) 
    //                     if (DrawPatching(clone, ref startingTrueAnomaly)) {
    //                         depth++;
    //                         break;
    //                     }

    //                 celestialBodies.ForEach(delegate(Orbit body) {
    //                     body._physics_process(trueAnomalyStepSize);
    //                 });
    
    //                 clone._physics_process(trueAnomalyStepSize);
    //                 totalStepped += trueAnomalyStepSize;

    //                 Vector3d posRelativeToStartParent = clone.getWorldPos() - parent.getWorldPos();
    //                 Vector3d localPos = posRelativeToStartParent - startLocalPos;
    //                 orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * CelestialPhysics.get_singleton().get_spaceScale()));
    
    //                 if (i == (orbitDetail - 1)) depth = CelestialPhysics.get_singleton().patchDepthLimit + 10;

    //             }
        
    
    //         } else {
    
    // 			double endTrueAnomaly = Math.Acos( -1 / clone.get_eccentricity());
    // 			double range = endTrueAnomaly - startingTrueAnomaly;
    // 			double trueAnomalyStepSize = (range / (orbitDetail - 1)) / clone.get_mean_motion_from_keplerian();
    
    // 			for (int i = 0; i < orbitDetail; i++) {
    
    //                 if (patching) 
    //                     if (DrawPatching(clone, ref startingTrueAnomaly)) {
    //                         depth++;
    //                         break;
    //                     }

    //                 celestialBodies.ForEach(delegate(Orbit body) {
    //                     body._physics_process(trueAnomalyStepSize);
    //                 });
    
    //                 clone._physics_process(trueAnomalyStepSize);
    //                 totalStepped += trueAnomalyStepSize;
        
    //                 Vector3d posRelativeToStartParent = clone.getWorldPos() - parent.getWorldPos();
    //                 Vector3d localPos = posRelativeToStartParent - startLocalPos;
    //                 orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * CelestialPhysics.get_singleton().get_spaceScale()));

    //                 if (i == (orbitDetail - 1)) depth = CelestialPhysics.get_singleton().patchDepthLimit + 10;
    			
    //             }
    //         }
    //     }

    //     celestialBodies.ForEach(delegate(Orbit body) {
    //         body._physics_process(-totalStepped);
    //     });

    // }

    private void GetOrbitPoints() {
        if (this.orbit.get_eccentricity() < 0) return;

        double startingTrueAnomaly = orbit.get_true_anomaly();
        double trueAnomaly = startingTrueAnomaly;
        
        orbitPoints = new();
        Vector3d startLocalPos = orbit.getLocalPos();
        orbitPoints.Add(Vector3.zero);


        if (orbit.get_eccentricity() < 1) {

            double trueAnomalyStepSize = 2 * Math.PI / orbitDetail;

            for(int i = 0; i < orbitDetail; i++) {

                trueAnomaly += trueAnomalyStepSize;
                orbit.set_true_anomaly(trueAnomaly);

                Vector3d localPos = orbit.getLocalPos() - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * CelestialPhysics.get_singleton().get_spaceScale()));

            }


        } else {

            double endTrueAnomaly = Math.Acos( -1 / orbit.get_eccentricity());
            double range = endTrueAnomaly - startingTrueAnomaly;
            double trueAnomalyStepSize = range / (orbitDetail - 1);

            for (int i = 0; i < orbitDetail; i++) {

                if (i < (orbitDetail - 2)) trueAnomaly += trueAnomalyStepSize;
                orbit.set_true_anomaly(trueAnomaly);

                Vector3d localPos = orbit.getLocalPos() - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * CelestialPhysics.get_singleton().get_spaceScale()));
            
            }
        }

        orbit.set_true_anomaly(startingTrueAnomaly);

    }

    private void DrawOrbitPoints() {

        Vector3 camPos;
        camPos = Camera.main.transform.position;

        float distance = camPos.magnitude;

        float width = distance / 300;


        lineRenderer.useWorldSpace = false;
        lineRenderer.material = CelestialPhysics.get_singleton().getLineMat();
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.enabled = true;
        lineRenderer.loop = false;

        lineRenderer.positionCount = orbitPoints.Count;
        lineRenderer.SetPositions(orbitPoints.ToArray());

        lineRenderer.startColor = orbitColour;
        lineRenderer.endColor = orbitColour;
        lineRenderer.widthMultiplier = width;
        
    }

}
