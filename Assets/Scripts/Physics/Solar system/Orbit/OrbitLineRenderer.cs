using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class OrbitLineRenderer : MonoBehaviour
{
//--#
    #region Variables

    
    [field: Space(7)]
    [field: Title("Refs")]
    [field: SerializeField, ReadOnly] public Orbit refOrbit { get; private set; }
    [field: SerializeField, ReadOnly] public CelestialObject refCO { get; private set; }

    [field: Space(7)]
    [field: Title("Orbit params")]
    private List<Vector3> orbitPoints;
    [SerializeField] private int orbitDetail = 100;
    [SerializeField] private Color orbitColour = Color.red;

    LineRenderer lineRenderer;
    int patchDepth;


    #endregion
//--#

//--#
    #region unity events

    
    private void Start() {
        refOrbit = refCO.refOrbit;
        lineRenderer = GetComponent<LineRenderer>();
    }


    #endregion
//--#

    public void SetCelestialObject(CelestialObject co) {
        refCO = co;
        refOrbit = co.refOrbit;
    }

//--#
    #region Drawing orbits
    
    public void DrawOrbit() {
        GetOrbitPoints();
        DrawOrbitPoints();
    }

    /// <summary>
    /// Intended for use when predicting refOrbit lines
    /// </summary>
    /// <param name="time"></param>
    // public void ProcessOrbitGhost(double time) {
    //     refOrbit._physics_process(time, false);
    //     if (patching) 
    //         if (refOrbit.patch_conics()) {
    //             refOrbit.SetOrbitStartTime(time);
    //             patchDepth++;
    //         }
    // }





    /*
            Draw those orbits!
    */
    // private void GetOrbitPointsPatched() {
    //     if (this.refOrbit.get_eccentricity() < 0) return;

    //     Orbit.OrbitInfo orbitInfo = new(){
    //         mass = refOrbit.get_mass(),
    //         mu = refOrbit.GetParentMu(),
    //         periapsis = refOrbit.get_periapsis(),
    //         eccentricity = refOrbit.get_eccentricity(),
    //         longitude_of_ascending_node = refOrbit.get_longitude_of_ascending_node(),
    //         longitude_of_perigee = refOrbit.get_longitude_of_perigee(),
    //         inclination = refOrbit.get_inclination(),
    //         clockwise = refOrbit.get_clockwise(),
    //         true_anomaly = refOrbit.get_true_anomaly(),
    //         mean_anomaly = refOrbit.get_mean_anomaly(),
    //         orbitStartTime = refOrbit.GetOrbitStartTime(),
    //         localPos = refOrbit.GetLocalPos(),
    //         localVel = refOrbit.GetLocalVel(),
    //         parent = refOrbit.transform.parent,
    //         parentOrbit = refOrbit.parentOrbit
    //     };

    //     double start = refOrbit.get_true_anomaly();
    //     double step = start;

    //     double startTime = UniversalTimeSingleton.Get.time;
        
    //     orbitPoints = new();
    //     Vector3d startLocalPos = refOrbit.GetLocalPos();

    //     Orbit parent = refOrbit.transform.parent.GetComponent<Orbit>();

    //     patchDepth = 1;

    //     while (patchDepth <= SpaceControllerSingleton.Get.patchDepthLimit)
    //     {   

    //         if (refOrbit.get_eccentricity() < 1) {
    
    //             double stepSize = (2 * Mathd.PI / orbitDetail) / refOrbit.get_mean_motion_from_keplerian();

    //             for(int i = 0; i <= orbitDetail; i++) {

    //                 int currentPatch = patchDepth;

    //                 SpaceControllerSingleton.Get.ProcessCelestialPhysics(startTime + step);
    //                 step += stepSize;

    //                 if(patchDepth != currentPatch) { break; }

    //                 Vector3d relativeToOGParent = refOrbit.GetWorldPos() - parent.GetWorldPos();
    //                 Vector3d localPos = relativeToOGParent - startLocalPos;
    //                 orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));
    
    //                 if (i == (orbitDetail - 1)) patchDepth += SpaceControllerSingleton.Get.patchDepthLimit;

    //             }
        
    
    //         } else {
    
    // 			double endTrueAnomaly = Mathd.Acos( -1 / refOrbit.get_eccentricity());
    // 			double range = endTrueAnomaly - start;
    // 			double stepSize = (range / (orbitDetail)) / refOrbit.get_mean_motion_from_keplerian();
    
    // 			for (int i = 0; i < orbitDetail; i++) {

    //                 int currentPatch = patchDepth;

    //                 SpaceControllerSingleton.Get.ProcessCelestialPhysics(startTime + step);
    //                 step += stepSize;

    //                 if(patchDepth != currentPatch) { break; }

    //                 Vector3d relativeToOGParent = refOrbit.GetWorldPos() - parent.GetWorldPos();
    //                 Vector3d localPos = relativeToOGParent - startLocalPos;
    //                 orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

    //                 if (i == (orbitDetail-1)) patchDepth += SpaceControllerSingleton.Get.patchDepthLimit;
    			
    //             }
    //         }
    //     }

    //     SpaceControllerSingleton.Get.ProcessCelestialPhysics(UniversalTimeSingleton.Get.time);

    //     refOrbit.InitialiseFromOrbitInfo(orbitInfo);


    // }

    private void GetOrbitPoints() {
        if (this.refOrbit.get_eccentricity() < 0) return;

        double start = refOrbit.get_true_anomaly();
        double step = start;
        
        orbitPoints = new();
        Vector3d startLocalPos = refOrbit.GetLocalPos();


        if (refOrbit.get_eccentricity() < 1) {

            double stepSize = 2 * Mathd.PI / orbitDetail;

            for(int i = 0; i < orbitDetail; i++) {

                Vector3d localPos = refOrbit.GetCartesianAtTrueAnomaly(start + step, false).localPos - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

                step += stepSize;

            }


        } else {

            double end = Mathd.Acos( -1 / refOrbit.get_eccentricity());
            double range = end - start;
            double stepSize = range / (orbitDetail - 1);

            for (int i = 0; i < orbitDetail; i++) {
                
                Vector3d localPos = refOrbit.GetCartesianAtTrueAnomaly(start + step, false).localPos - startLocalPos;
                orbitPoints.Add((Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * ScaleSpaceSingleton.Get.GetSpaceScale()));

                if (i < (orbitDetail - 2)) step += stepSize;
            
            }
        }
    }

    private void DrawOrbitPoints() {

        Vector3 camPos;
        camPos = Camera.main.transform.localPosition;

        float distance = camPos.magnitude;

        float width = distance / 300;


        lineRenderer.useWorldSpace = false;
        lineRenderer.material = SpaceControllerSingleton.Get.lineMat;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.enabled = true;
        lineRenderer.loop = true;

        lineRenderer.positionCount = orbitPoints.Count;
        lineRenderer.SetPositions(orbitPoints.ToArray());

        lineRenderer.startColor = orbitColour;
        lineRenderer.endColor = orbitColour;
        lineRenderer.widthMultiplier = width;
        
    }


    #endregion
//--#
}
