using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform flashlight;
    [ReadOnly] [SerializeField] private Vector2 moveInput;
    [ReadOnly] [SerializeField] private Vector2 lookInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float gravity = -10f;
    [SerializeField] private float fastFallingGravity = -15f;
    [SerializeField] private float jumpSpeed = 4.5f;
    [SerializeField] private float initialFallingSpeed = -2f;
    [SerializeField] private float maxFallingSpeed = -50f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.1f;
    [ReadOnly] [SerializeField] private Vector3 velocity;
    private float _coyoteTimer;
    private float _jumpBufferTimer;

    [Header("Look")]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float topClamp = 89f;
    [SerializeField] private float bottomClamp = -89f;
    [ReadOnly] [SerializeField] private float pitch;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        UpdateRotation();
        UpdateJump();
        UpdateVelocity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateJump()
    {
        if (controller.isGrounded)
        {
            _coyoteTimer = coyoteTime;
        }
        else if (_coyoteTimer > 0f)
        {
            _coyoteTimer -= Time.deltaTime;
        }

        _jumpBufferTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            velocity.y = jumpSpeed;
        }
    }

    private void UpdateRotation()
    {
        pitch -= lookInput.y * sensitivity;
        if (pitch < -360f)
        {
            pitch += 360f;
        }

        if (pitch > 360f)
        {
            pitch -= 360f;
        }

        pitch = Mathf.Clamp(pitch, bottomClamp, topClamp);

        cameraTarget.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        flashlight.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);

        var horizontalRotate = lookInput.x * sensitivity;
        transform.Rotate(Vector3.up * horizontalRotate);
    }

    private void UpdateVelocity()
    {
        var planarVelocity = Vector3.zero;
        if (moveInput.magnitude > 0.1f)
        {
            var direction = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
            planarVelocity = direction * moveSpeed;
        }

        if (controller.isGrounded)
        {
            velocity.x = planarVelocity.x;
            velocity.z = planarVelocity.z;
        }
        else
        {
            var horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            var velocityChange = (planarVelocity - horizontalVelocity).normalized * (airAcceleration * Time.deltaTime);
            var finalVelocity = horizontalVelocity + velocityChange;
            velocity.x = finalVelocity.x;
            velocity.z = finalVelocity.z;
        }

        if (controller.isGrounded && velocity.y <= 0f)
        {
            velocity.y = initialFallingSpeed;
        }
        else
        {
            var finalGravity = velocity.y > 0f ? gravity : fastFallingGravity;
            velocity.y += finalGravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, maxFallingSpeed, Mathf.Infinity);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y < -0.7f)
        {
            velocity.y = initialFallingSpeed;
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void OnJump(InputValue value)
    {
        _jumpBufferTimer = jumpBufferTime;
    }
}
