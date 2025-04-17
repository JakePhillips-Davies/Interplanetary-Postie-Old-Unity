using System;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float scrollSensitivity = 5f;
    [SerializeField] private float orbitRadius = 5f;
    [SerializeField] private KeyCode moveKey = KeyCode.Mouse1;

    private float yaw;
    private float pitch;

    void Start() {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (Input.GetKey(moveKey)) {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * sensitivity;
            pitch -= mouseY * sensitivity;
            pitch = Mathf.Clamp(pitch, -90, 90);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        orbitRadius -= Mathf.Clamp(transform.position.magnitude, 0.05f, 100000) * Input.mouseScrollDelta.y * scrollSensitivity / 10;
        orbitRadius = Mathf.Clamp(orbitRadius, 0, 100000);

        transform.position = -transform.forward * orbitRadius;
    }

}
