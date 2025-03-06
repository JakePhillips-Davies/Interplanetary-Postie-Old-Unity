using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 
/// 
/// 
/// Liberal help from this c++ Godot orbital mechanics addon: https://github.com/ivanoovii/GDCelestial/tree/master
/// Would have taken a loooooot longer without them.
///  
/// </summary>
[ExecuteInEditMode]
public class CelestialPhysics : MonoBehaviour
{
    //
    [Header("UpdateEditor")]
    [SerializeField] private bool BIG_BUTTON_FOR_VALIDATING;

    [SerializeField] private UIDocument ui;

    public static double gravitational_constant = 6.6743015e-11;
    [SerializeField] double time_scale = 1.0;
    public double time {get; private set;} = 0;
    [SerializeField] float spaceScaleDownFactor = 1000;
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
    
    static CelestialPhysics singleton;

    public struct CelestialObject
    {
        public OrbitManager orbitManager;
        public List<CelestialObject> children;
    }
    //

    private void Awake() {
        singleton = this;

        UpdateCelestialTree();

        ui.rootVisualElement.Q<Slider>().dataSource = this;
    }

    private void FixedUpdate() {
        ProcessCelestialPhysics();
        time += Time.fixedDeltaTime * get_time_scale();
    }

    private void OnValidate() { // Editor shenanigans
        Validate();
    }
    public void Validate() {
        singleton = this;
        UpdateCelestialTree();
        void CheckChildrenRecurs(CelestialObject obj){
            foreach (var child in obj.children)
            {
                CheckChildrenRecurs(child); 
                child.orbitManager.EditorUpdate();
                child.orbitManager.EditorUpdate();
            }
        }

        BIG_BUTTON_FOR_VALIDATING = false;

        CheckChildrenRecurs(rootCelestialObject);
    }




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
        ProcessCelestialChildren(rootCelestialObject, time, true);
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

    public double get_time_scale() { return time_scale; }
    public Material getLineMat() { return lineMat; }
    public float get_spaceDownScale() { return spaceScaleDownFactor; }
    public float get_spaceScale() { return 1 / spaceScaleDownFactor; }
    void set_time_scale(double new_time_scale){
        time_scale = new_time_scale;
    }
    
    public static CelestialPhysics get_singleton() { return singleton; }

    
    
    
    
    /*
            Conics maths
    */

    public static double true_anomaly_to_eccentric_anomaly(double true_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Mathd.Sqrt(1.0 - eccentricity * eccentricity));
        return true_anomaly - 2.0 * Mathd.Atan(beta * Mathd.Sin(true_anomaly) / (1.0 + beta * Mathd.Cos(true_anomaly)));
    }

    public static double eccentric_anomaly_to_true_anomaly(double eccentric_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Mathd.Sqrt(1.0 - eccentricity * eccentricity));
        return eccentric_anomaly + 2.0 * Mathd.Atan(beta * Mathd.Sin(eccentric_anomaly) / (1.0 - beta * Mathd.Cos(eccentric_anomaly)));
    }


    public static double mean_anomaly_to_true_anomaly(double mean_anomaly, double eccentricity, double true_anomaly_hint)
    {
        if (double.IsNaN(true_anomaly_hint)) true_anomaly_hint = 0;
        if (double.IsNaN(mean_anomaly)) return true_anomaly_hint;

        double tolerance = 0.00000001;
        int max_iter = 100;

        // Using Newton method to convert mean anomaly to true anomaly.
        if (eccentricity < 1) {

            mean_anomaly = Mathd.IEEERemainder(mean_anomaly, 2.0 * Mathd.PI);
            double eccentric_anomaly = 2.0 * Mathd.Atan(Mathd.Tan(0.5 * true_anomaly_hint) * Mathd.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));

            double region_min = mean_anomaly - eccentricity;
            double region_max = mean_anomaly + eccentricity;

            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                eccentric_anomaly = Mathd.Clamp(eccentric_anomaly, region_min, region_max);

                double residual = eccentric_anomaly - eccentricity * Mathd.Sin(eccentric_anomaly) - mean_anomaly;
                double derivative = 1.0 - eccentricity * Mathd.Cos(eccentric_anomaly);

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Mathd.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge. " + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Mathd.Atan(Mathd.Tan(0.5 * eccentric_anomaly) * Mathd.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity)));

        } else if(eccentricity == 1) {

            double z = Math.Cbrt(3.0 * mean_anomaly + Mathd.Sqrt(1 + 9.0 * mean_anomaly * mean_anomaly));
            return 2.0 * Mathd.Atan(z - 1.0 / z);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Mathd.Tan(0.5 * true_anomaly_hint) * Mathd.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                double residual = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly - mean_anomaly;
                double derivative = eccentricity * Math.Cosh(eccentric_anomaly) - 1.0;

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Mathd.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge." + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Mathd.Atan(Math.Tanh(0.5 * eccentric_anomaly) * Mathd.Sqrt((eccentricity + 1.0) / (eccentricity - 1.0)));

        }
    }

    public static double true_anomaly_to_mean_anomaly(double true_anomaly, double eccentricity)
    {
        //double eccentric_anomaly = true_anomaly_to_eccentric_anomaly(true_anomaly, eccentricity);
        //return eccentric_anomaly_to_mean_anomaly(eccentric_anomaly, eccentricity);
        
        if (eccentricity < 1) {

            double eccentric_anomaly = 2.0 * Mathd.Atan(Mathd.Tan(0.5 * true_anomaly) * Mathd.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));
            return eccentric_anomaly - eccentricity * Mathd.Sin(eccentric_anomaly);

        } else if(eccentricity == 1) {

            double hta_tan = Mathd.Tan(0.5 * true_anomaly);
            return 0.5 * hta_tan * (1.0 + hta_tan * hta_tan / 3.0);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Mathd.Tan(0.5 * true_anomaly) * Mathd.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            double meanAnomaly = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly;
            
            return meanAnomaly;

        }
    }
}
