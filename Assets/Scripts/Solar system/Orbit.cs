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
        e - eccentricity
        i - inclination
        AP - arguement of periapsis - ω
        LAN - longitude of the ascending node - Ω

        MA - mean anomaly
        EA - eccentric anomaly
        TA - true anomaly

        FoR - frame of reference

        h - specific angular momentum

    */
    [SerializeField] private double a;
    [SerializeField] private double e;
    [SerializeField] private double i;
    [SerializeField] private double AP;
    [SerializeField] private double LAN;

    
    [SerializeField] private AstralBody testMainBody;
    [SerializeField] private int testItt;


    //

    private void FixedUpdate() {
        //tester

        // for (int i = 0; i < testItt; i++)
        // {
        //     CartesianToKepler(new Vector3(686750, 0, 0), new Vector3(0, 0, 2267.7f), testMainBody);
        // }
        double X =-1.250467672661996E+08, Y = 7.927671170785488E+07, Z = 2.225500574263185E+04,
               VX=-1.660470820920962E+01, VY=-2.514752499084350E+01, VZ= 2.373935085678625E-03;

        //Debug.Log(CartesianToKepler(new Vector3d(675000, 66000, -800000), new Vector3d(2000, -6000, -1000), testMainBody));
        Debug.Log(CartesianToKepler(new Vector3d(X*1000, Y*1000, Z*1000), new Vector3d(VX*1000, VY*1000, VZ*1000), testMainBody));

    }

    (double a1, double e1, double i1, double AP1, double LAN1) CartesianToKepler( Vector3d worldPos, Vector3d vel, AstralBody reference ) {

        Vector3d refPos = reference.position;
        Vector3d pos = worldPos - refPos;

        double mu = reference.mu;

        // Compute the specific angular momentum
        Vector3d h = Vector3d.Cross( pos, vel );
        double hMag = h.magnitude;

        // Compute the radius, r, and velocity squared, v2
        double r = pos.magnitude;
        double v2 = vel.x*vel.x + vel.y*vel.y + vel.z*vel.z;

        // Compute the specific energy, E
        double E = v2/2 - mu/r;

        // Compute semi-major axis, a1
        double a1 = -mu/( 2*E );

        // Compute eccentricity, e1, and it's vector, eVec
        Vector3d eVec = (v2 / mu - 1.0 / r) * pos - Vector3d.Dot(pos, vel) * vel / mu;
        double e1 = eVec.magnitude;

        // Compute inclination, i1
        double i1 = Math.Acos( h.z/hMag )*Mathf.Rad2Deg;

        // Compute longitude of the ascending node, LAN1, and vector of ascending node, n
        Vector3d n = new(-h.y, h.x, 0);

        double LAN1 = Math.Acos( n.x/n.magnitude );
        if(n.y > 0) LAN1 = ( 2*Math.PI ) - LAN1;
        LAN1 *= Mathf.Rad2Deg;

        // Compute argument of periapse, AP1
        double AP1 = Math.Acos( Vector3d.Dot(n, eVec)/( n.magnitude*e1 ) );
        if(eVec.z > 0) AP1 = ( 2*Math.PI ) - AP1;
        AP1 *= Mathf.Rad2Deg;

        return (a1, e1, i1, AP1, LAN1);
    }


    (Vector3d pos, Vector3d vel) KeplerToCartesian(double a1, double e1, double i1, double AP1, double LAN1, double t, AstralBody FoR) {
        Vector3d pos=new(), vel=new();

        return (pos, vel);
    }


    private AstralBody GetFrameOfReference(Vector3d pos) {
        // TODO: fill out this code :)
        
        return testMainBody;
    }
}
