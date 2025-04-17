using UnityEngine;

[ExecuteInEditMode]
public class MatchRotation : MonoBehaviour {
    private GameObject target;

    private void LateUpdate() {
        target = SpaceControllerSingleton.Get.cameraObj;
        if (target != null)
            transform.rotation = target.transform.rotation;
    }
}