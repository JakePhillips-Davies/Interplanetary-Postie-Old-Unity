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
[ExecuteInEditMode]
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
    double orbitStartTime = 0;

    // Integrals.
    double specific_angular_momentum = 0.0;
    double specific_mechanical_energy = 0.0;
    double mean_motion = 0.0;

    Vector3d localPos = new();
    Vector3d worldPos = new();
    Vector3d localVel = new();

    Vector3d linear_velocity = new(0.0, 0.0);

    bool isEnabled = true;

    float scalar = 1 / 1000000;
    //




    /*
            Unity
    */

    private void Start() {
        update_children_mu();
        if (transform.parent.TryGetComponent<Orbit>(out var parent)) mu = parent.get_children_mu();
        orbitStartTime = CelestialPhysics.get_singleton().time;
        _physics_process(CelestialPhysics.get_singleton().time);
    }

    public void EditorUpdate() {
        if (transform.parent != null)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                mu = parent.get_children_mu();

        if (!Application.isPlaying) {
            true_anomaly = 0.0;
            mean_anomaly = 0.0;
            distance = 0.0;
        }

        scalar = CelestialPhysics.get_singleton().get_spaceScale();
        _physics_process(CelestialPhysics.get_singleton().time);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        scalar =  CelestialPhysics.get_singleton().get_spaceScale();

        if (CelestialPhysics.get_singleton().SOIGizmo) Gizmos.DrawWireSphere(transform.position, (float)get_influence_radius() * scalar);

        if (CelestialPhysics.get_singleton().VelGizmo) Gizmos.DrawLine(transform.position, transform.position + (Vector3)new Vector3d(localVel.x, localVel.z, -localVel.y) * scalar * 100000);
    }

    private void OnValidate() {
        if(!Application.isPlaying) CelestialPhysics.get_singleton().Validate();    
    }




    /*
            Getters, setters, and some misc
    */
    public struct OrbitInfo {
        public double mass;
        public double mu;
        public double eccentricity;
        public double periapsis;
        public double longitude_of_ascending_node;
        public double longitude_of_perigee;
        public double inclination;
        public bool clockwise;
        public double true_anomaly;
        public double mean_anomaly;
        public double orbitStartTime;
        public Vector3d localPos;
        public Vector3d localVel;
        public Transform parent;
    } 
    public void InitialiseFromOrbitInfo(OrbitInfo from) {
        this.mass=from.mass;
        this.mu=from.mu;
        this.eccentricity=from.eccentricity;
        this.periapsis=from.periapsis;
        this.longitude_of_ascending_node=from.longitude_of_ascending_node;
        this.longitude_of_perigee=from.longitude_of_perigee;
        this.inclination=from.inclination;
        this.clockwise=from.clockwise;
        this.true_anomaly=from.true_anomaly;
        this.mean_anomaly=from.mean_anomaly;
        this.orbitStartTime=from.orbitStartTime;
        this.localPos = from.localPos;
        this.localVel = from.localVel;
        this.transform.parent = from.parent;
        
        _physics_process(CelestialPhysics.get_singleton().time);
        local_cartesian_to_cartesian();
    }

    static Vector3d reference_direction = Vector3d.right;

    // Integrals calculation from keplerian parameters.
    double get_specific_angular_momentum_from_keplerian() { return (clockwise ? 1.0 : -1.0) * Mathd.Sqrt(mu * get_semi_latus_rectum()); }
    double get_specific_mechanical_energy_from_keplerian() { return -mu / (2.0 * get_semi_major_axis()); }
    public double get_mean_motion_from_keplerian()
    {
        //double abs_semi_major_axis = Mathd.abs(get_semi_major_axis());
        //return Mathd.Sqrt(mu / abs_semi_major_axis) / abs_semi_major_axis;

        double multiplier = (eccentricity == 1.0) ? 1.0 : Mathd.Abs(1.0 - eccentricity * eccentricity);
        multiplier = Mathd.Sqrt(multiplier * multiplier * multiplier);
        return multiplier * mu * mu / (specific_angular_momentum * specific_angular_momentum * specific_angular_momentum);
    }

    public Vector3d getLocalPos() {
        return localPos;
    }
    public Vector3d getWorldPos() {
        return worldPos;
    }

    public Vector3d getLocalVel() {
        return localVel;
    }
    
    void on_keplerian_parameters_changed() {
        return;
    }

    public double GetOrbitStartTime() { return orbitStartTime; }
    public void SetOrbitStartTime(double time) { orbitStartTime = time; }

    public bool get_enabled() { return isEnabled; }


    public double get_mass() { return mass; }

    public double get_distance() { return distance; }


    // Keplerian parameters.
    public double get_mu() { return mu; }

    public double get_periapsis() { return periapsis; }
    
    public double get_inclination() { return inclination; }

    public double get_eccentricity() { return eccentricity; }

    public double get_longitude_of_perigee() { return longitude_of_perigee; }
    
    public double get_longitude_of_ascending_node() { return longitude_of_ascending_node; }

    public bool get_clockwise() { return clockwise; }


    // Supplementary orbital parameters.
    public double get_semi_latus_rectum() { return periapsis * (1.0 + eccentricity); }
    // public void set_semi_latus_rectum(double new_semi_latus_rectum) { set_periapsis(new_semi_latus_rectum / (1.0 + eccentricity)); }

    public double get_semi_major_axis() { return periapsis / (1.0 - eccentricity); }

    public double get_apoapsis()
    { return eccentricity < 1.0 ? periapsis * (1.0 + eccentricity) / (1.0 - eccentricity) : double.PositiveInfinity; }


    // Anomalies.
    public double get_true_anomaly() { return true_anomaly; }

    public double get_mean_anomaly() { return mean_anomaly; }

    public double get_true_anomaly_at_distance(double _distance)
    {
        // TODO: optimize: get_apoapsis and get_semi_latus_rectum can be combined.
        return ((_distance >= periapsis) && (_distance <= get_apoapsis())) ?
            Mathd.Acos((get_semi_latus_rectum() / _distance - 1.0) / eccentricity) :
            double.NaN;
    }


    // Get gravitational parameter used by children.
    public double get_children_mu()
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


    // Spheres of influence for patched conics. Just making sure the sun has infinite influence
    public double get_influence_radius() {
        if (transform.parent)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                return mass < 100000 ? 0 : periapsis * Mathd.Pow(mass / parent.mass, 0.4);
        
        return double.PositiveInfinity;
    }
    public double get_influence_radius_squared() {
        if (transform.parent)
            if (transform.parent.TryGetComponent<Orbit>(out var parent))
                return mass < 100000 ? 0 :  periapsis * periapsis * Mathd.Pow(mass / parent.mass, 0.8);
        
        return double.PositiveInfinity;
    }



    void set_enabled(bool new_enabled)
    {
        isEnabled = new_enabled;
    }


    void set_mass(double new_mass)
    {
        mass = Mathd.Max(new_mass, 0.0);
        update_children_mu();
    }


    void set_mu(double new_mu)
    {
        mu = Mathd.Max(new_mu, 0.0);
        _physics_process(CelestialPhysics.get_singleton().time);
        on_keplerian_parameters_changed();  
    }

    // void set_periapsis(double new_periapsis)
    // {
    //     periapsis = Mathd.Max(new_periapsis, 0.0);
    //     keplerian_to_cartesian();
    //     on_keplerian_parameters_changed();
    // }

    // void set_eccentricity(double new_eccentricity)
    // {
    //     eccentricity = Mathd.Max(new_eccentricity, 0.0);

    //     // Reset anomalies, as they are invalidated when eccentricity changes.
    //     true_anomaly = 0.0;
    //     mean_anomaly = 0.0;

    //     keplerian_to_cartesian();
    //     on_keplerian_parameters_changed();
    // }

    // void set_longitude_of_perigee(double new_longitude_of_perigee)
    // {
    //     longitude_of_perigee = Mathd.IEEERemainder(new_longitude_of_perigee, 2.0 * Mathd.PI);
    //     keplerian_to_cartesian(); // Do not update the integrals, they are the same.
    //     on_keplerian_parameters_changed();
    // }

    // void set_clockwise(bool new_clockwise)
    // {
    //     // Change revolvation direction by changing the sign of the specific angular momentum.
    //     if (clockwise != new_clockwise)
    //     { specific_angular_momentum = -specific_angular_momentum; }

    //     clockwise = new_clockwise;
    //     keplerian_to_cartesian(); // Just need to update the sign of the specific angular momentum, no need for a full update.
    //     on_keplerian_parameters_changed();
    // }


    // public void set_true_anomaly(double new_true_anomaly)
    // {
    //     if (eccentricity < 1.0) { new_true_anomaly = Mathd.IEEERemainder(new_true_anomaly, 2.0 * Mathd.PI); }
    //     else
    //     {
    //         double max_true_anomaly_absolute_value = Mathd.Acos(-1.0 / eccentricity) -
    //                                                  (double)(Single.Epsilon);// `float` machine epsilon for stability.
            
    //         new_true_anomaly = Mathd.Clamp(new_true_anomaly, -max_true_anomaly_absolute_value, max_true_anomaly_absolute_value);
    //     }
    //     true_anomaly = new_true_anomaly;

    //     mean_anomaly = CelestialPhysics.true_anomaly_to_mean_anomaly(new_true_anomaly, eccentricity);
    //     keplerian_to_cartesian(); // Do not update the integrals, they are the same.
    // }


    // public void set_mean_anomaly(double new_mean_anomaly)
    // {
    //     if (eccentricity < 1.0) { new_mean_anomaly = Mathd.IEEERemainder(new_mean_anomaly, 2.0 * Mathd.PI); }
    //     mean_anomaly = new_mean_anomaly;

    //     true_anomaly = CelestialPhysics.mean_anomaly_to_true_anomaly(new_mean_anomaly, eccentricity, true_anomaly);

    //     keplerian_to_cartesian(); // Do not update the integrals, they are the same.
    // }

    Vector3d get_linear_velocity() { return linear_velocity; }
    public void set_linear_velocity(Vector3d new_linear_velocity)
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

    public (Vector3d localPos, Vector3d localVel) GetCartesianAtTime(double time, bool updating) {
        if(Mathd.Abs(inclination) <= 1E-06) inclination = 1E-06; // The orbital equations do NOT play nice with very low inclinations

        // Updating the integrals.
        specific_angular_momentum  = get_specific_angular_momentum_from_keplerian();
        specific_mechanical_energy = get_specific_mechanical_energy_from_keplerian();
        mean_motion = get_mean_motion_from_keplerian();

        double new_mean_anomaly = mean_anomaly + (time - orbitStartTime) * mean_motion;
        if (eccentricity < 1.0) { new_mean_anomaly = Mathd.IEEERemainder(new_mean_anomaly, 2.0 * Mathd.PI); }

        double true_anomaly = CelestialPhysics.mean_anomaly_to_true_anomaly(new_mean_anomaly, eccentricity, this.true_anomaly);

        var results = GetCartesianAtTrueAnomaly(true_anomaly, updating);

        return (results.localPos, results.localVel);
    }
    public (Vector3d localPos, Vector3d localVel) GetCartesianAtTrueAnomaly(double true_anomaly, bool updating) {
        if(Mathd.Abs(inclination) <= 1E-06) inclination = 1E-06; // The orbital equations do NOT play nice with very low inclinations

        // Updating the integrals.
        specific_angular_momentum  = get_specific_angular_momentum_from_keplerian();

        if (updating) this.true_anomaly = true_anomaly;

        // Updating the distance.
        double distance = get_semi_latus_rectum() / (1.0 + eccentricity * Mathd.Cos(true_anomaly));

        // Position
        double X = distance*(Mathd.Cos(longitude_of_ascending_node)*Mathd.Cos(longitude_of_perigee+true_anomaly) 
                  - Mathd.Sin(longitude_of_ascending_node)*Mathd.Sin(longitude_of_perigee+true_anomaly)*Mathd.Cos(inclination));

        double Y = distance*(Mathd.Sin(longitude_of_ascending_node)*Mathd.Cos(longitude_of_perigee+true_anomaly)
                  + Mathd.Cos(longitude_of_ascending_node)*Mathd.Sin(longitude_of_perigee+true_anomaly)*Mathd.Cos(inclination));

        double Z = distance*(Mathd.Sin(inclination)*Mathd.Sin(longitude_of_perigee+true_anomaly));

        Vector3d localPos = new(X, Y, Z);

        // Velocity
        double p = get_semi_major_axis()*(1-eccentricity*eccentricity);

        double V_X = (X*specific_angular_momentum*eccentricity/(distance*p))*Mathd.Sin(true_anomaly) 
                    - (specific_angular_momentum/distance)*(Mathd.Cos(longitude_of_ascending_node)*Mathd.Sin(longitude_of_perigee+true_anomaly)
                    + Mathd.Sin(longitude_of_ascending_node)*Mathd.Cos(longitude_of_perigee+true_anomaly)*Mathd.Cos(inclination));

        double V_Y = (Y*specific_angular_momentum*eccentricity/(distance*p))*Mathd.Sin(true_anomaly) 
                    - (specific_angular_momentum/distance)*(Mathd.Sin(longitude_of_ascending_node)*Mathd.Sin(longitude_of_perigee+true_anomaly) 
                    - Mathd.Cos(longitude_of_ascending_node)*Mathd.Cos(longitude_of_perigee+true_anomaly)*Mathd.Cos(inclination));

        double V_Z = (Z*specific_angular_momentum*eccentricity/(distance*p))*Mathd.Sin(true_anomaly) 
                    + (specific_angular_momentum/distance)*(Mathd.Cos(longitude_of_perigee+true_anomaly)*Mathd.Sin(inclination));

        Vector3d localVel = new(V_X, V_Y, V_Z);

        return (localPos, localVel);
    }

    public void cartesian_to_keplerian()
    {
        // Updating global cartesian coordinates.
        // (must be implemented in derived classes)
        cartesian_to_local_cartesian();

        if(Mathd.Abs(inclination) <= 1E-06) inclination = 1E-06; // The orbital equations do NOT play nice with very low inclinations

        distance = localPos.magnitude;
        double velocity_quad = localVel.sqrMagnitude;

        // Integrals.
        Vector3d angular_momentum = Vector3d.Cross(localPos, localVel);
        specific_angular_momentum = angular_momentum.magnitude;

        clockwise = true;

        // i
        inclination = Mathd.Acos(angular_momentum.z / specific_angular_momentum);

        // LAN
        longitude_of_ascending_node = Mathd.Atan2(angular_momentum[0],-angular_momentum[1]);

        // eccentricity
        double E = 0.5 * velocity_quad - mu / distance;
        double semi_major_axis = -mu / (2 * E);
        eccentricity = Mathd.Sqrt(1 - (specific_angular_momentum*specific_angular_momentum) / (semi_major_axis * mu));

        mean_motion = get_mean_motion_from_keplerian(); // TODO direct calculation from cartesian.

        // Periapsis height.
        periapsis = specific_angular_momentum * specific_angular_momentum / (mu * (1.0 + eccentricity));

        // True and mean anomaly.
        double p = semi_major_axis * (1 - eccentricity*eccentricity);
        true_anomaly = Mathd.Atan2(Mathd.Sqrt(p / mu) * Vector3d.Dot(localPos, localVel),
                                  p - distance);
                                  
        mean_anomaly = CelestialPhysics.true_anomaly_to_mean_anomaly(true_anomaly, eccentricity);
        orbitStartTime = CelestialPhysics.get_singleton().time;

        // LP
        longitude_of_perigee = Mathd.Atan2(localPos[2] / (Mathd.Sin(inclination) + double.Epsilon),
                                          localPos[0]*Mathd.Cos(longitude_of_ascending_node) + localPos[1]*Mathd.Sin(longitude_of_ascending_node));
        longitude_of_perigee -= true_anomaly;

        on_keplerian_parameters_changed();
    }

    // Conversion between local (on orbital plane) and global cartesian coordinates.
    void local_cartesian_to_cartesian(bool updateTransform = true) {
        
        worldPos = localPos;
        if (transform.parent.TryGetComponent<Orbit>(out var parent)) {
            worldPos += parent.worldPos;
        }

        if(updateTransform) transform.localPosition = (Vector3)(new Vector3d(localPos.x, localPos.z, -localPos.y) * scalar);

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

                transform.parent = new_parent.transform;
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

    public bool patch_conics()
    {
        if (transform.parent != null)
        if (transform.parent.TryGetComponent<Orbit>(out var parent))
        {
            // Check if in parent's SOI.
            if (parent.get_influence_radius() < distance){ 
                reparent_up();
                return true;
            }
            else { // Check siblings.

                Transform[] siblings = new Transform[transform.parent.childCount];
                for (int i = 0; i < siblings.Length; i++)
                    siblings[i] = transform.parent.GetChild(i);

                foreach (Transform sibling in siblings) {
                    if (sibling != this.transform)
                        if (sibling.TryGetComponent<Orbit>(out Orbit siblingBody))
                            if ((localPos - siblingBody.localPos).sqrMagnitude < siblingBody.get_influence_radius_squared()){
                                reparent_down(siblingBody);
                                return true;
                            }
                }
            }
        }

        return false;
    }



    /*
            Process
    */
    void _ready()
    {
        on_keplerian_parameters_changed();
        update_children_mu();
    }


    public void _physics_process(double time, bool updateTransform = true) {
        scalar = CelestialPhysics.get_singleton().get_spaceScale();
        if (isEnabled)
        {
            var results = GetCartesianAtTime(time, true);

            localPos = results.localPos;
            distance = localPos.magnitude;
            localVel = results.localVel;
            local_cartesian_to_cartesian();
        }
    }

}