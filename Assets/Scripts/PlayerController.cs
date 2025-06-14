using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform flashlight;
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private PlayerHealth playerHealth;
    [ReadOnly] [SerializeField] private Vector2 moveInput;
    [ReadOnly] [SerializeField] private Vector2 lookInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    [ReadOnly] [SerializeField] private Vector3 velocity;
    [ReadOnly] [SerializeField] private bool isSprinting;
    private bool _isStunned;

    [Header("Look")]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float topClamp = 89f;
    [SerializeField] private float bottomClamp = -89f;
    [ReadOnly] [SerializeField] private float pitch;

    [Header("View Bobbing")]
    [SerializeField] private float bobAmplitude = 0.025f;
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private float sprintBobAmplitude = 0.05f;
    [SerializeField] private float sprintBobFrequency = 2f;
    private float _bobAmount;
    private float _bobOffsetX;
    private Vector3 _originalFlashlightLocalPos;

    public Action OnFootstep;

    private void Start()
    {
        _originalFlashlightLocalPos = flashlight.localPosition;
    }

    private void Update()
    {
        if (_isStunned)
        {
            return;
        }

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

        UpdateFlashlightBobbing();
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

    private void UpdateFlashlightBobbing()
    {
        if (moveInput.magnitude > 0.1f)
        {
            _bobAmount += Time.deltaTime * (isSprinting ? sprintBobFrequency : bobFrequency);

            var amp = isSprinting ? sprintBobAmplitude : bobAmplitude;
            var lastOffsetX = _bobOffsetX;
            _bobOffsetX = Mathf.Sin(2 * Mathf.PI * _bobAmount) * amp;

            flashlight.localPosition = _originalFlashlightLocalPos + _bobOffsetX * Vector3.right;

            if ((lastOffsetX < 0f && _bobOffsetX >= 0f) || (lastOffsetX >= 0f && _bobOffsetX < 0f))
            {
                OnFootstep?.Invoke();
            }
        }
        else
        {
            _bobAmount = 0f;
            flashlight.localPosition =
                Vector3.Lerp(flashlight.localPosition, _originalFlashlightLocalPos, Time.deltaTime * 5f);
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

    private void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    private void OnEnable()
    {
        playerHealth.OnPlayerHurt += HandlePlayerHurt;
    }

    private void OnDisable()
    {
        playerHealth.OnPlayerHurt += HandlePlayerHurt;
    }

    private void HandlePlayerHurt()
    {
        StartCoroutine(HandlePlayerHurtCoroutine());
    }

    private IEnumerator HandlePlayerHurtCoroutine()
    {
        _isStunned = true;

        var originalPosition = cameraTarget.localPosition;
        const float shakeDuration = 0.5f;
        const float shakeIntensity = 0.05f;
        var elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            var offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            var offsetY = Random.Range(-1f, 1f) * shakeIntensity;
            cameraTarget.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTarget.localPosition = originalPosition;

        yield return new WaitForSeconds(0.5f);

        _isStunned = false;
    }
}
