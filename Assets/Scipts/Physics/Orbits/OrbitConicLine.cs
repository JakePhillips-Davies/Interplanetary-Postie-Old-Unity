using System;
using UnityEngine;

namespace Orbits
{

/* 
    #==============================================================#
    
    Struct for containing an array of points on an orbit from it's
    starting TA to an ending TA.


*/
[Serializable]
public struct OrbitConicLine {

//--#
    #region Variables


    public OrbitPoint[] points;
    public OrbitPoint startPoint;
    public OrbitPoint endPoint;

    public int orbitDetail;
    public double startTrueAnomaly;
    public double endTrueAnomaly;
    public Orbit orbit;
    

    #endregion
//--#



//--#
    #region Constructors


    public OrbitConicLine(Orbit _orbit, int _orbitDetail) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        startTrueAnomaly = _orbit.startingTrueAnomaly;
        endTrueAnomaly = _orbit.endingTrueAnomaly;

        var startRes = orbit.GetCartesianAtTrueAnomaly(startTrueAnomaly);
        startPoint = new OrbitPoint(startRes.localPos, startRes.localVel, startTrueAnomaly, orbit.GetTimeAtTrueAnomaly(startTrueAnomaly));
        var endRes = orbit.GetCartesianAtTrueAnomaly(endTrueAnomaly);
        endPoint = new OrbitPoint(endRes.localPos, endRes.localVel, endTrueAnomaly, orbit.GetTimeAtTrueAnomaly(endTrueAnomaly));

        points = new OrbitPoint[orbitDetail];

        UpdateOrbitPoints();

    }
    public OrbitConicLine(Orbit _orbit, int _orbitDetail, bool relativetoParent) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        startTrueAnomaly = _orbit.startingTrueAnomaly;
        endTrueAnomaly = _orbit.endingTrueAnomaly;

        var startRes = orbit.GetCartesianAtTrueAnomaly(startTrueAnomaly);
        startPoint = new OrbitPoint(startRes.localPos, startRes.localVel, startTrueAnomaly, orbit.GetTimeAtTrueAnomaly(startTrueAnomaly));
        var endRes = orbit.GetCartesianAtTrueAnomaly(endTrueAnomaly);
        endPoint = new OrbitPoint(endRes.localPos, endRes.localVel, endTrueAnomaly, orbit.GetTimeAtTrueAnomaly(endTrueAnomaly));

        points = new OrbitPoint[orbitDetail];

        UpdateOrbitPointsRelative();

    }

    public OrbitConicLine(Orbit _orbit, int _orbitDetail, double _startTrueAnomaly, double _endTrueAnomaly) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        startTrueAnomaly = _startTrueAnomaly;
        endTrueAnomaly = _endTrueAnomaly;

        var startRes = orbit.GetCartesianAtTrueAnomaly(startTrueAnomaly);
        startPoint = new OrbitPoint(startRes.localPos, startRes.localVel, startTrueAnomaly, orbit.GetTimeAtTrueAnomaly(startTrueAnomaly));
        var endRes = orbit.GetCartesianAtTrueAnomaly(endTrueAnomaly);
        endPoint = new OrbitPoint(endRes.localPos, endRes.localVel, endTrueAnomaly, orbit.GetTimeAtTrueAnomaly(endTrueAnomaly));

        points = new OrbitPoint[orbitDetail];

        UpdateOrbitPoints();

    }


    #endregion
//--#



//--#
    #region Update points


    public void UpdateOrbitPoints() {

        double time;
        Vector3d pos;
        Vector3d vel;
        double trueAnomaly = startTrueAnomaly;
        double trueAnomalyStep = (endTrueAnomaly - startTrueAnomaly) / (orbitDetail - 1);
        for (int i = 0; i < orbitDetail; i++) {

            pos = orbit.GetCartesianAtTrueAnomaly(trueAnomaly).localPos;
            vel = orbit.GetCartesianAtTrueAnomaly(trueAnomaly).localVel;
            time = orbit.GetTimeAtTrueAnomaly(trueAnomaly);
            if (i == 0) time = orbit.epoch;

            points[i] = new OrbitPoint(pos, vel, trueAnomaly, time);

            trueAnomaly += trueAnomalyStep;

        }

    }
    public void UpdateOrbitPointsRelative() {

        double time;
        Vector3d pos;
        Vector3d vel;
        double trueAnomaly = startTrueAnomaly;
        double trueAnomalyStep = (endTrueAnomaly - startTrueAnomaly) / (orbitDetail - 1);
        orbit.parent.simTransform.TryGetSimComponent(out OrbitDriver parentDriver);
        for (int i = 0; i < orbitDetail; i++) {

            var res = orbit.GetCartesianAtTrueAnomaly(trueAnomaly);
            pos = res.localPos;
            vel = res.localVel;
            time = orbit.GetTimeAtTrueAnomaly(trueAnomaly);
            if (i == 0) {
                time = orbit.epoch;
                startPoint.time = orbit.epoch;
            }

            var res2 = parentDriver.orbits[0].GetCartesianAtTime(time);

            points[i] = new OrbitPoint(pos + res2.localPos, vel + res2.localVel, trueAnomaly, time);

            trueAnomaly += trueAnomalyStep;

            if (i == 0) startPoint.position += res2.localPos;
            if (i == orbitDetail - 1) endPoint.position += res2.localPos;

        }

    }


    #endregion
//--#
}

}