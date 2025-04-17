using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerCam : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float sensitivity;
    private float yRot;
    private float xRot;
    [SerializeField] private KeyCode freeMouseKey = KeyCode.R;
    public bool isFreeLooking { get; set; } = false;

    [Header("")]
    [Header("Refs")]
    [SerializeField] private GameObject body;
    [SerializeField] private Camera cam;

    [Header("")]
    [Header("Interaction")]
    [SerializeField] private int reach;        public int getReach() { return reach; }
    [SerializeField] private int interactionReach;
    [SerializeField] private KeyCode interactkey;
    private bool interactDelay = false;
    private RaycastHit hit;
    private Ray interactRay;        public Ray GetInteractRay() { return interactRay; }

    private void Awake() {
        Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(cam);
    }

    private void OnEnable() {
        interactDelay = true;
        SpaceControllerSingleton.Get.SetCameraObj(this.gameObject);
    }

    void LateUpdate()
    {
        if(Input.GetKeyDown(freeMouseKey)){
            isFreeLooking = !isFreeLooking;
        }
        
        if(!isFreeLooking){
            Cursor.lockState = CursorLockMode.Locked;

            yRot = Input.GetAxisRaw("Mouse X") * sensitivity;
            xRot = -Input.GetAxisRaw("Mouse Y") * sensitivity;

            transform.Rotate(xRot, 0, 0, Space.Self);
            Vector3 eulerAngles = transform.localEulerAngles;
            if((eulerAngles.x > 180) && (eulerAngles.y >= 180)) eulerAngles.x = 270;
            if((eulerAngles.x < 180) && (eulerAngles.y >= 180)) eulerAngles.x = 90;
            transform.localRotation = Quaternion.Euler(eulerAngles.x, 0, 0);

            body.transform.Rotate(0, yRot, 0, Space.Self);
        }
        else{
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        interactRay = cam.ScreenPointToRay (Input.mousePosition);

        activatableHandler();
    }

    private void OnDrawGizmos() {
        
        Gizmos.DrawRay(interactRay.origin, interactRay.direction * reach);

    }

    void activatableHandler() {
        if(interactDelay) {
            interactDelay = false;
            return;
        }

        if(Physics.Raycast(transform.position, transform.forward, out hit)){
            if(Input.GetKeyDown(interactkey) && (hit.transform != null) && (hit.collider.GetComponent<Activator>() != null))
                hit.collider.GetComponent<Activator>().Activate();
        }
    }
}
