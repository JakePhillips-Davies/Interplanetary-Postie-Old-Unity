using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class PlayerCam : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float sensitivity;
    private float yRot;
    private float xRot;
    [SerializeField] private KeyCode freeMouseKey;
    public bool isFreeLooking { get; set; } = false;

    [Header("")]
    [Header("Body reference")]
    [SerializeField] private GameObject body;

    [Header("")]
    [Header("Interaction")]
    [SerializeField] private int reach;        public int getReach() { return reach; }
    [SerializeField] private int interactionReach;
    [SerializeField] private KeyCode interactkey;
    [SerializeField] private GameObject panel;
    [SerializeField] private Text text;
    [Header("")]
    [SerializeField] private GameObject buildingTool;
    [SerializeField] private KeyCode buildingToolKey;
    private RaycastHit hit;
    private Ray interactRay;        public Ray GetInteractRay() { return interactRay; }

    void Update()
    {
        if(Input.GetKeyDown(freeMouseKey)){
            isFreeLooking = !isFreeLooking;
        }
        
        if(!isFreeLooking){
            Cursor.lockState = CursorLockMode.Locked;

            yRot += Input.GetAxisRaw("Mouse X") * sensitivity;

            xRot -= Input.GetAxisRaw("Mouse Y") * sensitivity;

            transform.Rotate(xRot, 0, 0, Space.Self);
            Vector3 eulerAngles = transform.localEulerAngles;
            if((eulerAngles.x > 180) && (eulerAngles.y >= 180)) eulerAngles.x = 270;
            if((eulerAngles.x < 180) && (eulerAngles.y >= 180)) eulerAngles.x = 90;
            transform.localRotation = Quaternion.Euler(eulerAngles.x, 0, 0);

            body.transform.Rotate(0, yRot, 0, Space.Self);
            body.transform.localRotation = Quaternion.Euler(0, body.transform.localEulerAngles.y, 0);
            xRot = 0;
            yRot = 0;
        }
        else{
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        interactRay = gameObject.GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);

        activatableHandler();

        if(Input.GetKeyDown(buildingToolKey) && (buildingTool != null)) buildingTool.SetActive(!buildingTool.activeSelf);
    }

    private void OnEnable() {
        if(buildingTool != null) buildingTool.SetActive(false);
        Cursor.visible = true;

    }
    private void OnDisable() {
         if(buildingTool != null) buildingTool.SetActive(false);
    }

    private void OnDrawGizmos() {
        
        Gizmos.DrawRay(transform.position, interactRay.direction * reach);

    }

    void activatableHandler()
    {
        // text.text = "";
        // panel.SetActive(false);

        // if(Physics.Raycast(transform.position, transform.forward, out hit, interactionReach, LayerMask.GetMask("Activator"))){
        //     if(Input.GetKeyDown(interactkey) && (hit.transform != null) && (hit.collider.GetComponent<Activator>() != null))
        //         hit.collider.GetComponent<Activator>().Activate();

        //     if((hit.transform != null) && (hit.collider.GetComponent<Activator>() != null)){
        //         panel.SetActive(true);
        //         text.text = String.Format("{0}", hit.collider.name);
        //     }
        // }
    }
}
