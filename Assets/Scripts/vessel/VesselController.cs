using UnityEngine;

/*

*/
[ExecuteInEditMode]
public class VesselController : MonoBehaviour
{
//--#
    #region Variables


    [field: Header("References")]
    [field: SerializeField] public VesselController parentVessel {get; private set;}
    [field: SerializeField] public VesselRbObj rigidbodyRef {get; private set;}
    [field: SerializeField] public VesselColliderSpace colliderSpaceRef {get; private set;}
    [field: SerializeField] public VesselVisualObj visualObjRef {get; private set;}
    [field: SerializeField] public CelestialObject celestialObject {get; private set;}


    #endregion
//--#



//--#
    #region Unity events

    
    private void Start() {
        if (gameObject.TryGetComponent<LocalSpaceBody>(out LocalSpaceBody localSpaceBody)) {
            celestialObject = localSpaceBody.refCO;
        }
    }


    #endregion
//--#



//--#
    #region Transform Methods


    public Vector3 GetVesselPos() {
        return (parentVessel == null) ? rigidbodyRef.transform.position
                                      : (parentVessel.GetVesselRot() * (rigidbodyRef.transform.position - parentVessel.colliderSpaceRef.transform.position)) + parentVessel.GetVesselPos();
    }

    public Quaternion GetVesselRot() {
        return (parentVessel == null) ? rigidbodyRef.transform.rotation
                                      : parentVessel.GetVesselRot() * rigidbodyRef.transform.rotation;
    }


    #endregion
//--#



//--#
    #region Misc functions


    public void SetParent(VesselController other) {parentVessel = other;}


    #endregion
//--#
}
