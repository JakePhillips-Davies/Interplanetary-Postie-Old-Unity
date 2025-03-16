using UnityEngine;

public class CelestialObject : MonoBehaviour
{
    [field: SerializeField] public Orbit refOrbit { get; private set; }
    [field: SerializeField] public OrbitManager refOrbitManager { get; private set; }
    [field: SerializeField] public GameObject scaleSpacePrefab { get; private set; }
    [field: SerializeField] public GameObject localSpacePrefab { get; private set; }

    public GameObject scaleSpaceObj { get; private set; }
    public GameObject localSpaceObj { get; private set; }

    public void SetScaleSpaceObj(GameObject _scaleSpaceObj) { 
        scaleSpaceObj = _scaleSpaceObj;
        scaleSpaceObj.GetComponent<ScaleSpaceBody>().SetOrbit(this.refOrbit);
    }
    public void SetLocalSpaceObj(GameObject _localSpaceObj) { 
        localSpaceObj = _localSpaceObj; 
        localSpaceObj.GetComponent<LocalSpaceBody>().SetOrbit(this.refOrbit);
    }
}
