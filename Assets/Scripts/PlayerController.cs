using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float gravity = -9.81f;
    
    // Tracks vertical speed for gravity
    private Vector3 velocity; 

    [Header("Look")]
    public Transform playerCamera;
    public float lookSensitivity = 2f;
    public float sensitivityStep = 0.5f; 
    private float xRotation = 0f;

    private CharacterController characterController;

    // Reset variables
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleSensitivity();
        HandleReset();
    }

    void HandleMovement()
    {
        if (!characterController.gameObject.activeInHierarchy || !characterController.enabled)
        {
            return;
        }

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float currentSpeed = walkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSensitivity()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            lookSensitivity = Mathf.Max(0.1f, lookSensitivity - sensitivityStep);
            Debug.Log("Sensitivity: " + lookSensitivity);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            lookSensitivity += sensitivityStep;
            Debug.Log("Sensitivity: " + lookSensitivity);
        }
    }

    void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            characterController.enabled = false;
            
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            xRotation = 0f;
            playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
            velocity.y = 0f; 
            
            characterController.enabled = true;
            Debug.Log("Player reset to original position.");
        }
    }

    public void LoadLevel(int sceneIndex)
    {
        GameManager.s_Instance.LoadScene(sceneIndex);
    }
}