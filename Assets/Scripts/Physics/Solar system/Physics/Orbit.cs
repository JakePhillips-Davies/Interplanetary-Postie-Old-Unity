using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// 
/// 
/// Liberal help from this c++ Godot orbital mechanics addon: https://github.com/ivanoovii/GDCelestial/tree/master
/// Wouldn't have happened without their incredible work.
/// 
/// Some help also from this thread: https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
/// And this resource: https://orbital-mechanics.space/classical-orbital-elements/orbital-elements-and-the-state-vector.html#orbital-elements-state-vector
/// 
/// </summary>
public class Orbit : MonoBehaviour
{
    //
    /*
            Variables
    */

    [SerializeField] double mass;

    // Common orbital parameters.
    double mu = 1;           // Standard gravitational parameter of the parent celestial body.
    [SerializeField] double periapsis = 0.0;    // Distance to the lowest point of the orbit.
    [SerializeField] double eccentricity = 0.0; // Eccentricity of the orbit;
                                                // 0 - circular orbit, (0 - 1) - elliptical, 1 - parabolic, (1 - +\infty) - hyperbolic.
    [SerializeField] double longitude_of_perigee = 0.0;
    [SerializeField] double longitude_of_ascending_node = 0.0;
    [SerializeField] double inclination = 0.0;
    [SerializeField] bool clockwise = true;

    double true_anomaly = 0.0;
    double mean_anomaly = 0.0;
    double distance = 0.0;

    // Integrals.
    double specific_angular_momentum = 0.0;
    double specific_mechanical_energy = 0.0;
    double mean_motion = 0.0;

    Vector3d localPos = new();
    Vector3d localVel = new();

    Vector3d linear_velocity = new(0.0, 0.0);

    bool isEnabled = true;

    float scalar = 1;
    //




    /*
            Unity
    */

    private void Start() {
        update_children_mu();
    }

    private void OnValidate() {
        if (transform.parent != null)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                mu = parent.get_children_mu();

        if (!Application.isPlaying) {
            true_anomaly = 0.0;
            mean_anomaly = 0.0;
            distance = 0.0;
        }

        scalar = CelestialPhysics.get_singleton().get_spaceScale();
        _physics_process(0);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        scalar =  CelestialPhysics.get_singleton().get_spaceScale();

        if (CelestialPhysics.get_singleton().SOIGizmo) Gizmos.DrawWireSphere(transform.position, (float)get_influence_radius() * scalar);

        if (CelestialPhysics.get_singleton().VelGizmo) Gizmos.DrawLine(transform.position, transform.position + (Vector3)new Vector3d(localVel.x, localVel.z, -localVel.y) * scalar * 100000);
    }




    /*
            Getters, setters, and some misc
    */

    static Vector3d reference_direction = Vector3d.right;

    // Integrals calculation from keplerian parameters.
    double get_specific_angular_momentum_from_keplerian() { return (clockwise ? 1.0 : -1.0) * Math.Sqrt(mu * get_semi_latus_rectum()); }
    double get_specific_mechanical_energy_from_keplerian() { return -mu / (2.0 * get_semi_major_axis()); }
    double get_mean_motion_from_keplerian()
    {
        //double abs_semi_major_axis = Math.abs(get_semi_major_axis());
        //return Math.Sqrt(mu / abs_semi_major_axis) / abs_semi_major_axis;

        double multiplier = (eccentricity == 1.0) ? 1.0 : Math.Abs(1.0 - eccentricity * eccentricity);
        multiplier = Math.Sqrt(multiplier * multiplier * multiplier);
        return multiplier * mu * mu / (specific_angular_momentum * specific_angular_momentum * specific_angular_momentum);
    }

    public Vector3d getLocalPos() {
        return localPos;
    }

    
    void on_keplerian_parameters_changed() {
        return;
    }

    bool get_enabled() { return isEnabled; }


    double get_mass() { return mass; }


    // Keplerian parameters.
    double get_mu() { return mu; }

    public double get_periapsis() { return periapsis; }

    double get_eccentricity() { return eccentricity; }

    double get_longitude_of_perigee() { return longitude_of_perigee; }

    bool get_clockwise() { return clockwise; }


    // Supplementary orbital parameters.
    double get_semi_latus_rectum() { return periapsis * (1.0 + eccentricity); }
    void set_semi_latus_rectum(double new_semi_latus_rectum) { set_periapsis(new_semi_latus_rectum / (1.0 + eccentricity)); }

    public double get_semi_major_axis() { return periapsis / (1.0 - eccentricity); }

    double get_apoapsis()
    { return eccentricity < 1.0 ? periapsis * (1.0 + eccentricity) / (1.0 - eccentricity) : double.PositiveInfinity; }


    // Anomalies.
    double get_true_anomaly() { return true_anomaly; }

    double get_mean_anomaly() { return mean_anomaly; }

    double get_true_anomaly_at_distance(double _distance)
    {
        // TODO: optimize: get_apoapsis and get_semi_latus_rectum can be combined.
        return ((_distance >= periapsis) && (_distance <= get_apoapsis())) ?
            Math.Acos((get_semi_latus_rectum() / _distance - 1.0) / eccentricity) :
            double.NaN;
    }


    // Get gravitational parameter used by children.
    double get_children_mu()
    { return CelestialPhysics.gravitational_constant * mass; }

    void update_children_mu() {
        double children_mu = get_children_mu();

        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < children.Length; i++)
            children[i] = transform.GetChild(i);

        foreach (Transform child in children) {
            if (child.TryGetComponent<Orbit>(out var childBody)) childBody.set_mu(children_mu);
        }
    }


    // Spheres of influence for pathced conics or system generation. Just making sure the sun has infinite influence
    public double get_influence_radius() { 
        if (transform.parent)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                return periapsis * Math.Pow(mass / parent.mass, 0.4);
        
        return double.PositiveInfinity;
    }
    public double get_influence_radius_squared() {
        if (transform.parent)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                return periapsis * periapsis * Math.Pow(mass / parent.mass, 0.8);
        
        return double.PositiveInfinity;
    }
    double get_Hill_radius()              { return periapsis * Math.Pow(get_children_mu() / (3.0 * mu), 1.0 / 3.0); }
    double get_Hill_radius_squared()      { return periapsis * periapsis * Math.Pow(get_children_mu() / (3.0 * mu), 2.0 / 3.0); }



    void set_enabled(bool new_enabled)
    {
        isEnabled = new_enabled;
    }


    void set_mass(double new_mass)
    {
        mass = Math.Max(new_mass, 0.0);
        update_children_mu();
    }


    void set_mu(double new_mu)
    {
        mu = Math.Max(new_mu, 0.0);
        keplerian_to_cartesian();
        on_keplerian_parameters_changed();
    }

    void set_periapsis(double new_periapsis)
    {
        periapsis = Math.Max(new_periapsis, 0.0);
        keplerian_to_cartesian();
        on_keplerian_parameters_changed();
    }

    void set_eccentricity(double new_eccentricity)
    {
        eccentricity = Math.Max(new_eccentricity, 0.0);

        // Reset anomalies, as they are invalidated when eccentricity changes.
        true_anomaly = 0.0;
        mean_anomaly = 0.0;

        keplerian_to_cartesian();
        on_keplerian_parameters_changed();
    }

    void set_longitude_of_perigee(double new_longitude_of_perigee)
    {
        longitude_of_perigee = Math.IEEERemainder(new_longitude_of_perigee, 2.0 * Math.PI);
        keplerian_to_cartesian(); // Do not update the integrals, they are the same.
        on_keplerian_parameters_changed();
    }

    void set_clockwise(bool new_clockwise)
    {
        // Change revolvation direction by changing the sign of the specific angular momentum.
        if (clockwise != new_clockwise)
        { specific_angular_momentum = -specific_angular_momentum; }

        clockwise = new_clockwise;
        keplerian_to_cartesian(); // Just need to update the sign of the specific angular momentum, no need for a full update.
        on_keplerian_parameters_changed();
    }

    Dictionary<String, double> get_keplerian_parameters() {
        Dictionary<String, double> result = new();

        result["mu"] = mu;
        result["periapsis"] = periapsis;
        result["eccentricity"] = eccentricity;
        result["longitude_of_perigee"] = longitude_of_perigee;

        return result;
    }


    void set_true_anomaly(double new_true_anomaly)
    {
        if (eccentricity < 1.0) { new_true_anomaly = Math.IEEERemainder(new_true_anomaly, 2.0 * Math.PI); }
        else
        {
            double max_true_anomaly_absolute_value = Math.Acos(-1.0 / eccentricity) -
                                                     (double)(Single.Epsilon);// `float` machine epsilon for stability.
            
            new_true_anomaly = Math.Clamp(new_true_anomaly, -max_true_anomaly_absolute_value, max_true_anomaly_absolute_value);
        }
        true_anomaly = new_true_anomaly;

        mean_anomaly = CelestialPhysics.true_anomaly_to_mean_anomaly(new_true_anomaly, eccentricity);
        keplerian_to_cartesian(); // Do not update the integrals, they are the same.
    }


    void set_mean_anomaly(double new_mean_anomaly)
    {
        if (eccentricity < 1.0) { new_mean_anomaly = Math.IEEERemainder(new_mean_anomaly, 2.0 * Math.PI); }
        mean_anomaly = new_mean_anomaly;

        true_anomaly = CelestialPhysics.mean_anomaly_to_true_anomaly(new_mean_anomaly, eccentricity, true_anomaly);

        keplerian_to_cartesian(); // Do not update the integrals, they are the same.
    }

    Vector3d get_linear_velocity() { return linear_velocity; }
    void set_linear_velocity(Vector3d new_linear_velocity)
    {
        linear_velocity = new_linear_velocity;
        
        cartesian_to_keplerian();
    }



    /*   
            The big bit!    
                 |
                 |
                 V
    */

    void keplerian_to_cartesian()
    {
        if(Math.Abs(inclination) <= 1E-06) inclination = 1E-06; // The orbital equations do NOT play nice with very low inclinations

        // Updating the integrals.
        specific_angular_momentum  = get_specific_angular_momentum_from_keplerian();
        specific_mechanical_energy = get_specific_mechanical_energy_from_keplerian();
        mean_motion = get_mean_motion_from_keplerian();

        // Updating the distance.
        distance = get_semi_latus_rectum() / (1.0 + eccentricity * Math.Cos(true_anomaly));

        // Position
        double X = distance*(Math.Cos(longitude_of_ascending_node)*Math.Cos(longitude_of_perigee+true_anomaly) 
                  - Math.Sin(longitude_of_ascending_node)*Math.Sin(longitude_of_perigee+true_anomaly)*Math.Cos(inclination));

        double Y = distance*(Math.Sin(longitude_of_ascending_node)*Math.Cos(longitude_of_perigee+true_anomaly)
                  + Math.Cos(longitude_of_ascending_node)*Math.Sin(longitude_of_perigee+true_anomaly)*Math.Cos(inclination));

        double Z = distance*(Math.Sin(inclination)*Math.Sin(longitude_of_perigee+true_anomaly));

        localPos = new(X, Y, Z);

        // Velocity
        double p = get_semi_major_axis()*(1-eccentricity*eccentricity);

        double V_X = (X*specific_angular_momentum*eccentricity/(distance*p))*Math.Sin(true_anomaly) 
                    - (specific_angular_momentum/distance)*(Math.Cos(longitude_of_ascending_node)*Math.Sin(longitude_of_perigee+true_anomaly)
                    + Math.Sin(longitude_of_ascending_node)*Math.Cos(longitude_of_perigee+true_anomaly)*Math.Cos(inclination));

        double V_Y = (Y*specific_angular_momentum*eccentricity/(distance*p))*Math.Sin(true_anomaly) 
                    - (specific_angular_momentum/distance)*(Math.Sin(longitude_of_ascending_node)*Math.Sin(longitude_of_perigee+true_anomaly) 
                    - Math.Cos(longitude_of_ascending_node)*Math.Cos(longitude_of_perigee+true_anomaly)*Math.Cos(inclination));

        double V_Z = (Z*specific_angular_momentum*eccentricity/(distance*p))*Math.Sin(true_anomaly) 
                    + (specific_angular_momentum/distance)*(Math.Cos(longitude_of_perigee+true_anomaly)*Math.Sin(inclination));

        localVel = new(V_X, V_Y, V_Z);

        // Updating global cartesian coordinates.
        local_cartesian_to_cartesian();

    }

    void cartesian_to_keplerian()
    {
        // Updating global cartesian coordinates.
        // (must be implemented in derived classes)
        cartesian_to_local_cartesian();

        if(Math.Abs(inclination) <= 1E-06) inclination = 1E-06; // The orbital equations do NOT play nice with very low inclinations

        distance = localPos.magnitude;
        double velocity_quad = localVel.sqrMagnitude;

        // Integrals.
        Vector3d angular_momentum = Vector3d.Cross(localPos, localVel);
        specific_angular_momentum = angular_momentum.magnitude;

        clockwise = true;

        // i
        inclination = Math.Acos(angular_momentum.z / specific_angular_momentum);

        // LAN
        longitude_of_ascending_node = Math.Atan2(angular_momentum[0],-angular_momentum[1]);

        // eccentricity
        double E = 0.5 * velocity_quad - mu / distance;
        double semi_major_axis = -mu / (2 * E);
        eccentricity = Math.Sqrt(1 - (specific_angular_momentum*specific_angular_momentum) / (semi_major_axis * mu));

        mean_motion = get_mean_motion_from_keplerian(); // TODO direct calculation from cartesian.

        // Periapsis height.
        periapsis = specific_angular_momentum * specific_angular_momentum / (mu * (1.0 + eccentricity));

        // True and mean anomaly.
        double p = semi_major_axis * (1 - eccentricity*eccentricity);
        true_anomaly = Math.Atan2(Math.Sqrt(p / mu) * Vector3d.Dot(localPos, localVel),
                                  p - distance);
                                  
        mean_anomaly = CelestialPhysics.true_anomaly_to_mean_anomaly(true_anomaly, eccentricity);

        // LP
        longitude_of_perigee = Math.Atan2(localPos[2] / (Math.Sin(inclination) + double.Epsilon),
                                          localPos[0]*Math.Cos(longitude_of_ascending_node) + localPos[1]*Math.Sin(longitude_of_ascending_node));
        longitude_of_perigee -= true_anomaly;

        on_keplerian_parameters_changed();
    }

    // Conversion between local (on orbital plane) and global cartesian coordinates.
    void local_cartesian_to_cartesian() {
        transform.localPosition = (Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * scalar);

        linear_velocity = localVel;
    }
    void cartesian_to_local_cartesian() {
        localVel = linear_velocity;
    }




    /*
                Patching
    */

    void reparent_up()
    {
        if (transform.parent.TryGetComponent<Orbit>(out var parent)) {
            if (parent.transform.parent.TryGetComponent<Orbit>(out var new_parent)) {

                transform.SetParent(new_parent.transform, true);
                localPos += parent.localPos;
                
                mu = new_parent.get_children_mu();
                
                set_linear_velocity(get_linear_velocity() + parent.get_linear_velocity());
            }
            else
            { Debug.Log("Can not reparent CelestialBody2D up: new parent is not a valid node."); }
        }
        else
        { Debug.Log("Can not reparent CelestialBody2D up: current parent is not a CelestialBody2D node."); }
    }

    void reparent_down(Orbit new_parent)
    {
        transform.SetParent(new_parent.transform, true);
        localPos -= new_parent.localPos;
        
        mu = new_parent.get_children_mu();
        
        set_linear_velocity(localVel - new_parent.localVel);

    }

    public void patch_conics()
    {
        if (transform.parent != null)
        if (transform.parent.TryGetComponent<Orbit>(out var parent))
        {
            // Check if in parent's SOI.
            if (parent.get_influence_radius() < distance){ 
                reparent_up(); 
            }
            else { // Check siblings.

                Transform[] siblings = new Transform[transform.parent.childCount];
                for (int i = 0; i < siblings.Length; i++)
                    siblings[i] = transform.parent.GetChild(i);

                foreach (Transform sibling in siblings) {
                    if (sibling != this.transform)
                        if (sibling.TryGetComponent<Orbit>(out Orbit siblingBody))
                            if ((localPos - siblingBody.localPos).sqrMagnitude < siblingBody.get_influence_radius_squared())
                                reparent_down(siblingBody);
                }
            }
        }
    }



    /*
            Process
    */
    void _ready()
    {
        on_keplerian_parameters_changed();
        update_children_mu();
    }

    /// <summary>
    /// Adds delta to mean anomaly taking into account mean motion
    /// </summary>
    /// <param name="delta"></param>
    public void _physics_process(double delta)
    {
        if (isEnabled)
        {
            double physics_dt = delta;
            set_mean_anomaly(mean_anomaly + physics_dt * mean_motion);
        }
    }

    /// <summary>
    /// Straight adds delta to the mean anomaly
    /// </summary>
    /// <param name="delta"></param>
    public void _orbit_process(double delta)
    {
        double physics_dt = delta;
        set_mean_anomaly(mean_anomaly + physics_dt);
    }

}