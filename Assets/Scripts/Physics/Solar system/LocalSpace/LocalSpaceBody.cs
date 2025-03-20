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

        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < children.Length; i++)
            transform.GetChild(i).gameObject.SetActive(true);

        if (rb != null)
            rb.isKinematic = false;

        Debug.Log(refOrbit.gameObject.name + " has been loaded into local space");
    }
    public void Unload() {
        isLoaded = false;

        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < children.Length; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        if (rb != null)
            rb.isKinematic = true;

        Debug.Log(refOrbit.gameObject.name + " has been unloaded from local space");
    }



    /*
        Misc
    */
    
    public void SetOrbit(Orbit orbit) { refOrbit = orbit; }

}
