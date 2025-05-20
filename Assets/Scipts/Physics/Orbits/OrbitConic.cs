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
public struct OrbitConic {

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


    public OrbitConic(Orbit _orbit, int _orbitDetail) {

        orbit = _orbit;
        orbitDetail = _orbitDetail;
        if (orbit.eccentricity < 1) {
            startTrueAnomaly = _orbit.startingTrueAnomaly;
            endTrueAnomaly = _orbit.startingTrueAnomaly + 2 * Mathd.PI;
        }
        else {
            double taInf = Mathd.Acos(-1 / orbit.eccentricity); // True anomaly at infinity on a para/hyperbolic orbit
            startTrueAnomaly = -taInf * 0.9999d;
            endTrueAnomaly = taInf * 0.9999d;
        }

        points = new OrbitPoint[orbitDetail];

        UpdateOrbitPoints();

    }

    public OrbitConic(Orbit _orbit, int _orbitDetail, double _startTrueAnomaly, double _endTrueAnomaly) {

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
        double trueAnomaly = startTrueAnomaly;
        double trueAnomalyStep = (endTrueAnomaly - startTrueAnomaly) / (orbitDetail - 1);
        for (int i = 0; i < orbitDetail; i++) {

            pos = orbit.GetCartesianAtTrueAnomaly(trueAnomaly).localPos;
            time = orbit.GetTimeAtTrueAnomaly(trueAnomaly);

            points[i] = new OrbitPoint(pos, time);

            trueAnomaly += trueAnomalyStep;

        }

    }


    #endregion
//--#
}

}