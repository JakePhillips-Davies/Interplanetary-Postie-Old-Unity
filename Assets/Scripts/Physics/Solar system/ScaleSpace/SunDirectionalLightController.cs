using UnityEngine;

[ExecuteInEditMode]
public class SunDirectionalLightController : MonoBehaviour
{
    [SerializeField] private Orbit theSun;
    [SerializeField] private new Light light;
    [SerializeField] private float lightIntensityMult;
    Vector3d sunToZero;
    Vector3d sunPos;
    Vector3d focusPos;
    Vector3 sunDirectionLocal;

    private void Update() {
        sunPos = theSun.GetWorldPos();
        focusPos = SpaceControllerSingleton.Get.GetFocusPosition();

        sunToZero = focusPos - sunPos;
        sunToZero /= 10000000000;

        sunDirectionLocal = new Vector3((float)sunToZero.x, (float)sunToZero.z, (float)-sunToZero.y);

        transform.LookAt(transform.position + sunDirectionLocal);

        light.intensity = 1/sunDirectionLocal.magnitude * lightIntensityMult;
    }
}
