using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class VesselChildMatchLocalCartesians : MonoBehaviour
{    
//--#
    #region Variables


    [Header("Refs")]
    [field: SerializeField] public Transform match {get; private set;}


    #endregion
//--#



//--#
    #region Unity events


    private void LateUpdate() {
        
        transform.localPosition = match.localPosition;
        transform.localEulerAngles = match.localEulerAngles;

    }


    #endregion
//--#
}
