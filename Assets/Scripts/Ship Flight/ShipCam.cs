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

    private void OnEnable() {
        SpaceControllerSingleton.Get.SetCameraObj(cam.gameObject);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
