using UnityEngine;

/*
    #==============================================================#

    Visual element of the object.

    This object's position and rotation will match it's
    VesselRbObj's position and rotation, plus the parent's position 
    and rotation.
    

*/
[ExecuteInEditMode]
public class VesselVisualObj : MonoBehaviour
{
//--#
    #region variables


    [Header("References")]
    [field: SerializeField] public VesselController vesselController {get; private set;}


    #endregion
//--#

//--#
    #region unity events


    private void LateUpdate() {
        transform.position = vesselController.GetVesselPos();
        transform.rotation = vesselController.GetVesselRot();
    }


    #endregion
//--#
}
