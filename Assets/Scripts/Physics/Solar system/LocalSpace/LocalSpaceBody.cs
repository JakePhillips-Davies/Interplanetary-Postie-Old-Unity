using UnityEngine;

public class LocalSpaceBody : MonoBehaviour
{
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    [field: SerializeField] public Rigidbody rb { get; private set; }

    public bool isLoaded { get; private set; } = false;



    /*
        Load/Unload
    */

    public void Load() {
        isLoaded = true;
        Debug.Log(refOrbit.gameObject.name + " has been loaded into local space");
    }
    public void Unload() {
        isLoaded = false;
        Debug.Log(refOrbit.gameObject.name + " has been unloaded from local space");
    }



    /*
        Misc
    */
    
    public void SetOrbit(Orbit orbit) { refOrbit = orbit; }

}
