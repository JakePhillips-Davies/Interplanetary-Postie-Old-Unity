using EditorAttributes;
using UnityEngine;
using Orbits;
using UnityEditor.ShaderGraph;

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
    private void OnDrawGizmos() {
        if (orbitDriver.simTransform.TryGetSimComponent(out CelestialObject celestialObject)) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, (float)(celestialObject.SOIdistance / ScaleSpaceSingleton.Get.scaleDownFactor));
        }
    }
#endif

    private void Start() {
        if (ScaleSpaceSingleton.Get?.localSpaceTransform != null)
            orbitDriver = ScaleSpaceSingleton.Get.localSpaceTransform.Find(gameObject.name).GetComponent<OrbitDriver>();
        
        if (orbitDriver != null)
            transform.position = (Vector3)(orbitDriver.simTransform.position / ScaleSpaceSingleton.Get.scaleDownFactor);
    }
    private void FixedUpdate() {
        transform.position = (Vector3)(orbitDriver.simTransform.position / ScaleSpaceSingleton.Get.scaleDownFactor);
    }


    #endregion
//--#



//--#
    #region Misc functions


    


    #endregion
//--#
}
