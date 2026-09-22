using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class SimpleFirstPersonController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lookSpeed = 2.2f;
    [SerializeField] private float gravity = 20f;

    private CharacterController characterController;
    private Camera playerCamera;
    private MouseGestureInput mouseGestureInput;
    private float pitch;
    private float verticalVelocity;

    public void Initialize(Camera cameraRef, MouseGestureInput mouseInput)
    {
        playerCamera = cameraRef;
        mouseGestureInput = mouseInput;
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool shouldLock = Cursor.lockState != CursorLockMode.Locked;
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }
    }

    private void HandleLook()
    {
        if (playerCamera == null || (mouseGestureInput != null && mouseGestureInput.IsRecording) || Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        float yaw = Input.GetAxis("Mouse X") * lookSpeed;
        float deltaPitch = Input.GetAxis("Mouse Y") * lookSpeed;
        pitch = Mathf.Clamp(pitch - deltaPitch, -75f, 75f);

        transform.Rotate(0f, yaw, 0f);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        Vector3 move = (transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized;
        if (characterController.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }
}
