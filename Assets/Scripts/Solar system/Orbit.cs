using System;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Orbit : MonoBehaviour
{
    //
    /*
        Notes on Keplerian orbital elements

        G - gravitational constant

        a - semi-major axis
        eccentricity - eccentricity
        inclination - inclination
        AP - arguement of periapsis - ω
        LAN - longitude of the ascending node - Ω

        MA - mean anomaly
        EA - eccentric anomaly
        TA - true anomaly

        FoR - frame of reference

        h - specific angular momentum

    */
    [SerializeField] private double semiMajorAxis;
    [SerializeField] private double eccentricity;
    [SerializeField] private double inclination;
    [SerializeField] private double AP;
    [SerializeField] private double LAN;

    
    [SerializeField] private AstralBody testMainBody;
    [SerializeField] private int testItt;


    //

    private void FixedUpdate() {
        //tester

        
        Debug.Log("------------------------------------------------------------------------------------------------------------");
        double X =-1.250467672661996E+08, Y = 7.927671170785488E+07, Z = 2.225500574263185E+04,
               VX=-1.660470820920962E+01, VY=-2.514752499084350E+01, VZ= 2.373935085678625E-03;

        var a = CartesianToKepler(new Vector3d(X*1000, Y*1000, Z*1000), new Vector3d(VX*1000, VY*1000, VZ*1000), testMainBody);
        Debug.Log(a);
        Debug.Log(KeplerToCartesian(a.semiMajorAxis, a.eccentricity, a.inclination, a.AP, a.LAN, a.period, testMainBody));


        Debug.Log("------------------------------------------------------------------------------------------------------------");
        X = 23E+08; Y = -9E+07; Z = 5.75+08;
        VX= 0.3E+01; VY= 1.65E+01; VZ= -1E-01;

        a = CartesianToKepler(new Vector3d(X*1000, Y*1000, Z*1000), new Vector3d(VX*1000, VY*1000, VZ*1000), testMainBody);
        Debug.Log(a);
        Debug.Log(KeplerToCartesian(a.semiMajorAxis, a.eccentricity, a.inclination, a.AP, a.LAN, a.period, testMainBody));

        Debug.Log("------------------------------------------------------------------------------------------------------------");
        X = 23E+08; Y = -9E+07; Z = 5.75+08;
        VX= 0.3E+01; VY= 1.65E+05; VZ= -1E-01;

        a = CartesianToKepler(new Vector3d(X*1000, Y*1000, Z*1000), new Vector3d(VX*1000, VY*1000, VZ*1000), testMainBody);
        Debug.Log(a);
        Debug.Log(KeplerToCartesian(a.semiMajorAxis, a.eccentricity, a.inclination, a.AP, a.LAN, a.period, testMainBody));

    }

    (double semiMajorAxis, double eccentricity, double inclination, double AP, double LAN, double period) CartesianToKepler( Vector3d worldPos, Vector3d vel, AstralBody reference ) {

        Vector3d refPos = reference.position;
        Vector3d pos = worldPos - refPos;

        double mu = reference.mu;

        // Compute the specific angular momentum
        Vector3d h = Vector3d.Cross( pos, vel );
        double hMag = h.magnitude;

        // Compute the radius, r, and velocity squared, v2
        double r = pos.magnitude;
        double v2 = vel.x*vel.x + vel.y*vel.y + vel.z*vel.z;

        // Compute the specific energy
        double E = v2/2 - mu/r;

        // Compute semi-major axis
        double semiMajorAxis = -mu/( 2*E );

        // Compute eccentricity and it's vector, eccVec
        Vector3d eccVec = (v2 / mu - 1.0 / r) * pos - Vector3d.Dot(pos, vel) * vel / mu;
        double eccentricity = eccVec.magnitude;

        // Compute inclination
        double inclination = Math.Acos( h.z/hMag )*Mathf.Rad2Deg;

        // Compute longitude of the ascending node, LAN, and vector of ascending node, n
        Vector3d n = new(-h.y, h.x, 0);
        double nMag = n.magnitude;

        double LAN = Math.Acos( n.x/nMag );
        if(n.y > 0) LAN = ( 2*Math.PI ) - LAN;
        LAN *= Mathf.Rad2Deg;

        // Compute argument of periapse
        double AP = Math.Acos( Vector3d.Dot(n, eccVec)/(nMag * eccentricity) );
        if(eccVec.z > 0) AP = (2 * Math.PI) - AP;
        AP *= Mathf.Rad2Deg;

        double PeR = (1.0 - eccentricity) * semiMajorAxis;
		double ApR = (1.0 + eccentricity) * semiMajorAxis;
		double period = Math.PI * 2.0 * Math.Sqrt(Math.Pow(Math.Abs(semiMajorAxis), 3.0) / mu);

        return (semiMajorAxis, eccentricity, inclination, AP, LAN, period);
    }


    (Vector3d pos, Vector3d vel) KeplerToCartesian(double semiMajorAxis, double eccentricity, double inclination, double AP, double LAN, double period, AstralBody reference) {

        double mu = reference.mu;

        double time = UT.UniverseTime; 
        time %= period;

		double meanAnomaly = time / period * 2.0 * Math.PI;

		double eccAnomaly = (eccentricity < 0.9) ? SolveEccentricAnomaly(meanAnomaly, eccentricity)
                          : ((eccentricity >= 1.0) ? SolveEccentricAnomalyHyp(meanAnomaly, eccentricity) 
                          : SolveEccentricAnomalyExtremeEcc(meanAnomaly, eccentricity));

        // Compute true anomaly and radius
		double trueAnomaly = Math.Acos((Math.Cos(eccAnomaly) - eccentricity) / (1 - eccentricity*Math.Cos(eccAnomaly)));

        double r = semiMajorAxis / (1.0 - eccentricity * Math.Cos(eccAnomaly));

        // Compute the specific angular momentum
        double h = Math.Sqrt( mu * semiMajorAxis * ( 1 - eccentricity * eccentricity ) );

        // Compute position
        Vector3d pos = new( r*(Math.Cos(LAN)*Math.Cos(AP+trueAnomaly) - Math.Sin(LAN)*Math.Sin(AP+trueAnomaly)*Math.Cos(inclination)),
                            r*(Math.Sin(LAN)*Math.Cos(AP+trueAnomaly) + Math.Cos(LAN)*Math.Sin(AP+trueAnomaly)*Math.Cos(inclination)),
                            r*(Math.Sin(inclination)*Math.Sin(AP+trueAnomaly)));

        // Compute velocity
        double p = semiMajorAxis * ( 1 - eccentricity*eccentricity );

        Vector3d vel = new( pos.x*h*eccentricity/(r*p)*Math.Sin(trueAnomaly) - h/r*(Math.Cos(LAN)*Math.Sin(AP+trueAnomaly) + Math.Sin(LAN)*Math.Cos(AP+trueAnomaly)*Math.Cos(inclination)),
                            pos.y*h*eccentricity/(r*p)*Math.Sin(trueAnomaly) - h/r*(Math.Sin(LAN)*Math.Sin(AP+trueAnomaly) - Math.Cos(LAN)*Math.Cos(AP+trueAnomaly)*Math.Cos(inclination)),
                            pos.z*h*eccentricity/(r*p)*Math.Sin(trueAnomaly) + h/r*(Math.Cos(AP+trueAnomaly)*Math.Sin(inclination)));

        vel.y = -vel.y; // Don't ask why

        return (pos, vel);
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Big solving happens here
    double SolveEccentricAnomaly(double meanAnomaly, double eccentricity) {
        double maxError = 1E-7;

        double num = 1.0;
        double num2 = meanAnomaly + eccentricity * Math.Sin(meanAnomaly) + 0.5 * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly);

        while (Math.Abs(num) > maxError)
        {
            double num3 = num2 - eccentricity * Math.Sin(num2);
            num = (meanAnomaly - num3) / (1.0 - eccentricity * Math.Cos(num2));
            num2 += num;
        }

        return num2;
    }
    double SolveEccentricAnomalyHyp(double meanAnomaly, double eccentricity) {
        double maxError = 1E-7;

        double num = 1.0;
        double num2 = Math.Log(2.0 * meanAnomaly / eccentricity + 1.8);

        while (Math.Abs(num) > maxError)
        {
            num = (eccentricity * Math.Sinh(num2) - num2 - meanAnomaly) / (eccentricity * Math.Cosh(num2) - 1.0);
		    num2 -= num;
        }

        return num2;
    }
    double SolveEccentricAnomalyExtremeEcc(double meanAnomaly, double eccentricity) {
        int iterations = 8;

        double num = meanAnomaly + 0.85 * eccentricity * (double)Math.Sign(Math.Sin(meanAnomaly));
        for (int i = 0; i < iterations; i++)
        {
            double num2 = eccentricity * Math.Sin(num);
            double num3 = eccentricity * Math.Cos(num);
            double num4 = num - num2 - meanAnomaly;
            double num5 = 1.0 - num3;
            double num6 = num2;
            num += -5.0 * num4 / (num5 + (double)Math.Sign(num5) * Math.Sqrt(Math.Abs(16.0 * num5 * num5 - 20.0 * num4 * num6)));
        }

        return num;
    } 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private AstralBody GetFrameOfReference(Vector3d pos) {
        // TODO: fill out this code :)
        
        return testMainBody;
    }
}
