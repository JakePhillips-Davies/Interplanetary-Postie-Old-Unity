using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Orbits
{

/*
    #==============================================================#
	
	
	
	
*/
[Serializable]
public class Orbit
{    
//--#
    #region Variables


    // Orbital prerameters
    public double periapsis;
    public double apoapsis;

    public double semiMajorAxis;
    public double inclination;
    public double eccentricity;
    public Vector3d eccentricityVector;
    public double argumentOfPeriapsis;
    public double rightAscensionOfAscendingNode;

    public double startingTrueAnomaly;

    public CelestialObject parent;


    // Time
    public double epoch;
    public double meanAnomalyAtEpoch;
    public double orbitEndTime;
    public double period;


    // Misc
    public double meanMotion;

    public double angularMomentum;
    public Vector3d angularMomentumVector;

    public double SOIdistance;

    private double distance;
    private double velSqr;
    private double p;


    #endregion
//--#



//--#
    #region Constructors


    public Orbit() {}
    public Orbit(Vector3d _pos, Vector3d _vel, CelestialObject _parent, double _epoch) {
        parent = _parent;
        epoch = _epoch;
        CartesianToKeplerian(_pos, _vel);
    }
    public Orbit(double _periapsis, double _eccentricity, double _inclination, double _rightAscensionOfAscendingNode, double _argumentOfPeriapsis, double _trueAnomaly, CelestialObject _parent, double _epoch) {
        periapsis = _periapsis;
        eccentricity = _eccentricity;
        if (eccentricity == 1) eccentricity = 1.0000000001d;
        inclination = _inclination;
        rightAscensionOfAscendingNode = _rightAscensionOfAscendingNode;
        argumentOfPeriapsis = _argumentOfPeriapsis;
        startingTrueAnomaly = _trueAnomaly;
        epoch = _epoch;
        meanAnomalyAtEpoch = ConicMath.TrueAnomalyToMeanAnomaly(startingTrueAnomaly, eccentricity);
        semiMajorAxis = periapsis / (1 - eccentricity + double.Epsilon);

        parent = _parent;
        angularMomentum = Mathd.Sqrt(parent.gravitationalParameter * periapsis * (1.0 + eccentricity));

        var res = GetCartesianAtTrueAnomaly(startingTrueAnomaly);
        CartesianToKeplerian(res.localPos, res.localVel);
    }


    #endregion
//--#



//--#
    #region Kepler to Cartesian


    public (Vector3d localPos, Vector3d localVel) GetCartesianAtTime(double time) {

        meanMotion = GetMeanMotionFromKeplerian();

        double newMeanAnomaly = meanAnomalyAtEpoch + (time - epoch) * meanMotion;
        if (eccentricity < 1.0) newMeanAnomaly = Mathd.IEEERemainder(newMeanAnomaly, 2.0 * Mathd.PI);

        double newTrueAnomaly = ConicMath.MeanAnomalyToTrueAnomaly(newMeanAnomaly, eccentricity, this.startingTrueAnomaly);

        var results = GetCartesianAtTrueAnomaly(newTrueAnomaly);

        return (results.localPos, results.localVel);
    }
    public (Vector3d localPos, Vector3d localVel) GetCartesianAtTrueAnomaly(double _trueAnomaly) {

        // Updating the distance.
        double distance = periapsis * (1.0 + eccentricity) / (1.0 + eccentricity * Mathd.Cos(_trueAnomaly));

        // Position
        double X = distance * (Mathd.Cos(rightAscensionOfAscendingNode) * Mathd.Cos(argumentOfPeriapsis + _trueAnomaly) 
                  - Mathd.Sin(rightAscensionOfAscendingNode) * Mathd.Sin(argumentOfPeriapsis + _trueAnomaly) * Mathd.Cos(inclination));

        double Z = distance * (Mathd.Sin(rightAscensionOfAscendingNode) * Mathd.Cos(argumentOfPeriapsis + _trueAnomaly)
                  + Mathd.Cos(rightAscensionOfAscendingNode) * Mathd.Sin(argumentOfPeriapsis + _trueAnomaly) * Mathd.Cos(inclination));

        double Y = distance * (Mathd.Sin(inclination) * Mathd.Sin(argumentOfPeriapsis + _trueAnomaly));

        // Velocity
        double p = semiMajorAxis * (1 - eccentricity * eccentricity);
        if (eccentricity == 1 || double.IsInfinity(semiMajorAxis)) p = float.Epsilon;

        double V_X = (X * angularMomentum * eccentricity / (distance * p)) * Mathd.Sin(_trueAnomaly)
                    - (angularMomentum / distance) * (Mathd.Cos(rightAscensionOfAscendingNode) * Mathd.Sin(argumentOfPeriapsis + _trueAnomaly)
                    + Mathd.Sin(rightAscensionOfAscendingNode) * Mathd.Cos(argumentOfPeriapsis + _trueAnomaly) * Mathd.Cos(inclination));

        double V_Z = (Z * angularMomentum * eccentricity / (distance * p)) * Mathd.Sin(_trueAnomaly) 
                    - (angularMomentum / distance) * (Mathd.Sin(rightAscensionOfAscendingNode) * Mathd.Sin(argumentOfPeriapsis + _trueAnomaly) 
                    - Mathd.Cos(rightAscensionOfAscendingNode) * Mathd.Cos(argumentOfPeriapsis + _trueAnomaly) * Mathd.Cos(inclination));

        double V_Y = (Y * angularMomentum * eccentricity / (distance * p)) * Mathd.Sin(_trueAnomaly)
                    + (angularMomentum / distance) * (Mathd.Cos(argumentOfPeriapsis + _trueAnomaly) * Mathd.Sin(inclination));

        Vector3d localPos = new(X, Y, Z);
        Vector3d localVel = new(V_X, V_Y, V_Z);

        return (localPos, localVel);
    }

    public Vector3d GetPeriapsisVector() {
        return GetCartesianAtTrueAnomaly(0).localPos;
    }
    public Vector3d GetApoapsisVector() {
        return GetCartesianAtTrueAnomaly(Mathd.PI).localPos;
    }
    public Vector3d GetAscendingNodeVector() {
        return GetCartesianAtTrueAnomaly(-argumentOfPeriapsis).localPos;
    }
    public Vector3d GetDescendingNodeVector() {
        return GetCartesianAtTrueAnomaly(Mathd.PI - argumentOfPeriapsis).localPos;
    }

    public double GetTimeAtTrueAnomaly(double trueAnomaly) {
        double meanAnomaly = ConicMath.TrueAnomalyToMeanAnomaly(trueAnomaly, eccentricity);
        meanMotion = GetMeanMotionFromKeplerian(); // Just in case
        return (meanAnomaly - meanAnomalyAtEpoch) / meanMotion;
    }


    #endregion
//--#



//--#
    #region Cartesian to Kepler


    public void CartesianToKeplerian(Vector3d pos, Vector3d vel) {

        distance = pos.magnitude;
        velSqr = vel.sqrMagnitude;

        angularMomentumVector = -Vector3d.Cross(pos, vel);
        angularMomentum = angularMomentumVector.magnitude;

        inclination = Mathd.Acos(Mathd.Clamp((angularMomentumVector.y / angularMomentum), -1d, 1d)); // Acos must be between -1 and 1

        Vector3d N = -Vector3d.Cross(new(0, 1, 0), angularMomentumVector);
        double NMag = N.magnitude;

        rightAscensionOfAscendingNode = Mathd.Acos(Mathd.Clamp((N.x / (NMag + double.Epsilon)), -1d, 1d)); // Acos must be between -1 and 1
        if (N.z < 0) rightAscensionOfAscendingNode = 2 * Mathd.PI - rightAscensionOfAscendingNode;

        eccentricityVector = (((velSqr / parent.gravitationalParameter) - (1 / distance)) * pos) - (Vector3d.Dot(pos, vel) / parent.gravitationalParameter) * vel;
        eccentricity = eccentricityVector.magnitude;

        meanMotion = GetMeanMotionFromKeplerian();
        period = 2 * Mathd.PI / meanMotion;

        argumentOfPeriapsis = Mathd.Acos(Mathd.Clamp(Vector3d.Dot(N, eccentricityVector) / (NMag * eccentricity + double.Epsilon), -1d, 1d)); // Acos must be between -1 and 1
        if (eccentricityVector.y < 0) argumentOfPeriapsis = 2 * Mathd.PI - argumentOfPeriapsis;

        startingTrueAnomaly = Mathd.Acos(Mathd.Clamp(Vector3d.Dot(eccentricityVector, pos) / (eccentricity * distance + double.Epsilon), -1d, 1d)); // Acos must be between -1 and 1
        if (Vector3d.Dot(pos / distance, vel) < 0) startingTrueAnomaly = 2 * Mathd.PI - startingTrueAnomaly;
        
        meanAnomalyAtEpoch = ConicMath.TrueAnomalyToMeanAnomaly(startingTrueAnomaly, eccentricity);
        epoch = 0; // TODO: link to UT

        semiMajorAxis = -parent.gravitationalParameter / (2 * (0.5 * velSqr - parent.gravitationalParameter / distance));

        periapsis = (angularMomentum * angularMomentum) / (parent.gravitationalParameter * (1 + eccentricity));
        apoapsis = (eccentricity >= 1)? Mathd.Infinity : -periapsis + 2 * semiMajorAxis;

    }


    #endregion
//--#



//--#
    #region --


    


    #endregion
//--#



//--#
    #region Misc methods


    public double GetMeanMotionFromKeplerian() {

        double multiplier = (eccentricity == 1.0) ? 1.0 : Mathd.Abs(1.0 - eccentricity * eccentricity);
        multiplier = Mathd.Sqrt(multiplier*multiplier*multiplier);
        return multiplier * parent.gravitationalParameter * parent.gravitationalParameter / (angularMomentum*angularMomentum*angularMomentum);
    }

    public override String ToString() {
        return "Orbit: " + "\n" +
               "Periapsis: " + periapsis + "\n" +
               "Apoapsis: " + apoapsis + "\n" +
               "SemiMajorAxis: " + semiMajorAxis + "\n" +
               "Inclination: " + inclination + "\n" +
               "Eccentricity: " + eccentricity + "\n" +
               "ArgumentOfPeriapsis: " + argumentOfPeriapsis + "\n" +
               "RightAscensionOfAscendingNode: " + rightAscensionOfAscendingNode + "\n" +
               "TrueAnomaly: " + startingTrueAnomaly + "\n" +
               "parent.gravitationalParameter: " + parent.gravitationalParameter + "\n" +
               "Epoch: " + epoch + "\n" +
               "MeanAnomalyAtEpoch: " + meanAnomalyAtEpoch + "\n" +
               "OrbitEndTime: " + orbitEndTime + "\n" +
               "MeanMotion: " + meanMotion + "\n" +
               "AngularMomentum: " + angularMomentum + "\n" +
               "Distance: " + distance + "\n";
    }


    #endregion
//--#
}


}