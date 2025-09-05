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
    public List<OrbitPoint> orbitRangeCrossingPoints;
    public Orbit orbit;
    public CelestialObject celestialObject;
    public double SOIdistance;
    
    public PatchingCandidate(Orbit _orbit, CelestialObject _celestialObject) {
        orbit = _orbit;
        celestialObject = _celestialObject;
        SOIdistance = celestialObject.SOIdistance;

        orbitRangeCrossingPoints = new();
    }
}

}