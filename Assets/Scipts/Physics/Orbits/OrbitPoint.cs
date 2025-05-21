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
public struct OrbitPoint {
    public Vector3d position;
    public Vector3d velocity;
    public double time;

    public OrbitPoint(Vector3d _position, Vector3d _velocity, double _time) {
        position = _position;
        velocity = _velocity;
        time = _time;
    }
}

}