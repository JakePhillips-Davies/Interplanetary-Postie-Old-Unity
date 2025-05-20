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
    public double time;

    public OrbitPoint(Vector3d position, double time) {
        this.position = position;
        this.time = time;
    }
}

}