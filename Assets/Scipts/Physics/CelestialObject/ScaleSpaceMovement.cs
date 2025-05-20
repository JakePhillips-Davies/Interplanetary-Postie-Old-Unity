using EditorAttributes;
using UnityEngine;
using Orbits;

/*
    #==============================================================#
	
	
	
	
*/
public class ScaleSpaceMovement : MonoBehaviour
{    
//--#
    #region Variables


    [field: Title("Refs")]
    [field: SerializeField] public OrbitDriver orbitDriver {get; private set;}


    #endregion
//--#



//--#
    #region Unity events


#if UNITY_EDITOR
    public void OnValidate() {
        Start();
    }
#endif

    private void Start() {
        if (ScaleSpaceSingleton.Get?.localSpaceTransform != null)
            orbitDriver = ScaleSpaceSingleton.Get.localSpaceTransform.Find(gameObject.name).GetComponent<OrbitDriver>();
        
        if (orbitDriver != null)
            transform.position = (Vector3)(orbitDriver.transformD.position / ScaleSpaceSingleton.Get.scaleDownFactor);
    }
    private void FixedUpdate() {
        transform.position = (Vector3)(orbitDriver.transformD.position / ScaleSpaceSingleton.Get.scaleDownFactor);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
