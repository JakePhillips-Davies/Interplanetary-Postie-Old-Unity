using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class VesselEnterTrigger : MonoBehaviour
{    
//--#
    #region Variables


    [Header("references")]
    [field: SerializeField] public VesselController vesselController  {get; private set;}


    #endregion
//--#



//--#
    #region Unity events


    void OnTriggerStay(Collider other) {

        if (other.isTrigger || (other.attachedRigidbody == null)) return;
        
        if (other.attachedRigidbody.TryGetComponent<VesselRbObj>(out VesselRbObj vesselRbObj)) {
            
            vesselRbObj.VesselEnterTriggerHandle(vesselController);

        }

    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
    }


    #endregion
//--#



//--#
    #region Misc functions





    #endregion
//--#
}
