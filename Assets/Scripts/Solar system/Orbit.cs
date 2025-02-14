using UnityEngine;

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

    */
    [SerializeField] double a;
    [SerializeField] double e;
    [SerializeField] double i;
    [SerializeField] double AP;
    [SerializeField] double LAN;
    //

    (double a1, double e1, double i1, double AP1, double LAN1) CartesianToKepler(Vector3 pos, Vector3 vel) {
        double a1=0, e1=0, i1=0, AP1=0, LAN1=0;

        // get nearest frame of reference
        //...

        return (a1, e1, i1, AP1, LAN1);
    }

    (Vector3 pos, Vector3 vel) KeplerToCartesian(double a1, double e1, double i1, double AP1, double LAN1, double t, AstralBody FoR) {
        Vector3 pos=new(), vel=new();

        return (pos, vel);
    }
}
