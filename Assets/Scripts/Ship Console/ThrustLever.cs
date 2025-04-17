using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class ThrustLever : MonoBehaviour
{    
//--#
    #region Variables


    [Header("refs")]
    [field: SerializeField] public ShipMovement shipMover  {get; private set;}
    
    [Header("angles")]
    [field: SerializeField] public float zeroAngle  {get; private set;}
    [field: SerializeField] public float fullAngle  {get; private set;}
    private float maxAngleChange;
    private float currAngle;


    #endregion
//--#



//--#
    #region Unity events


    private void Awake() {
        maxAngleChange = fullAngle - zeroAngle;
    }

    private void Update() {
        currAngle = shipMover.thrustLevel * maxAngleChange;
        
        transform.localEulerAngles = new(zeroAngle + currAngle, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
