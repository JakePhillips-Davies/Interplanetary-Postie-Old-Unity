using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private int walkSpeed;
    [SerializeField] private int sprintSpeed;
    [SerializeField] private int crouchSpeed;
    [SerializeField] private int evaPackForce;
    [SerializeField] private float rotationTorque;
    [SerializeField] private Vector3 gravity;

    [Space(7)]
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    
    [Space(7)]
    [SerializeField] private float jumpForce;

    [Space(7)]
    [SerializeField] private float groundCheckSphereRadius;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckDistanceCrouch;

    [Space(7)]
    [Header("Key Binds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode rotateLeft = KeyCode.Q;
    [SerializeField] private KeyCode rotateRight = KeyCode.E;
    [SerializeField] private KeyCode sprint = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouch = KeyCode.LeftControl;

    [Space(7)]
    [Header("refs")]
    [SerializeField] private PlayerCam cam;


    private float forwardInput;
    private float rightInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private float moveSpeed;

    private bool readyToJump;
    private bool safeToUncrouch;
    private bool crouching;

    public bool grounded;
    
    private RaycastHit hitInfo;
    private RaycastHit hitInfoDud;
    private Rigidbody rb;

    //---                                ---//

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        readyToJump = true;
    }

    private void Start() {
        SpaceControllerSingleton.Get.Setplayer(transform.parent.gameObject); // Ehhh could be better, but whatever
    }

    void FixedUpdate()
    {

        GroundCheck();

        if(Input.GetKey(crouch)){
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            crouching = true;
        }
        if(crouching){
            safeToUncrouch = !(Physics.SphereCast(transform.position, 0.495f, transform.up, out hitInfoDud, 1));
        }
        if(safeToUncrouch && !Input.GetKey(crouch)){
            transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            crouching = false;
        }

        HandleRotation();

        GetMovement();

        if(Input.GetKey(jumpKey))
            Jump();

        MovePlayer();

        Drag();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            SwitchMapView();
        }
    }



    void GetMovement()
    {
        if(crouching){
            moveSpeed = crouchSpeed;
        }
        else if(Input.GetKey(sprint)){
            moveSpeed = sprintSpeed;
        }
        else{
            moveSpeed = walkSpeed;
        }

        forwardInput = Input.GetAxisRaw("Vertical");
        rightInput = Input.GetAxisRaw("Horizontal");

        if (!grounded) verticalInput = Input.GetAxisRaw("UppyDowny");
        else verticalInput = 0;

        moveDirection = (transform.forward * forwardInput + transform.right * rightInput + transform.up * verticalInput).normalized;
        if(grounded){
            moveSpeed *= Mathf.Pow(1 + Vector3.Dot(moveDirection, hitInfo.normal), 1.3f); // make the player slower on steeper terrain by multiplying movespeed by the dot product + 1
            moveDirection = Vector3.ProjectOnPlane(moveDirection, hitInfo.normal);
        }
    }

    void MovePlayer()
    {

        Vector3 moveForce;

        moveForce = moveDirection.normalized * moveSpeed * rb.mass;
        
        if(!grounded) {
            moveForce = moveDirection.normalized * (Input.GetKey(sprint) ? evaPackForce * 1.5f : evaPackForce) + gravity * rb.mass;
        }

        rb.AddForce(moveForce, ForceMode.Force);
    }

    void GroundCheck()
    {
        if(gravity.sqrMagnitude == 0) {
            grounded = false;
        }
        else if(crouching){
            grounded = Physics.SphereCast(transform.position, groundCheckSphereRadius, -transform.up, out hitInfo, groundCheckDistanceCrouch, 1 << gameObject.layer);
        }
        else
            grounded = Physics.SphereCast(transform.position, groundCheckSphereRadius, -transform.up, out hitInfo, groundCheckDistance, 1 << gameObject.layer);
    }

    void Drag()
    {
        if(grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = airDrag;
        
    }

    void Jump()
    {
        if(grounded && readyToJump){
            readyToJump = false;
            grounded = false;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), 0.2f);
        }
    }
    void ResetJump()
    {
        readyToJump = true;
    }

    void HandleRotation() {

        if (gravity.magnitude != 0) {

            float rotationY = transform.rotation.eulerAngles.y;

            transform.up = -gravity;

            transform.localRotation = Quaternion.Euler(new(transform.rotation.eulerAngles.x, rotationY, transform.rotation.eulerAngles.z));

        }
        else {

            transform.Rotate(cam.transform.localEulerAngles, Space.Self);
            cam.transform.localEulerAngles = Vector3.zero;

            if (Input.GetKey(rotateLeft)) transform.Rotate(transform.forward * rotationTorque * Time.fixedDeltaTime, Space.World);
            else if (Input.GetKey(rotateRight)) transform.Rotate(transform.forward * -rotationTorque * Time.fixedDeltaTime, Space.World);

        }

    }

    public void Teleport(Transform to)
    {
        transform.position = to.position;
        transform.rotation = to.rotation;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position-transform.up*groundCheckDistanceCrouch, groundCheckSphereRadius);
        Gizmos.DrawWireSphere(transform.position-transform.up*groundCheckDistance, groundCheckSphereRadius);
    }


    private void SwitchMapView() {
        ScaleSpaceSingleton.Get.cam.GetComponent<MatchRotation>().enabled = !ScaleSpaceSingleton.Get.cam.GetComponent<MatchRotation>().enabled;
        ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled = !ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled;

        ScaleSpaceSingleton.Get.cam.transform.position = Vector3.zero;

        if (ScaleSpaceSingleton.Get.cam.GetComponent<MapCamera>().enabled) {
            cam.cam.gameObject.SetActive(false);
            cam.gameObject.SetActive(false);
            ScaleSpaceSingleton.Get.cam.GetComponent<Camera>().fieldOfView = 75;
        }
        else {
            cam.gameObject.SetActive(true);
            cam.cam.gameObject.SetActive(true);
        }
    }
}
