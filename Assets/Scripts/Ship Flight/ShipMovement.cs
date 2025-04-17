using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShipMovement : MonoBehaviour
{
    //---                      ---//
    [SerializeField] private new GameObject camera;
    [SerializeField] private GameObject camContainer;
    public Rigidbody shipRigidbody;

    //||-- movement --||//

    float forwardInput;
    float rightInput;
    float upInput;
    Vector3 shipForce;
    [field: SerializeField, Tooltip("Thrust in newtons")] public float shipManoeuvringThrust {get; private set;} = 0;

    public float thrustLevel {get; private set;} = 0; 
    [field: SerializeField, Tooltip("Thrust in newtons")] public float maxThrust {get; private set;} = 0;

    //||-- movement --||//

    public KeyCode exit;
    
    public KeyCode freeCam;

    [SerializeField] private KeyCode thrustUp;
    [SerializeField] private KeyCode thrustDown;

    //---                      ---//
    
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Input.mousePosition.Set(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
    }

    void Update()
    {
        if(Input.GetKeyDown(exit)) {
            ControlSwapper cs = GetComponent<ControlSwapper>();
            cs.Activate();
        }        

        if(Input.GetKeyDown(freeCam)) {
            ShipFreeCam cam = camera.GetComponent<ShipFreeCam>();
            cam.enabled = !cam.enabled;
        }
        
        if (Input.GetKeyDown(KeyCode.M)) {
            SwitchMapView();
        }
    }

    void FixedUpdate()
    {
        if(Input.GetKey(thrustDown)) {
            thrustLevel -= 0.02f;
            if (thrustLevel < 0) thrustLevel = 0;
            if (thrustLevel > 1) thrustLevel = 1;
        }
        if(Input.GetKey(thrustUp)) {
            thrustLevel += 0.02f;
            if (thrustLevel < 0) thrustLevel = 0;
            if (thrustLevel > 1) thrustLevel = 1;
        }

        // Yea some of these need to be negative apparently 
        forwardInput = -Input.GetAxisRaw("Vertical");
        rightInput = -Input.GetAxisRaw("Horizontal");
        upInput = Input.GetAxisRaw("UppyDowny");

        shipForce = (transform.forward * forwardInput + transform.right * rightInput + transform.up * upInput) * shipManoeuvringThrust;
        shipForce -= transform.forward * thrustLevel * maxThrust;

        shipRigidbody.AddForce(shipForce);
    }

    private void SwitchMapView() {
        ScaleSpaceSingleton.Get.cam.GetComponent<MatchRotation>().enabled = !ScaleSpaceSingleton.Get.cam.GetComponent<MatchRotation>().enabled;
        ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled = !ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled;

        ScaleSpaceSingleton.Get.cam.transform.position = Vector3.zero;

        if (ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled) {
            camContainer.SetActive(false);
            ScaleSpaceSingleton.Get.cam.GetComponent<Camera>().fieldOfView = 75;
        }
        else {
            camContainer.SetActive(true);
        }
    }
}
