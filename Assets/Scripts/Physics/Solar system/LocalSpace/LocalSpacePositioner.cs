using System;
using UnityEngine;

public class LocalSpacePositioner : MonoBehaviour
{
    Orbit focusOrbit;
    Orbit orbit;
    Vector3d posFromFocus;

    public void UpdatePositions() {
        focusOrbit = SpaceControllerSingleton.Get.focus.refOrbit;
        foreach (CelestialObject _object in SpaceControllerSingleton.Get.objectList) {
            
            orbit = _object.refOrbit;

            posFromFocus = orbit.GetWorldPos() - focusOrbit.GetWorldPos();
            posFromFocus = new(posFromFocus.x, posFromFocus.z, -posFromFocus.y);

            PositionBody(_object.localSpaceObj.GetComponent<LocalSpaceBody>());

        }
    }

    private void PositionBody(LocalSpaceBody body) {
        
        // Exit early if it's out of range
        if (posFromFocus.magnitude > SpaceControllerSingleton.Get.localRange) {
            if (body.isLoaded) body.Unload();
            return; 
        }
        
        if (!body.isLoaded) body.Load();
        body.transform.position = (Vector3)posFromFocus;

    }
}
