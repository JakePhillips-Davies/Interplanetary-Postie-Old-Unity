using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class VesselLeaveTrigger : MonoBehaviour
{    
//--#
    #region Variables





    #endregion
//--#



//--#
    #region Unity events


    void OnTriggerStay(Collider other) {

        if (other.isTrigger || (other.attachedRigidbody == null)) return;
        
        if (other.attachedRigidbody.TryGetComponent<VesselRbObj>(out VesselRbObj vesselRbObj)) {
            
            vesselRbObj.VesselLeaveTriggerHandle();

        }

    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
