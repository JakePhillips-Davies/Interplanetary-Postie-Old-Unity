using Unity.Mathematics;
using UnityEngine;

public class DotController : MonoBehaviour
{
    //---             ---//
    [SerializeField] private float deadZone;
    [SerializeField] private float maxZoneMagnitude;
    private Vector2 screenCenter, dotDistance;
    private float rotation;
    public int lookSpeed;
    public Rigidbody ship;
    //---             ---//
    void FixedUpdate()
    {
        MoveDot();
        RotateShip();
    }
    
    private void Awake() {
        Input.mousePosition.Set(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }
    private void OnEnable() {
        Input.mousePosition.Set(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    void MoveDot()
    {
        screenCenter.y = Screen.height * 0.5f;
        screenCenter.x = Screen.width * 0.5f;

        dotDistance.y = (Input.mousePosition.y - screenCenter.y) / screenCenter.y;
        dotDistance.x = -(Input.mousePosition.x - screenCenter.x) / screenCenter.y;

        if (dotDistance.magnitude < deadZone) dotDistance = Vector2.zero;

        dotDistance = Vector2.ClampMagnitude(dotDistance, 1f);

        transform.localPosition = new Vector3(dotDistance.x*maxZoneMagnitude, 0f, dotDistance.y*maxZoneMagnitude);
    }

    void RotateShip()
    {
        rotation = Mathf.Lerp(rotation, Input.GetAxisRaw("Rotational"), 0.1f);
        ship.AddRelativeTorque(dotDistance.y * ship.mass * lookSpeed, -dotDistance.x * ship.mass * lookSpeed, rotation * ship.mass * lookSpeed);
    }
}
