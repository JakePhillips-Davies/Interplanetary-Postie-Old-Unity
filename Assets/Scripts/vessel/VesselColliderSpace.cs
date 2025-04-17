using UnityEngine;

/*
    #==============================================================#

    Game-space designed to hold rigidbodies that are inside the 
    vessel. 

    This object should only hold the colliders, and should be on a 
    seperate interiorColliderSpace layer that does not interact with
    any other layer.
    

*/
public class VesselColliderSpace : MonoBehaviour
{
//--#
    #region Variables


    [Header("References")]
    [field: SerializeField] public VesselController vesselController {get; private set;}


    #endregion
//--#

//--#
    #region unity events





    #endregion
//--#
}
