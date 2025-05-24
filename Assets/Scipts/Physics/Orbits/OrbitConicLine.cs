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
    public int orbitDetail;
    public Orbit orbit;

    public double startTrueAnomaly;
    public double endTrueAnomaly;


    #endregion
//--#



//--#
    #region Constructors


    public OrbitConicLine(Orbit _orbit, int _orbitDetail) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        startTrueAnomaly = _orbit.startingTrueAnomaly;
        endTrueAnomaly = _orbit.endingTrueAnomaly;

        points = new OrbitPoint[orbitDetail];

        UpdateOrbitPoints();

    }

    public OrbitConicLine(Orbit _orbit, int _orbitDetail, double _startTrueAnomaly, double _endTrueAnomaly) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        startTrueAnomaly = _startTrueAnomaly;
        endTrueAnomaly = _endTrueAnomaly;

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

            points[i] = new OrbitPoint(pos, vel, trueAnomaly, time);

            trueAnomaly += trueAnomalyStep;

        }

    }


    #endregion
//--#
}

}