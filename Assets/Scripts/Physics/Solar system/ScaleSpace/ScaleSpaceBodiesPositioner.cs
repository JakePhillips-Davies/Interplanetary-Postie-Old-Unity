using UnityEngine;

public class ScaleSpaceBodiesPositioner : MonoBehaviour
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

            _object.scaleSpaceBody.transform.position = (Vector3)(posFromFocus * ScaleSpaceSingleton.Get.GetSpaceScale()); 

        }
    }
    
}
