using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode, RequireComponent(typeof(LineRenderer))]
public class PlanetBeam : MonoBehaviour
{
    LineRenderer lineRenderer;
    Vector3[] positions = {new(0, -50, 0), new(0, 50, 0)};

    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = SpaceControllerSingleton.Get.lineMat;
        lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        lineRenderer.enabled = true;
        lineRenderer.loop = false;

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    // Update is called once per frame
    void LateUpdate()
    {        
        Vector3 camPos;
        camPos = Camera.main.transform.position;

        float distance = camPos.magnitude;

        float width = distance / 300;
        
        lineRenderer.widthMultiplier = width;

        lineRenderer.positionCount = 2;
        positions[0] = transform.position + 25 * width * Vector3.down;
        positions[1] = transform.position + 25 * width * Vector3.up;
        lineRenderer.SetPositions(positions);
    }
}
