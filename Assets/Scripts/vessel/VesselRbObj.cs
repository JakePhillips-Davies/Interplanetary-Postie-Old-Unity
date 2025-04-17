using UnityEngine;

/*
    #==============================================================#
    
    Rigidbody element of the object. This object should exist on one
    of two collision layers:
    -Default, whatever your default layer is
    -interiorColliderSpace, some layer meant for the interiors and
    doesn't interact with anything else

    Parenting:
    This will live under the VesselSpaceController and, by default, 
    be on the default layer.

    On it's way INTO an interior space:
    -Re-parent to that vessel's collider space
    -Remove new parent vessel's rigidbody's velocity and angular 
    momentum from this
    -Change layer new parent's layer

    On it's way OUT:
    -Re-parent to that vessel's rigidbody's parent
    -Add old parent vessel's rigidbody's velocity and angular 
    momentum to this
    -Change layer new parent's layer

    On re-parenting it will also pass the new parent's visual
    object to it's own VesselVisualObj for re-parenting
    (if it's new parent has one)


    !! Known issues
    If it's parent's exit is backed up against a wall this object will 
    phase through that wall until it hits the leave trigger and re=
    parents up.
    It's kinda just one of those annoying limitations of using this
    method.


*/
[RequireComponent(typeof(Rigidbody))]
public class VesselRbObj : MonoBehaviour
{
//--#
    #region variables


    [Header("References")]
    [field: SerializeField] public VesselController vesselController {get; private set;}
    private Rigidbody rb;

    private bool isMovingThisFrame = false;

    private Vector3 relativePos;
    private Vector3 relativeLinearVel;
    private Vector3 relativeAngularVel;


    #endregion
//--#



//--#
    #region unity events


    private void Awake() {
        rb = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {

        if (vesselController.parentVessel != null)
            rb.isKinematic = vesselController.parentVessel.rigidbodyRef.rb.isKinematic;

        isMovingThisFrame = false;    
    }


    #endregion
//--#



//--#
    #region Trigger handlers


    public void VesselEnterTriggerHandle(VesselController otherVessel) {

        if (isMovingThisFrame) return;
        isMovingThisFrame = true;

        relativePos = this.transform.position - otherVessel.rigidbodyRef.transform.position;
        relativeLinearVel = this.GetLinearVel() - otherVessel.rigidbodyRef.GetLinearVel();
        relativeAngularVel = this.GetAngularVel() - otherVessel.rigidbodyRef.GetAngularVel();
        
        Debug.Log(vesselController.name + " is entering: " + otherVessel.name);

        MoveToVessel(otherVessel);

        if (vesselController.celestialObject != null) {
            vesselController.celestialObject.SetState(CelestialObject.CelestialObjectState.NON_ORBITTING);
        }

    }
    
    public void VesselLeaveTriggerHandle() {

        if (isMovingThisFrame) return;
        isMovingThisFrame = true;

        relativePos = this.transform.position - vesselController.parentVessel.colliderSpaceRef.transform.position;
        relativeLinearVel = this.GetLinearVel();
        relativeAngularVel = this.GetAngularVel();
        
        Debug.Log(vesselController.name + " is leaving: " + vesselController.parentVessel.name);

        VesselController _parentVessel = vesselController.parentVessel;

        LeaveParent();

        if ((vesselController.celestialObject != null) && (gameObject.layer == LayerMask.NameToLayer("Default"))) {
            // A lot more needs to be done here!!
            if (_parentVessel.celestialObject != null) {
                vesselController.celestialObject.SetState(CelestialObject.CelestialObjectState.TRANSITIONING);

                Vector3d orbitPos = _parentVessel.celestialObject.refOrbit.GetLocalPos();
                orbitPos = new(orbitPos.x, orbitPos.z, -orbitPos.y);

                orbitPos = orbitPos + new Vector3d(transform.position - _parentVessel.rigidbodyRef.transform.position);
                
                orbitPos = new(orbitPos.x, -orbitPos.z, orbitPos.y);

                Vector3 vel = rb.linearVelocity;
                vel = new(vel.x, -vel.z, vel.y);

                vesselController.celestialObject.refOrbit.SetParentOrbit(_parentVessel.celestialObject.refOrbit.parentOrbit);
                vesselController.celestialObject.refOrbit.SetCartesianElements(new Vector3d(vel), orbitPos);
                vesselController.celestialObject.refOrbit.SetOrbitStartTime(_parentVessel.celestialObject.lastTime);
                vesselController.celestialObject.RecalculateFamily();

                vesselController.celestialObject.refOrbit._physics_process(UniversalTimeSingleton.Get.time);
            }
            else {
                Debug.Log(vesselController.name + " | Parent does does not have an attached celestial object and this is attempting to leave!");
            }
        }

    }


    #endregion
//--#



//--#
    #region Re-parent


    private void MoveToVessel(VesselController otherVessel) {

        transform.position = otherVessel.colliderSpaceRef.transform.position + otherVessel.rigidbodyRef.transform.InverseTransformPoint(transform.position);
        transform.rotation = Quaternion.Inverse(otherVessel.rigidbodyRef.transform.rotation ) * transform.rotation;

        rb.linearVelocity = Quaternion.Inverse(otherVessel.rigidbodyRef.transform.rotation ) * relativeLinearVel;
        rb.angularVelocity = Quaternion.Inverse(otherVessel.rigidbodyRef.transform.rotation ) * relativeAngularVel;
        
        vesselController.SetParent(otherVessel);
    
        gameObject.layer = LayerMask.NameToLayer("InteriorColliderSpace");
        foreach (Transform rbElement in GetComponentInChildren<Transform>()) {
            rbElement.gameObject.layer = LayerMask.NameToLayer("InteriorColliderSpace");
        }

    }

    private void LeaveParent() {

        transform.position = vesselController.parentVessel.rigidbodyRef.transform.TransformPoint(relativePos);
        transform.rotation = vesselController.parentVessel.rigidbodyRef.transform.rotation * transform.rotation;

        rb.linearVelocity = (vesselController.parentVessel.rigidbodyRef.transform.rotation * relativeLinearVel) + vesselController.parentVessel.rigidbodyRef.GetLinearVel();
        rb.angularVelocity = (vesselController.parentVessel.rigidbodyRef.transform.rotation * relativeAngularVel) + vesselController.parentVessel.rigidbodyRef.GetAngularVel();

        if (vesselController.parentVessel.parentVessel != null) {
            vesselController.SetParent(vesselController.parentVessel.parentVessel);

            gameObject.layer = LayerMask.NameToLayer("InteriorColliderSpace");
            foreach (Transform rbElement in GetComponentInChildren<Transform>()) {
                rbElement.gameObject.layer = LayerMask.NameToLayer("InteriorColliderSpace");
            }
        }
        else {
            vesselController.SetParent(null);

            gameObject.layer = LayerMask.NameToLayer("Default");
            foreach (Transform rbElement in GetComponentInChildren<Transform>()) {
                rbElement.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        
    }


    #endregion
//--#



//--#
    #region Misc functions


    public Vector3 GetLinearVel() { return rb.linearVelocity; }
    public Vector3 GetAngularVel() { return rb.angularVelocity; }


    #endregion
//--#
}
