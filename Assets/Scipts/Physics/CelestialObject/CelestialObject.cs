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
    }

#endif


    private void Awake() {

        simTransform = GetComponent<SpaceSimTransform>();
        simTransform.AddSimComponent(this);

        gravitationalParameter = Mathd.G * mass;
    }

    private void OnDestroy() {
        simTransform.RemoveSimComponent(this);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
