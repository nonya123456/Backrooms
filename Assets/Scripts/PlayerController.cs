using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform flashlight;
    [SerializeField] private FlashlightController flashlightController;
    [ReadOnly] [SerializeField] private Vector2 moveInput;
    [ReadOnly] [SerializeField] private Vector2 lookInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    [ReadOnly] [SerializeField] private Vector3 velocity;
    [ReadOnly] [SerializeField] private bool isSprinting;

    [Header("Look")]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float topClamp = 89f;
    [SerializeField] private float bottomClamp = -89f;
    [ReadOnly] [SerializeField] private float pitch;

    private void Update()
    {
        UpdateRotation();
        UpdateVelocity();
        controller.Move(velocity * Time.deltaTime);

        if (isSprinting)
        {
            flashlightController.SetSprintRate();
        }
        else
        {
            flashlightController.SetNormalRate();
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
            var speed = isSprinting ? sprintSpeed : moveSpeed;
            planarVelocity = direction * speed;
        }

        velocity.x = planarVelocity.x;
        velocity.z = planarVelocity.z;
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }
}
