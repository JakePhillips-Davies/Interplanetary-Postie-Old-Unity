using UnityEngine;
using UnityEngine.Rendering.Universal;

/*
    #==============================================================#
	
	
	
	
*/
public class ShipCam : MonoBehaviour
{    
//--#
    #region Variables


    [Header("refs")]
    [field: SerializeField] public Camera cam  {get; private set;}


    #endregion
//--#



//--#
    #region Unity events


    private void Awake() {
        Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(cam);
    }

    private void Start() {
        SpaceControllerSingleton.Get.SetCameraObj(this.gameObject);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
