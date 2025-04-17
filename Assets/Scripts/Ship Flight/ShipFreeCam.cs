using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipFreeCam : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float sensitivity;
    private float yRot;
    private float xRot;
    [SerializeField] private KeyCode freeMouseKey;
    public bool isFreeLooking { get; set; } = false;

    [Header("")]
    [Header("Dot reference")]
    [SerializeField] private GameObject dot;

    [Header("")]
    [Header("Interaction")]
    [SerializeField] private int reach;        public int getReach() { return reach; }
    [SerializeField] private int interactionReach;
    [SerializeField] private KeyCode interactkey;

    private RaycastHit hit;
    private Ray interactRay;        public Ray GetInteractRay() { return interactRay; }

    void Update()
    {
        if(Input.GetKeyDown(freeMouseKey)){
            isFreeLooking = !isFreeLooking;
        }

        if(Input.GetKey(KeyCode.Mouse1)) {
            GetComponent<Camera>().fieldOfView = 30;
            ScaleSpaceSingleton.Get.cam.GetComponent<Camera>().fieldOfView = 30;
        }
        else {
            GetComponent<Camera>().fieldOfView = 75;
            ScaleSpaceSingleton.Get.cam.GetComponent<Camera>().fieldOfView = 75;
        }
        
        if(!isFreeLooking){
            Cursor.lockState = CursorLockMode.Locked;

            yRot -= 180;
            yRot += Input.GetAxisRaw("Mouse X") * sensitivity;
            yRot = Mathf.Clamp(yRot, -90f, 90f);
            yRot += 180;

            xRot -= Input.GetAxisRaw("Mouse Y") * sensitivity;
            xRot = Mathf.Clamp(xRot, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRot, yRot, 0);
        }
        else{
            Cursor.lockState = CursorLockMode.Confined;
            transform.localRotation = Quaternion.Euler(xRot, yRot, 0);
        }

        interactRay = gameObject.GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
    }

    private void OnEnable() {
        Input.mousePosition.Set(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Cursor.visible = true;
        dot.SetActive(false);
        xRot = 0;
        yRot = 180;
    }

    private void OnDisable() {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Input.mousePosition.Set(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        transform.localRotation = Quaternion.Euler(0, 180, 0);
        dot.SetActive(true);
    }

    private void OnDrawGizmos() {
        
        Gizmos.DrawRay(transform.position, interactRay.direction * reach);

    }
}
    
