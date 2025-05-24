using System;
using System.Collections.Generic;
using System.Diagnostics;
using EditorAttributes;
using Unity.VisualScripting;
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
    
    public OrbitConicLine orbitConic;

    public Nullable<OrbitPoint> firstPatchPoint;

    public List<PatchingCandidate> patchingCandidates;

    public List<OrbitPoint> points;


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
        if (points != null) {
            Gizmos.color = lineCol;
            for (int i = 0; i < points.Count; i++) {
                if (i != points.Count - 1)
                    Gizmos.DrawSphere((Vector3)((parent.GetComponent<SpaceSimTransform>().position + points[i].position) / ScaleSpaceSingleton.Get.scaleDownFactor), 0.05f);
            }
        }
        if (orbitConic.points != null) {
            Gizmos.color = lineCol;
            for (int i = 0; i < orbitConic.points.Length; i++) {
                if (i != orbitConic.points.Length - 1)
                    Gizmos.DrawLine((Vector3)((parent.GetComponent<SpaceSimTransform>().position + orbitConic.points[i].position) / ScaleSpaceSingleton.Get.scaleDownFactor), 
                                    (Vector3)((parent.GetComponent<SpaceSimTransform>().position + orbitConic.points[i + 1].position) / ScaleSpaceSingleton.Get.scaleDownFactor));
            }
        }
        if (firstPatchPoint?.position != null) {
            Gizmos.color = lineCol;
            Gizmos.DrawSphere((Vector3)((parent.GetComponent<SpaceSimTransform>().position + firstPatchPoint?.position) / ScaleSpaceSingleton.Get.scaleDownFactor), 0.075f);
            Gizmos.DrawLine((Vector3)((parent.GetComponent<SpaceSimTransform>().position + firstPatchPoint?.position) / ScaleSpaceSingleton.Get.scaleDownFactor), 
                            (Vector3)((parent.GetComponent<SpaceSimTransform>().position + firstPatchPoint?.position + firstPatchPoint?.velocity) / ScaleSpaceSingleton.Get.scaleDownFactor));
        }
    }
#endif

    private void Awake() {
        simTransform = GetComponent<SpaceSimTransform>();
        simTransform.AddSimComponent(this);

        orbits = new List<Orbit>(10);

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

        orbitConic = new(orbits[0], 64); // Patch
        RecalculatePatchPrediction();

    }

    private void OnDestroy() {
        simTransform.RemoveSimComponent(this);
    }

    private void FixedUpdate() {
        simTransform.SetLocalPosition(orbits[0].GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos);
        simTransform.ToTransform();

        RecalculatePatchPrediction();
    }


    #endregion
//--#



//--#
    #region Setup


    public void RecalculateOrbit(double _periapsis, double _eccentricity, double _inclination, double _rightAscensionOfAscendingNode, double _argumentOfPeriapsis, double _trueAnomaly, CelestialObject _parent) {
        orbits.Insert(0, new(_periapsis, _eccentricity, _inclination, _rightAscensionOfAscendingNode, _argumentOfPeriapsis, _trueAnomaly, _parent, UniversalTimeSingleton.Get.time));
    }
    public void RecalculateOrbit(Vector3d _pos, Vector3d _vel, CelestialObject _parent) {
        orbits.Insert(0, new(_pos, _vel, _parent, UniversalTimeSingleton.Get.time));
    }


    #endregion
//--#



//--#
    #region Patching


    public void RecalculatePatchPrediction() {
        Stopwatch watch = Stopwatch.StartNew();

        points = new();
        
        // Get the list of potential candidates for patching
        patchingCandidates = new();
        foreach (SpaceSimTransform sibling in simTransform.parent.children) {
            if (sibling != this.simTransform
                && sibling.TryGetSimComponent<CelestialObject>(out CelestialObject candidateCelestialObject)
                && sibling.TryGetSimComponent<OrbitDriver>(out OrbitDriver candidateOrbitDriver)
            ) {

                if (candidateCelestialObject.SOIdistance > 100 // SOI size prefilter
                    && !(orbits[0].apoapsis < candidateOrbitDriver.orbits[0].periapsis - candidateCelestialObject.SOIdistance) // Apoapsis prefilter
                    && !(orbits[0].periapsis > candidateOrbitDriver.orbits[0].apoapsis + candidateCelestialObject.SOIdistance) // Periapsis prefilter
                ) {

                    patchingCandidates.Add(new(candidateOrbitDriver.orbits[0], candidateCelestialObject.SOIdistance));

                }

            }
        }


        if (orbits[0].apoapsis > parent.SOIdistance) {
            double ta = orbits[0].GetTrueAnomalyAtDistance(parent.SOIdistance);
            var res = orbits[0].GetCartesianAtTrueAnomaly(ta);
            firstPatchPoint = new(res.localPos, res.localVel, ta, orbits[0].GetTimeAtTrueAnomaly(ta));
        }
        else firstPatchPoint = null;

        double startDistance = orbits[0].GetCartesianAtTime(orbits[0].epoch).localPos.magnitude;
        foreach (PatchingCandidate candidate in patchingCandidates) {

            CalculateOrbitCrossoverPointsForCandidate(candidate);

            /*
                Get closest approach between each crossover point until one has a closest approach less than SOI distance of the candidate.

                Each case here follows the same logic, just altered for certain situations
                - Get the closest approach time between the entry and exit of boundry
                - Check if that's within the candidate's SOI
                - If so set the first patch point to be on the boundary and clear the list to break out
                - Else remove the points just searched between from the list
                - Repeat until there are no points in list

            */
            double closestApproachTime = 0; 
            double finalTime = (double)((firstPatchPoint == null) ? orbits[0].orbitEndTime : firstPatchPoint?.time);
            if (candidate.orbitRangeCrossingPoints.Count <= 0) { // If there are no crossover points and it hasn't been culled by the prefilters then it must, until the first patch, be at all times within the candidate's orbit.
                
                closestApproachTime = SearchForTimeOfClosestApproach(orbits[0].epoch, finalTime, candidate);
                var res = orbits[0].GetCartesianAtTime(closestApproachTime);

                if (orbits[0].GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                    firstPatchPoint = SOIBoundarySearch(orbits[0].epoch, closestApproachTime, candidate);
                    candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                }
            }
            
            // Special starting case, from starting time to first point's time
            else if (candidate.orbit.periapsis - candidate.SOIdistance <= startDistance && startDistance <= candidate.orbit.apoapsis + candidate.SOIdistance) {
                
                closestApproachTime = SearchForTimeOfClosestApproach(orbits[0].epoch, candidate.orbitRangeCrossingPoints[0].time, candidate);
                var res = orbits[0].GetCartesianAtTime(closestApproachTime);

                if (orbits[0].GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                    firstPatchPoint = SOIBoundarySearch(orbits[0].epoch, closestApproachTime, candidate);
                    candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                }
                else candidate.orbitRangeCrossingPoints.RemoveAt(0);
            }

            int itt = 0; // Juuuuuuuust in case, never trust a while loop. There should only ever be 2 of these checks max
            while (itt < 5 && candidate.orbitRangeCrossingPoints.Count > 0) {
                
                // From first point to firstPatchPoint
                if (candidate.orbitRangeCrossingPoints.Count == 1) {
                    
                    closestApproachTime = SearchForTimeOfClosestApproach(candidate.orbitRangeCrossingPoints[0].time, finalTime, candidate);
                    var res = orbits[0].GetCartesianAtTime(closestApproachTime);

                    if (orbits[0].GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                        firstPatchPoint = SOIBoundarySearch(candidate.orbitRangeCrossingPoints[0].time, closestApproachTime, candidate);
                        candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                    }
                    else candidate.orbitRangeCrossingPoints.RemoveAt(0);
                }
                
                // From first point to second point
                else {
                    
                    closestApproachTime = SearchForTimeOfClosestApproach(candidate.orbitRangeCrossingPoints[0].time, candidate.orbitRangeCrossingPoints[1].time, candidate);
                    var res = orbits[0].GetCartesianAtTime(closestApproachTime);

                    if (orbits[0].GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                        firstPatchPoint = SOIBoundarySearch(candidate.orbitRangeCrossingPoints[0].time, closestApproachTime, candidate);
                        candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                    }
                    else {
                        candidate.orbitRangeCrossingPoints.RemoveAt(0);
                        candidate.orbitRangeCrossingPoints.RemoveAt(0);
                    }
                }

                itt++;
            }

        }

        watch.Stop();
        Debug.Log("patch prediction timer for "+gameObject.name+": " + ((double)(1000 * watch.ElapsedTicks) / Stopwatch.Frequency) + "ms");
    }

    public void CalculateOrbitCrossoverPointsForCandidate(PatchingCandidate candidate) {

        double time1;
        double time2;

        try{ // Get the two points where the main orbit crosses the minimum orbital radius for an encounter

            double ta = orbits[0].GetTrueAnomalyAtDistance(candidate.orbit.periapsis - candidate.SOIdistance);
            if (ta != -1) {
                time1 = orbits[0].GetTimeAtTrueAnomaly(ta);
                if ((time1 < firstPatchPoint?.time || firstPatchPoint == null) && double.IsFinite(time1)) {
                    var res1 = orbits[0].GetCartesianAtTrueAnomaly(ta);
                    candidate.orbitRangeCrossingPoints.Add(new(res1.localPos, res1.localVel, ta, time1));
                }

                time2 = orbits[0].GetTimeAtTrueAnomaly(-ta);
                if ((time2 < firstPatchPoint?.time || firstPatchPoint == null) && double.IsFinite(time2)) {
                    var res2 = orbits[0].GetCartesianAtTrueAnomaly(-ta);
                    candidate.orbitRangeCrossingPoints.Add(new(res2.localPos, res2.localVel, -ta, time2));
                }
            }

        }
        catch (System.Exception){}

        try{ // Get the two points where the main orbit crosses the maximum orbital radius for an encounter

            double ta = orbits[0].GetTrueAnomalyAtDistance(candidate.orbit.apoapsis + candidate.SOIdistance);
            if (ta != -1) {
                time1 = orbits[0].GetTimeAtTrueAnomaly(ta);
                if ((time1 < firstPatchPoint?.time || firstPatchPoint == null) && double.IsFinite(time1)) {
                    var res1 = orbits[0].GetCartesianAtTrueAnomaly(ta);
                    candidate.orbitRangeCrossingPoints.Add(new(res1.localPos, res1.localVel, ta, time1));
                }

                time2 = orbits[0].GetTimeAtTrueAnomaly(-ta);
                if ((time2 < firstPatchPoint?.time || firstPatchPoint == null) && double.IsFinite(time2)) {
                    var res2 = orbits[0].GetCartesianAtTrueAnomaly(-ta);
                    candidate.orbitRangeCrossingPoints.Add(new(res2.localPos, res2.localVel, -ta, time2));
                }
            }

        }
        catch (System.Exception){}

        candidate.orbitRangeCrossingPoints.Sort();
        
    }

    public double SearchForTimeOfClosestApproach(double _startTime, double _endTime, PatchingCandidate _candidate) {

        // Uses a golden section search to find the closest approach

        double f(double t) => orbits[0].GetDistanceFromSiblingOrbitAtTime(_candidate.orbit, t);
        double a, b, c, d, fb, fc;
        double phi = Mathd.Phi;

        a = _startTime;
        d = _endTime;

        b = d + (a - d) / phi;
        c = a + (d - a) / phi;

        fb = f(b);
        fc = f(c);

        int itr = 0;
        int itrMax = 50;
        double tolerance = 1d;
        while ((Mathd.Abs(d - a) > tolerance) && itr < itrMax) {
            
            if (fb < fc) {
                d = c;
                c = b;
                b = d + (a - d) / phi;

                fc = fb;
                fb = f(b);
            }
            else {
                a = b;
                b = c;
                c = a + (d - a) / phi;

                fb = fc;
                fc = f(c);
            }

            itr++;

        }

        return (a + d) / 2;
    }

    public OrbitPoint SOIBoundarySearch(double _startTime, double _endTime, PatchingCandidate _candidate) {

        // Uses simple bisection method to find point where distance = SOI

        double f(double t) => orbits[0].GetDistanceFromSiblingOrbitAtTime(_candidate.orbit, t);
        double a, b, c, distance;
        a = _startTime;
        c = _endTime;
        b = (a + c) / 2;

        distance = f(b);
        
        int itr = 0;
        int itrMax = 50;
        double tolerance = 0.1d;
        while (Mathd.Abs(distance - _candidate.SOIdistance) > tolerance && itr < itrMax) {
            
            if (distance > _candidate.SOIdistance) {
                a = b;
                b = (a + c) / 2;

                distance = f(b);
            }
            else {
                c = b;
                b = (a + c) / 2;

                distance = f(b);
            }
            
            itr++;
        }

        var res = orbits[0].GetCartesianAtTime(b);
        return new OrbitPoint(res.localPos, res.localVel, orbits[0].GetTrueAnomalyAtTime(b), b);

    }


    #endregion
//--#
}

}