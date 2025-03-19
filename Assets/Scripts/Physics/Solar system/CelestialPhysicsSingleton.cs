using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// 
/// 
/// Liberal help from this c++ Godot orbital mechanics addon: https://github.com/ivanoovii/GDCelestial/tree/master
/// Would have taken a loooooot longer without them.
///  
/// </summary>
public class CelestialPhysicsSingleton : MonoBehaviour
{
    //

    public static double gravitational_constant = 6.6743015e-11;
    
    [SerializeField] Material lineMat;

    [field: SerializeField] public int patchDepthLimit { get; private set; } = 5;
    
    [field: SerializeField] public bool SOIGizmo { get; private set; } = true;
    [field: SerializeField] public bool VelGizmo { get; private set; } = true;
    [field: SerializeField] public GameObject blankObject;
    
    /// <summary>
    /// This is the object everything orbits. This object will not itself do any orbiting
    /// </summary>
    [SerializeField] private OrbitManager rootCelestialBody;
    public CelestialObject rootCelestialObject { get; private set; }
    
    public static CelestialPhysicsSingleton Get { get; private set; } = null;

    public struct CelestialObject
    {
        public OrbitManager orbitManager;
        public List<CelestialObject> children;
    }
    //

    private void Awake() {
        if ( Get == null ) {
            Get = this;
        }
        else {
            Debug.Log("SINGLETON INSTANCE ALREADY SET!!!!   check for duplicates of: " + this); 
        }
    }

    private void Start() {
        UpdateCelestialTree();
    }

    private void FixedUpdate() {
        ProcessCelestialPhysics();
    }
    
    public void Validate() {
        UpdateCelestialTree();
        void CheckChildrenRecurs(CelestialObject obj){
            foreach (var child in obj.children)
            {
                CheckChildrenRecurs(child); 
                child.orbitManager.EditorUpdate();
            }
        }

        CheckChildrenRecurs(rootCelestialObject);
    }

    
    public static void SetSingleton(CelestialPhysicsSingleton input) { Get = input; }




    /*
        Running the simulation
    */
    
    public void UpdateCelestialTree() {
        rootCelestialObject = new() {
            orbitManager = rootCelestialBody,
            children = new()
        };
        
        UpdateCelestialChildren(rootCelestialObject);

        SetupCelestialChildren(rootCelestialObject);

    }
    private void UpdateCelestialChildren(CelestialObject parent) {
        Transform[] children = new Transform[parent.orbitManager.transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = parent.orbitManager.transform.GetChild(i);

        foreach (Transform child in children) {
            if (child.TryGetComponent<OrbitManager>(out var childOrbit)) {
                CelestialObject childCelestialObject = new()
                {
                    orbitManager = childOrbit,
                    children = new()
                };

                UpdateCelestialChildren(childCelestialObject);

                parent.children.Add(childCelestialObject);
            }
        }
    }
    private void LogChildrenRecurs(CelestialObject obj) {
        Debug.Log(obj.orbitManager);

        foreach (var child in obj.children)
        {
            LogChildrenRecurs(child);
        }
    }

    private void SetupCelestialChildren(CelestialObject obj) {
        foreach (var child in obj.children) {
            
            child.orbitManager.Setup();

            SetupCelestialChildren(child);

        }
    }
    public void ProcessCelestialPhysics() {
        ProcessCelestialChildren(rootCelestialObject, UniversalTimeSingleton.Get.time, true);
    }
    public void ProcessCelestialPhysics(double t) {
        ProcessCelestialChildren(rootCelestialObject, t, false);
    }
    private void ProcessCelestialChildren(CelestialObject obj, double t, bool drawing) {
        foreach (var child in obj.children) {
            
            if(drawing)child.orbitManager.ProcessOrbit(t);
            else child.orbitManager.ProcessOrbitGhost(t);

            ProcessCelestialChildren(child, t, drawing);
            
        }
    }
    public List<Orbit> GetBigCelestialBody() {
        List<Orbit> orbits = new();

        void CheckChildrenRecurs(CelestialObject obj){
            foreach (var child in obj.children)
            {
                CheckChildrenRecurs(child); 
                if (child.orbitManager.orbit.get_influence_radius() > 10) orbits.Add(child.orbitManager.orbit);
            }
        }

        CheckChildrenRecurs(rootCelestialObject);

        return orbits;
    }





    /*
            Getters n shit
    */

    public Material getLineMat() { return lineMat; }
    
    public static CelestialPhysicsSingleton get_singleton() { return Get; }

    
    
    
    
    /*
            Conics maths
    */

    public static double true_anomaly_to_eccentric_anomaly(double true_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Math.Sqrt(1.0 - eccentricity * eccentricity));
        return true_anomaly - 2.0 * Math.Atan(beta * Math.Sin(true_anomaly) / (1.0 + beta * Math.Cos(true_anomaly)));
    }

    public static double eccentric_anomaly_to_true_anomaly(double eccentric_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Math.Sqrt(1.0 - eccentricity * eccentricity));
        return eccentric_anomaly + 2.0 * Math.Atan(beta * Math.Sin(eccentric_anomaly) / (1.0 - beta * Math.Cos(eccentric_anomaly)));
    }


    public static double mean_anomaly_to_true_anomaly(double mean_anomaly, double eccentricity, double true_anomaly_hint)
    {
        if (double.IsNaN(true_anomaly_hint)) {Debug.Log("True anomaly hint is NaN"); true_anomaly_hint = 0;}
        if (double.IsNaN(mean_anomaly)) {Debug.Log("Mean anomaly is NaN"); return true_anomaly_hint;}

        double tolerance = 0.000000001;
        int max_iter = 200;

        // Using Newton method to convert mean anomaly to true anomaly.
        if (eccentricity < 1) {

            mean_anomaly = Math.IEEERemainder(mean_anomaly, 2.0 * Math.PI);
            double eccentric_anomaly = 2.0 * Math.Atan(Math.Tan(0.5 * true_anomaly_hint) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));

            double region_min = mean_anomaly - eccentricity;
            double region_max = mean_anomaly + eccentricity;

            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                eccentric_anomaly = Math.Clamp(eccentric_anomaly, region_min, region_max);

                double residual = eccentric_anomaly - eccentricity * Math.Sin(eccentric_anomaly) - mean_anomaly;
                double derivative = 1.0 - eccentricity * Math.Cos(eccentric_anomaly);

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge. " + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Math.Atan(Math.Tan(0.5 * eccentric_anomaly) * Math.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity)));

        } else if(eccentricity == 1) {

            double z = Math.Cbrt(3.0 * mean_anomaly + Math.Sqrt(1 + 9.0 * mean_anomaly * mean_anomaly));
            return 2.0 * Math.Atan(z - 1.0 / z);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * true_anomaly_hint) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                double residual = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly - mean_anomaly;
                double derivative = eccentricity * Math.Cosh(eccentric_anomaly) - 1.0;

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge." + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Math.Atan(Math.Tanh(0.5 * eccentric_anomaly) * Math.Sqrt((eccentricity + 1.0) / (eccentricity - 1.0)));

        }
    }

    public static double true_anomaly_to_mean_anomaly(double true_anomaly, double eccentricity)
    {
        //double eccentric_anomaly = true_anomaly_to_eccentric_anomaly(true_anomaly, eccentricity);
        //return eccentric_anomaly_to_mean_anomaly(eccentric_anomaly, eccentricity);
        
        if (eccentricity < 1) {

            double eccentric_anomaly = 2.0 * Math.Atan(Math.Tan(0.5 * true_anomaly) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));
            return eccentric_anomaly - eccentricity * Math.Sin(eccentric_anomaly);

        } else if(eccentricity == 1) {

            double hta_tan = Math.Tan(0.5 * true_anomaly);
            return 0.5 * hta_tan * (1.0 + hta_tan * hta_tan / 3.0);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * true_anomaly) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            double meanAnomaly = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly;
            
            return meanAnomaly;

        }
    }
}
