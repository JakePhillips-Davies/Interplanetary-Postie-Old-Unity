using System;
using EditorAttributes;
using Orbits;
using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class CelestialObject : MonoBehaviour
{    
//--#
    #region Variables


    [field: Title("Parameters")]
    [field: SerializeField] public double mass {get; private set;}
    [field: SerializeField, ReadOnly] public double gravitationalParameter  {get; private set;}
    [field: SerializeField, ReadOnly] public double SOIdistance  {get; private set;}
    [field: SerializeField, ReadOnly] public Vector3d position  {get; private set;}

    public SpaceSimTransform simTransform {get; private set;}


    #endregion
//--#



//--#
    #region Constructor


    public CelestialObject(double mass) {
        this.mass = mass;
        gravitationalParameter = Mathd.G * mass;
    }


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    
    private void OnValidate() {
        Awake();
        Start();
    }

#endif


    private void Awake() {
        simTransform = GetComponent<SpaceSimTransform>();
        simTransform.AddSimComponent(this);

        gravitationalParameter = Mathd.G * mass;
    }
    private void Start() {
        ReCalcSOI();
    }

    private void OnDestroy() {
        simTransform.RemoveSimComponent(this);
    }


    #endregion
//--#



//--#
    #region Misc functions


    public void ReCalcSOI() {
        if (simTransform.TryGetSimComponent(out OrbitDriver orbitDriver))
            SOIdistance = orbitDriver.orbit.periapsis * Math.Pow(mass / orbitDriver.orbit.parent.mass, 0.4);
        else SOIdistance = double.PositiveInfinity;
    }


    #endregion
//--#
}
