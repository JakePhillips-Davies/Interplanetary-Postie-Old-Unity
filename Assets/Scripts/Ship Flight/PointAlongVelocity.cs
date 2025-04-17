using UnityEngine;

/*
    #==============================================================#
	
	
	
	
*/
public class PointAlongVector : MonoBehaviour
{    
//--#
    #region Variables


    [Header("refs")]
    [field: SerializeField] public Rigidbody rb {get; private set;}


    #endregion
//--#



//--#
    #region Unity events


    private void LateUpdate() {
        transform.rotation = Quaternion.LookRotation(-rb.linearVelocity.normalized, Vector3.up);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
