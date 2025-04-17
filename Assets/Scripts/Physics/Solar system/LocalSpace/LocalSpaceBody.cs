using UnityEngine;

public class LocalSpaceBody : MonoBehaviour
{
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    [field: SerializeField] public CelestialObject refCO { get; private set; }
    [field: SerializeField] public Rigidbody rb { get; private set; }

    public bool isLoaded { get; private set; } = false;



    /*
        Load/Unload
    */

    private void Start() {
        if (rb != null) rb.mass = (float)refOrbit.get_mass();
    }

    public void Load() {
        isLoaded = true;

        if (rb != null)
            rb.isKinematic = false;

        Debug.Log(refOrbit.gameObject.name + " has been loaded into local space");
    }
    public void Unload() {
        isLoaded = false;

        if (rb != null)
            rb.isKinematic = true;

        Debug.Log(refOrbit.gameObject.name + " has been unloaded from local space");
    }



    /*
        Misc
    */
    
    public void SetOrbit(Orbit orbit) { refOrbit = orbit; }
    public void SetCelestialObject(CelestialObject co) { refCO = co; }

    public void SetPosition(Vector3 pos) {
        if (rb != null) rb.transform.position = pos;
        else transform.position = pos;
    }

    public Vector3 GetPosition() {
        if (rb != null) return rb.transform.position;
        else return transform.position;
    }

}
