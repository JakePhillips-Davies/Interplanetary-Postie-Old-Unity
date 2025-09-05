using System.Collections.Generic;
using Orbits;
using UnityEngine;

public class PatchedConicsSolver
{
//--#
    #region Crossover points


    public static (bool patched, OrbitPoint? patchPoint, CelestialObject celestialObjectPatchedInto, bool patchedUp) RecalculatePatchPrediction(Orbit _orbit, OrbitDriver _orbitDriver) {

        OrbitPoint? firstPatchPoint = null;
        CelestialObject celestialObjectPatchedInto = null;
        List<PatchingCandidate> patchingCandidates = new();

        CelestialObject parent = _orbit.parent;
        bool patchedUp = false;

        // Get the list of potential candidates for patching
        foreach (SpaceSimTransform sibling in parent.simTransform.children) {
            if (sibling != _orbitDriver.simTransform
                && sibling.TryGetSimComponent<CelestialObject>(out CelestialObject candidateCelestialObject)
                && sibling.TryGetSimComponent<OrbitDriver>(out OrbitDriver candidateOrbitDriver)
            ) {

                if (candidateCelestialObject.SOIdistance > 100 // SOI size prefilter
                    && !(_orbit.apoapsis < candidateOrbitDriver.orbits[0].periapsis - candidateCelestialObject.SOIdistance) // Apoapsis prefilter
                    && !(_orbit.periapsis > candidateOrbitDriver.orbits[0].apoapsis + candidateCelestialObject.SOIdistance) // Periapsis prefilter
                ) {

                    patchingCandidates.Add(new(candidateOrbitDriver.orbits[0], candidateCelestialObject));

                }

            }
        }


        if (_orbit.apoapsis > parent.SOIdistance) {
            double ta = _orbit.GetTrueAnomalyAtDistance(parent.SOIdistance);
            if (ta <= _orbit.startingTrueAnomaly) ta += 2 * Mathd.PI; // Sometimes it sneaks behind the starting anomaly and causes issues 

            var res = _orbit.GetCartesianAtTrueAnomaly(ta);
            firstPatchPoint = new(res.localPos, res.localVel, ta, _orbit.GetTimeAtTrueAnomaly(ta));

            _orbit.parent.simTransform.parent.TryGetComponent<CelestialObject>(out CelestialObject grandparent);
            celestialObjectPatchedInto = grandparent;
            patchedUp = true;
        }

        double startDistance = _orbit.GetCartesianAtTime(_orbit.epoch).localPos.magnitude;
        foreach (PatchingCandidate candidate in patchingCandidates) {

            CalculateOrbitCrossoverPointsForCandidate(_orbit, candidate, firstPatchPoint);

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
            double finalTime = (double)((firstPatchPoint == null) ? _orbit.orbitEndTime : firstPatchPoint?.time);
            if (candidate.orbitRangeCrossingPoints.Count <= 0) { // If there are no crossover points and it hasn't been culled by the prefilters then it must, until the first patch, be at all times within the candidate's orbit.
                
                closestApproachTime = SearchForTimeOfClosestApproach(_orbit.epoch, finalTime, _orbit, candidate.orbit);
                var res = _orbit.GetCartesianAtTime(closestApproachTime);

                if (_orbit.GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                    firstPatchPoint = SOIBoundarySearch(_orbit.epoch, closestApproachTime, _orbit, candidate);
                    celestialObjectPatchedInto = candidate.celestialObject;
                    patchedUp = false;
                    candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                }
            }
            
            // Special starting case, from starting time to first point's time
            else if (candidate.orbit.periapsis - candidate.SOIdistance <= startDistance && startDistance <= candidate.orbit.apoapsis + candidate.SOIdistance) {
                
                closestApproachTime = SearchForTimeOfClosestApproach(_orbit.epoch, candidate.orbitRangeCrossingPoints[0].time, _orbit, candidate.orbit);
                var res = _orbit.GetCartesianAtTime(closestApproachTime);

                if (_orbit.GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                    firstPatchPoint = SOIBoundarySearch(_orbit.epoch, closestApproachTime, _orbit, candidate);
                    celestialObjectPatchedInto = candidate.celestialObject;
                    patchedUp = false;
                    candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                }
                else candidate.orbitRangeCrossingPoints.RemoveAt(0);
            }

            int itt = 0; // Juuuuuuuust in case, never trust a while loop. There should only ever be 2 of these checks max
            while (itt < 5 && candidate.orbitRangeCrossingPoints.Count > 0) {
                
                // From first point to firstPatchPoint
                if (candidate.orbitRangeCrossingPoints.Count == 1) {
                    
                    closestApproachTime = SearchForTimeOfClosestApproach(candidate.orbitRangeCrossingPoints[0].time, finalTime, _orbit, candidate.orbit);
                    var res = _orbit.GetCartesianAtTime(closestApproachTime);

                    if (_orbit.GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                        firstPatchPoint = SOIBoundarySearch(candidate.orbitRangeCrossingPoints[0].time, closestApproachTime, _orbit, candidate);
                        celestialObjectPatchedInto = candidate.celestialObject;
                        patchedUp = false;
                        candidate.orbitRangeCrossingPoints.Clear(); // Don't bother checking further
                    }
                    else candidate.orbitRangeCrossingPoints.RemoveAt(0);
                }
                
                // From first point to second point
                else {
                    
                    closestApproachTime = SearchForTimeOfClosestApproach(candidate.orbitRangeCrossingPoints[0].time, candidate.orbitRangeCrossingPoints[1].time, _orbit, candidate.orbit);
                    var res = _orbit.GetCartesianAtTime(closestApproachTime);

                    if (_orbit.GetDistanceFromSiblingOrbitAtTime(candidate.orbit, closestApproachTime) < candidate.SOIdistance) {
                        firstPatchPoint = SOIBoundarySearch(candidate.orbitRangeCrossingPoints[0].time, closestApproachTime, _orbit, candidate);
                        celestialObjectPatchedInto = candidate.celestialObject;
                        patchedUp = false;
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

        return (firstPatchPoint != null, firstPatchPoint, celestialObjectPatchedInto, patchedUp);
    }


    #endregion
//--#



//--#
    #region Crossover points


    public static void CalculateOrbitCrossoverPointsForCandidate(Orbit _orbit, PatchingCandidate _candidate, OrbitPoint? _firstPatchPoint) {

        double time1;
        double time2;


        // Get the two points where the main orbit crosses the minimum orbital radius for an encounter
        try{ 

            double ta = _orbit.GetTrueAnomalyAtDistance(_candidate.orbit.periapsis - _candidate.SOIdistance);
            if (ta != -1) {
                time1 = _orbit.GetTimeAtTrueAnomaly(ta);
                if ((time1 < _firstPatchPoint?.time || _firstPatchPoint == null) && double.IsFinite(time1)) {
                    var res1 = _orbit.GetCartesianAtTrueAnomaly(ta);
                    _candidate.orbitRangeCrossingPoints.Add(new(res1.localPos, res1.localVel, ta, time1));
                }

                time2 = _orbit.GetTimeAtTrueAnomaly(-ta);
                if ((time2 < _firstPatchPoint?.time || _firstPatchPoint == null) && double.IsFinite(time2)) {
                    var res2 = _orbit.GetCartesianAtTrueAnomaly(-ta);
                    _candidate.orbitRangeCrossingPoints.Add(new(res2.localPos, res2.localVel, -ta, time2));
                }
            }

        }
        catch (System.Exception){}


        // Get the two points where the main orbit crosses the maximum orbital radius for an encounter
        try{ 

            double ta = _orbit.GetTrueAnomalyAtDistance(_candidate.orbit.apoapsis + _candidate.SOIdistance);
            if (ta != -1) {
                time1 = _orbit.GetTimeAtTrueAnomaly(ta);
                if ((time1 < _firstPatchPoint?.time || _firstPatchPoint == null) && double.IsFinite(time1)) {
                    var res1 = _orbit.GetCartesianAtTrueAnomaly(ta);
                    _candidate.orbitRangeCrossingPoints.Add(new(res1.localPos, res1.localVel, ta, time1));
                }

                time2 = _orbit.GetTimeAtTrueAnomaly(-ta);
                if ((time2 < _firstPatchPoint?.time || _firstPatchPoint == null) && double.IsFinite(time2)) {
                    var res2 = _orbit.GetCartesianAtTrueAnomaly(-ta);
                    _candidate.orbitRangeCrossingPoints.Add(new(res2.localPos, res2.localVel, -ta, time2));
                }
            }

        }
        catch (System.Exception){}

        _candidate.orbitRangeCrossingPoints.Sort();
        
    }


    #endregion
//--#



//--#
    #region Closest approach


    public static double SearchForTimeOfClosestApproach(double _startTime, double _endTime, Orbit _orbit, Orbit _orbitOther) {

        // Uses a golden section search to find the closest approach

        double f(double t) => _orbit.GetDistanceFromSiblingOrbitAtTime(_orbitOther, t);
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


    #endregion
//--#



//--#
    #region Boundary search


    public static OrbitPoint SOIBoundarySearch(double _startTime, double _endTime, Orbit _orbit, PatchingCandidate _candidate) {

        // Uses simple bisection method to find point where distance = SOI

        double f(double t) => _orbit.GetDistanceFromSiblingOrbitAtTime(_candidate.orbit, t);
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

        var res = _orbit.GetCartesianAtTime(b);
        double trueAnomaly = _orbit.GetTrueAnomalyAtTime(b);
        if (trueAnomaly <= _orbit.startingTrueAnomaly) {trueAnomaly += 2 * Mathd.PI;}

        return new OrbitPoint(res.localPos, res.localVel, trueAnomaly, b);

    }


    #endregion
//--#
}