using System.Collections.Generic;
using System.Diagnostics;
using EditorAttributes;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Void = EditorAttributes.Void;

namespace Orbits
{

/*
    #==============================================================#
	
	
	
	
*/
[RequireComponent(typeof(SpaceSimTransform))]
public class OrbitDriver : MonoBehaviour
{
//--#
    #region Variables


    /////////////////////////////////////// Initialiser ///////////////////////////////////////
    [field: Title("Initialisation")]
    [HorizontalGroup(true, nameof(cartesianInitialise), nameof(keplerInitialise))]
    [SerializeField] private Void initButtonsHolder;

    public void InitCartesian() { keplerInitialise = !cartesianInitialise; }
    [SerializeField, HideInInspector, OnValueChanged(nameof(InitCartesian))] private bool cartesianInitialise;

    public void InitKepler() { cartesianInitialise = !keplerInitialise; }
    [SerializeField, HideInInspector, OnValueChanged(nameof(InitKepler))] private bool keplerInitialise;

    [field: Title("")]
    [field: SerializeField] public CelestialObject parent {get; private set;}
    [field: Space(5)]
    [field: SerializeField, ShowField(nameof(cartesianInitialise))] public Vector3d initPos {get; private set;}
    [field: SerializeField, ShowField(nameof(cartesianInitialise))] public Vector3d initVel {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initPeriapsis {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initEccentricity {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initInclination {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initRightAscensionOfAscendingNode {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initArgumentOfPeriapsis {get; private set;}
    [field: SerializeField, ShowField(nameof(keplerInitialise))] public double initTrueAnomaly {get; private set;}
    /////////////////////////////////////// ----------- ///////////////////////////////////////

    [field: Space(10)]
    [field: Title("Orbit")]
    [field: SerializeField, ReadOnly] public List<Orbit> orbits {get; private set;} = new List<Orbit>(10);
    [Button] public void LogOrbit() => Debug.Log(orbits[0]);

    public SpaceSimTransform simTransform {get; private set;}

    public Color lineCol = Color.red;
    
    public List<OrbitConicLine> orbitConicLines;

    public OrbitPoint currentPoint;


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    private void OnValidate() {
        Awake();
        Start();
        if (ScaleSpaceSingleton.Get.scaledSpaceTransform.Find(gameObject.name).TryGetComponent<ScaleSpaceMovement>(out ScaleSpaceMovement scaleSpaceMovement)){
            scaleSpaceMovement.OnValidate();
        };
    }

    private void OnDrawGizmos() { // TODO: Get shot for an actual orbit drawer
        if (orbitConicLines != null) {
            foreach (OrbitConicLine orbitConicLine in orbitConicLines) {

                Gizmos.color = lineCol;
                for (int i = 0; i < orbitConicLine.points.Length; i++) {
                    if (i != orbitConicLine.points.Length - 1)
                        Gizmos.DrawLine((Vector3)((orbitConicLine.orbit.parent.simTransform.position + orbitConicLine.points[i].position) / ScaleSpaceSingleton.Get.scaleDownFactor), 
                                        (Vector3)((orbitConicLine.orbit.parent.simTransform.position + orbitConicLine.points[i + 1].position) / ScaleSpaceSingleton.Get.scaleDownFactor));
                }
                Gizmos.color = Color.green;
                Gizmos.DrawSphere((Vector3)((orbitConicLine.orbit.parent.simTransform.position + orbitConicLine.startPoint.position) / ScaleSpaceSingleton.Get.scaleDownFactor), 0.05f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere((Vector3)((orbitConicLine.orbit.parent.simTransform.position + orbitConicLine.endPoint.position) / ScaleSpaceSingleton.Get.scaleDownFactor), 0.05f);

            }
        }
    }
#endif

    private void Awake() {
        simTransform = GetComponent<SpaceSimTransform>();
        simTransform.AddSimComponent(this);

        orbits = new List<Orbit>();

        if (keplerInitialise) {
            RecalculateOrbit(initPeriapsis, initEccentricity, initInclination, initRightAscensionOfAscendingNode, initArgumentOfPeriapsis, initTrueAnomaly, parent);
        }
        else if (cartesianInitialise) {
            RecalculateOrbit(initPos, initVel, parent);
        }

        simTransform.SetLocalPosition(orbits[0].GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        simTransform.ToTransform();

        if (simTransform.TryGetSimComponent(out CelestialObject celestialObject)) {
            celestialObject.ReCalcSOI();
        }
    }
    private void Start() {
        RecalculatePatchedConics();
    }

    private void OnDestroy() {
        simTransform.RemoveSimComponent(this);
    }

    private void FixedUpdate() {

        Stopwatch watch = new();
        watch.Start();

        var res = orbits[0].GetCartesianAtTime(UniversalTimeSingleton.Get.time);
        currentPoint = new(res.localPos, res.localVel, orbits[0].GetTrueAnomalyAtTime(UniversalTimeSingleton.Get.time), UniversalTimeSingleton.Get.time);
        simTransform.SetLocalPosition(currentPoint.position);
        simTransform.ToTransform();

        RecalculateOrbit(currentPoint.position, currentPoint.velocity, parent);
        RecalculatePatchedConics();

        watch.Stop();
        Debug.Log($"{gameObject.name} took {(double)watch.ElapsedTicks * 1000 / Stopwatch.Frequency} ms");
    }


    #endregion
//--#



//--#
    #region Setup


    public void RecalculateOrbit(double _periapsis, double _eccentricity, double _inclination, double _rightAscensionOfAscendingNode, double _argumentOfPeriapsis, double _trueAnomaly, CelestialObject _parent) {
        orbits.Clear();
        orbits.Insert(0, new(_periapsis, _eccentricity, _inclination, _rightAscensionOfAscendingNode, _argumentOfPeriapsis, _trueAnomaly, _parent, UniversalTimeSingleton.Get.time));
    }
    public void RecalculateOrbit(Vector3d _pos, Vector3d _vel, CelestialObject _parent) {
        orbits.Clear();
        orbits.Insert(0, new(_pos, _vel, _parent, UniversalTimeSingleton.Get.time));
    }


    #endregion
//--#



//--#
    #region Patching


    public void RecalculatePatchedConics() {
        
        for (int i = 0; i < OrbitSettingsSingleton.Get.patchLimit-1; i++) {

            var patchingResults = PatchedConicsSolver.RecalculatePatchPrediction(orbits[i], this);
            if (patchingResults.patched) {

                orbits[i].endingTrueAnomaly = (double)(patchingResults.patchPoint?.trueAnomaly);
                if (orbits[i].endingTrueAnomaly <= orbits[i].startingTrueAnomaly) orbits[i].endingTrueAnomaly += 2 * Mathf.PI;
                orbits[i].orbitEndTime = (double)(patchingResults.patchPoint?.time);

                Vector3d relativePos;
                Vector3d relativeVel;
                if (patchingResults.patchedUp) {
                    orbits[i].parent.simTransform.TryGetSimComponent<OrbitDriver>(out OrbitDriver patchedOrbitDriver);
                    var parentRes = patchedOrbitDriver.orbits[0].GetCartesianAtTime(orbits[i].orbitEndTime);
                    relativePos = (Vector3d)(patchingResults.patchPoint?.position) + parentRes.localPos;
                    relativeVel = (Vector3d)(patchingResults.patchPoint?.velocity) + parentRes.localVel;
                }
                else {
                    patchingResults.celestialObjectPatchedInto.simTransform.TryGetSimComponent<OrbitDriver>(out OrbitDriver patchedOrbitDriver);
                    var newParentRes = patchedOrbitDriver.orbits[0].GetCartesianAtTime(orbits[i].orbitEndTime);
                    relativePos = (Vector3d)(patchingResults.patchPoint?.position) - newParentRes.localPos;
                    relativeVel = (Vector3d)(patchingResults.patchPoint?.velocity) - newParentRes.localVel;
                }
                orbits.Insert(i + 1, new(relativePos, relativeVel, patchingResults.celestialObjectPatchedInto, orbits[i].orbitEndTime));
                
            }
            else 
                break;

        }
        
        orbitConicLines.Clear();
        foreach (Orbit orbit in orbits) {
            orbitConicLines.Add(new(orbit, 64));
        }

    }


    #endregion
//--#
}

}