using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;

namespace Orbits
{

/*
    #==============================================================#
	
	
	
	
*/
[ExecuteInEditMode]
public class OrbitTester : MonoBehaviour
{    
//--#
    #region Variables


    [field: Title("Tests")]
    [field: SerializeField] public Vector3 pos {get; private set;}
    [field: SerializeField] public Vector3 vel {get; private set;}
    

    [Button] public void Test_Kepler() => TestKep();
    [Button] public void Log_Orbit() => Debug.Log(orbit);
    [Button] public void Test_Newtonian() => TestNewt();
    [Button] public void Test_Deviation() => TestDeviation();

    private Orbit orbit;
    List<Vector3> points;
    List<Vector3> pointsNewton;
    List<Vector3> inclinationAngle;
    List<Vector3> arguementOfPeriapseAngle;
    private Vector3d perN;
    private Vector3d apoN;


    #endregion
//--#



//--#
    #region Unity events


    private void OnValidate() {
        TestKep();
    }
    private void OnDrawGizmos() {
        if (points == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < points.Count; i++) {
            if (i != points.Count-1)
                Gizmos.DrawLine(points[i], points[i + 1]);
            else
                Gizmos.DrawLine(points[i], points[0]);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.GetPeriapsisVector() / 10000000));
        Gizmos.DrawSphere((Vector3)(orbit.GetPeriapsisVector() / 10000000), 0.1f);
        Gizmos.DrawWireSphere((Vector3)(orbit.GetPeriapsisVector() / 10000000), 1);
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.GetApoapsisVector() / 10000000));
        Gizmos.DrawSphere((Vector3)(orbit.GetApoapsisVector() / 10000000), 0.1f);
        Gizmos.DrawWireSphere((Vector3)(orbit.GetApoapsisVector() / 10000000), 1);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos / 10000000));
        Gizmos.DrawSphere((Vector3)(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos / 10000000), 0.1f);
        Gizmos.DrawWireSphere((Vector3)(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos / 10000000), 1);
        Gizmos.DrawLine((Vector3)(orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos / 10000000), (Vector3)((orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localPos / 10000000) + (orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time).localVel / 100)));
        Gizmos.DrawLine(Vector3.zero, pos / 10000000);
        Gizmos.DrawSphere(pos / 10000000, 0.1f);
        Gizmos.DrawWireSphere(pos / 10000000, 1);
        Gizmos.DrawLine(pos / 10000000, (pos / 10000000) + (vel / 100));
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.GetAscendingNodeVector() / 10000000));
        Gizmos.DrawSphere((Vector3)(orbit.GetAscendingNodeVector() / 10000000), 0.1f);
        Gizmos.DrawWireSphere((Vector3)(orbit.GetAscendingNodeVector() / 10000000), 1);
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.GetDescendingNodeVector() / 10000000));
        Gizmos.DrawSphere((Vector3)(orbit.GetDescendingNodeVector() / 10000000), 0.1f);
        Gizmos.DrawWireSphere((Vector3)(orbit.GetDescendingNodeVector() / 10000000), 1);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(orbit.angularMomentumVector.normalized * 20));
        Gizmos.color = Color.white;
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 1, 0) * 20);

        Gizmos.color = Color.yellow;
        if (pointsNewton != null) {
            for (int i = 0; i < pointsNewton.Count; i++) {
                if (i != pointsNewton.Count-1)
                    Gizmos.DrawLine(pointsNewton[i], pointsNewton[i + 1]);
                else
                    Gizmos.DrawLine(pointsNewton[i], pointsNewton[0]);
            }

            Gizmos.DrawLine((Vector3)(perN / 10000000), (Vector3)(perN / 10000000) + (Vector3.up * 20));
            Gizmos.DrawLine((Vector3)(apoN / 10000000), (Vector3)(apoN / 10000000) + (Vector3.up * 20));
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, Quaternion.AngleAxis((float)(-orbit.rightAscensionOfAscendingNode * Mathd.Rad2Deg), Vector3.up) * Vector3.right);

        Gizmos.color = new Color(0.533f, 0.086f, 0.82f);
        if (inclinationAngle != null) {
            for (int i = 0; i < inclinationAngle.Count; i++) {
                if (i != inclinationAngle.Count-1)
                    Gizmos.DrawLine(inclinationAngle[i], inclinationAngle[i + 1]);
            }
        }
        Vector3d ninetyNode = orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis + Mathd.PI / 2).localPos;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(ninetyNode / 10000000));
        ninetyNode.y = 0;
        Gizmos.color = Color.white;
        Gizmos.DrawLine((Vector3)(orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis + Mathd.PI / 2).localPos / 10000000), (Vector3)(ninetyNode / 10000000));
        Gizmos.DrawLine(Vector3.zero, (Vector3)(ninetyNode / 10000000));
        
        Gizmos.color = new Color(0.533f, 0.086f, 0.82f);
        Vector3d negNinetyNode = orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis - Mathd.PI / 2).localPos;
        Gizmos.DrawLine(Vector3.zero, (Vector3)(negNinetyNode / 10000000));
        negNinetyNode.y = 0;
        Gizmos.color = Color.white;
        Gizmos.DrawLine((Vector3)(orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis - Mathd.PI / 2).localPos / 10000000), (Vector3)(negNinetyNode / 10000000));
        Gizmos.DrawLine(Vector3.zero, (Vector3)(negNinetyNode / 10000000));

        Gizmos.color = Color.blue;
        if (arguementOfPeriapseAngle != null) {
            for (int i = 0; i < arguementOfPeriapseAngle.Count; i++) {
                if (i != arguementOfPeriapseAngle.Count-1)
                    Gizmos.DrawLine(arguementOfPeriapseAngle[i], arguementOfPeriapseAngle[i + 1]);
            }
        }

        
    }


    #endregion
//--#



//--#
    #region Misc functions


    public void TestKep() {
        orbit = new Orbit();
        orbit.parent = new CelestialObject(5.9722E+24);
        orbit.CartesianToKeplerian(new Vector3d(pos), new Vector3d(vel));

        double tA = 0;
        double tAStep = 2 * Mathd.PI / 250;
        points = new List<Vector3>();

        for (int i = 0; i < 250; i++) {
            
            var res = orbit.GetCartesianAtTrueAnomaly(tA);
            tA += tAStep;
            
            points.Add((Vector3)(res.localPos / 10000000));
            
        }

        inclinationAngle = new List<Vector3>();
        Vector3d ascendingNode = orbit.GetAscendingNodeVector();
        Vector3d ninetyNode;
        if (orbit.inclination <= Mathd.PI / 2)
            ninetyNode = orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis + Mathd.PI / 2).localPos;
        else ninetyNode = orbit.GetCartesianAtTrueAnomaly(-orbit.argumentOfPeriapsis - Mathd.PI / 2).localPos;
        ninetyNode.y = 0;

        float angle = 0;
        float angleStep = (float)(orbit.inclination * Mathd.Rad2Deg / 25);
        for (int i = 0; i <= 25; i++) {
            inclinationAngle.Add(Quaternion.AngleAxis(-angle, (Vector3)ascendingNode.normalized) * (Vector3)(ninetyNode.normalized * (ninetyNode.magnitude / 20000000)));
            angle += angleStep;
        }


        arguementOfPeriapseAngle = new List<Vector3>();
        Vector3d angularMomentum = orbit.angularMomentumVector;

        angle = 0;
        angleStep = (float)(orbit.argumentOfPeriapsis * Mathd.Rad2Deg / 25);
        for (int i = 0; i <= 25; i++) {
            arguementOfPeriapseAngle.Add(Quaternion.AngleAxis(-angle, (Vector3)angularMomentum.normalized) * (Vector3)(ascendingNode.normalized * (orbit.periapsis / 20000000)));
            angle += angleStep;
        }

    }

    public void TestNewt() {
        // Run a newtonian simulation to check against
        pointsNewton = new List<Vector3>();
        Vector3d pos = new Vector3d(this.pos);
        Vector3d vel = new Vector3d(this.vel);

        perN = Vector3d.up * 5346957863497865397466957324d;
        apoN = Vector3d.zero;

        double time = 0;
        double timeStep = orbit.period / 20000;
        
        for (int i = 0; i < 20000; i++) {
            pointsNewton.Add((Vector3)(pos / 10000000));
            double sqrMag = pos.sqrMagnitude;

            if (sqrMag < perN.sqrMagnitude) perN = pos;
            if (sqrMag > apoN.sqrMagnitude) apoN = pos;

            time += timeStep;
            Vector3d acceleration = -pos.normalized * (3.986004418e+14 / sqrMag);
            vel += acceleration * timeStep;
            pos += vel * timeStep;
        }
    }

    public void TestDeviation() {
        Vector3d pos = orbit.GetCartesianAtTime(0).localPos;
        Vector3d vel = orbit.GetCartesianAtTime(0).localVel;
        Vector3d pointKep;

        double time = 0;
        double timeStep = Time.fixedDeltaTime;
        for (int i = 0; i <= 25000; i++) {
            pointKep = orbit.GetCartesianAtTime(time).localPos;
            if (i % 2500 == 0 || i == 1) Debug.Log("-- Deviation after " + time + " second(s): " + ((pos - pointKep).magnitude));

            time += timeStep;

            double sqrMag = pos.sqrMagnitude;
            
            Vector3d acceleration = -pos.normalized * (3.986004418e+14 / sqrMag);
            vel += acceleration * timeStep;
            pos += vel * timeStep;
        }

        Orbit _orbit = new Orbit();
        orbit.parent = new CelestialObject(5.9722E+24);

        _orbit.CartesianToKeplerian(pos, vel);

        Debug.Log(orbit);
        Debug.Log(_orbit);
    }


    #endregion
//--#
}

}