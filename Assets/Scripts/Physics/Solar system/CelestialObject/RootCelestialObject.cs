using UnityEngine;

public class RootCelestialObject : MonoBehaviour {
    
    [SerializeField] private CelestialObject rootCelestialObj;

    private void Start() {
        rootCelestialObj.ChainStartup();
    }
    private void FixedUpdate() {
        rootCelestialObj.ChainUpdateNewtonians();
        rootCelestialObj.ChainPosition();
        rootCelestialObj.ChainSimulate();
    }

    public void Validate() {
        rootCelestialObj.ChainStartup();
        rootCelestialObj.ChainUpdateNewtonians();
        rootCelestialObj.ChainPosition();
        rootCelestialObj.ChainSimulate();
        rootCelestialObj.ChainUpdateNewtonians();
        rootCelestialObj.ChainPosition();
    }
}