using System;
using System.Collections;
using UnityEngine;

public class LocalSpacePositioner : MonoBehaviour
{
    Orbit focusOrbit;
    Orbit orbit;
    Vector3d intendedPosFromFocus;

    public void UpdatePositions() {
        focusOrbit = SpaceControllerSingleton.Get.focus.refOrbit;
        foreach (CelestialObject _object in SpaceControllerSingleton.Get.objectList) {
            
            orbit = _object.refOrbit;

            intendedPosFromFocus = orbit.GetWorldPos() - focusOrbit.GetWorldPos();
            intendedPosFromFocus = new(intendedPosFromFocus.x, intendedPosFromFocus.z, -intendedPosFromFocus.y);

            PositionBody(_object.localSpaceObj.GetComponent<LocalSpaceBody>());

        }
    }

    private void PositionBody(LocalSpaceBody body) {
        
        // Exit early if it's out of range
        if (intendedPosFromFocus.magnitude > SpaceControllerSingleton.Get.localRange) {
            if (body.isLoaded) body.Unload();
            return; 
        }
        
        if (!body.isLoaded) body.Load();
        
        if (body.rb != null) {
            Vector3d prevPos = orbit.GetCartesianAtTime(UniversalTimeSingleton.Get.time - Time.fixedDeltaTime, false).localPos;
            Vector3d prevPosFromPos = prevPos - orbit.GetLocalPos();
            prevPosFromPos = new(prevPosFromPos.x, prevPosFromPos.z, -prevPosFromPos.y);
            Vector3 vel = (Vector3)(-prevPosFromPos/Time.fixedDeltaTime);

            body.transform.position = (Vector3)(intendedPosFromFocus + prevPosFromPos);
            body.rb.linearVelocity = vel;
            
            StartCoroutine(PostPhysics(body, orbit, vel));
        }
        else {
            body.transform.position = (Vector3)intendedPosFromFocus;
        }

    }
    
    IEnumerator PostPhysics(LocalSpaceBody body, Orbit orbit, Vector3 prePhysVel) {
        yield return new WaitForFixedUpdate();

        Vector3 vel = body.rb.linearVelocity - prePhysVel;
        Vector3d velD = new(vel.x, -vel.z, vel.y);
        orbit.SetCartesianElements(orbit.GetLocalVel() + velD, orbit.GetLocalPos());

        body.rb.linearVelocity = Vector3.zero;
    }
}
