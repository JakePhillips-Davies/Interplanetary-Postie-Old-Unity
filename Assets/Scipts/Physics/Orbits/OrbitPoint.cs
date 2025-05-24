using System;
using UnityEngine;

namespace Orbits
{

/* 
    #==============================================================#
    
    Struct for containing a point on an orbit and the time at said
    point.


*/
[Serializable]
public struct OrbitPoint : IComparable<OrbitPoint> {
    public Vector3d position;
    public Vector3d velocity;
    public double trueAnomaly;
    public double time;

    public OrbitPoint(Vector3d _position, Vector3d _velocity, double _trueAnomaly, double _time) {
        position = _position;
        velocity = _velocity;
        trueAnomaly = _trueAnomaly;
        time = _time;
    }

    public readonly int CompareTo(OrbitPoint other) {
        if (this.time > other.time) return 1;
        else if (this.time < other.time) return -1;
        else return 0;
    }
}

}