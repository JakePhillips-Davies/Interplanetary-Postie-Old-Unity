using System;
using System.Collections.Generic;

namespace Orbits
{

/* 
    #==============================================================#

    Struct for containing a patching candidate of a celestial
    object.


*/
[Serializable]
public struct PatchingCandidate {
    public Orbit orbit;
    public double SOIdistance;
    public List<OrbitPoint> orbitRangeCrossingPoints;

    public PatchingCandidate(Orbit _orbit, double _SOIdistance) {
        orbit = _orbit;
        SOIdistance = _SOIdistance;

        orbitRangeCrossingPoints = new();
    }
}

}