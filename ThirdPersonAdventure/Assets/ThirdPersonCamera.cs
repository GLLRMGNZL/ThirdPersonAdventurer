using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform planetTransform;

    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 1.5f;
    public float smoothSpeed = 10f;
    public Vector2 sensitivity = new Vector2(2f, 1.5f);
    public Vector2 pitchLimits = new Vector2(-30f, 60f); // Min, Max pitch angle

    [Header("Collision Settings")]
    public float collisionOffset = 0.3f;
    public LayerMask collisionLayers;

    private float currentYaw;
    private float currentPitch;
    private Vector3 currentVelocity = Vector3.zero;
    private PlayerInput input;
    private Vector2 lookInput;
    private bool isMoving;

    private void Awake()
    {
        input = new PlayerInput();
        input.Player.Enable();

        // Initialize camera position
        transform.position = CalculateTargetPosition();
        transform.LookAt(playerTransform.position);

        // Extract initial rotation values
        currentYaw = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
    }

    private void OnEnable()
    {
        input.Player.Look.performed += OnLook;
        input.Player.Look.canceled += OnLook;
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        input.Player.Look.performed -= OnLook;
        input.Player.Look.canceled -= OnLook;
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        isMoving = context.ReadValue<Vector2>().magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        // Update rotation based on input
        currentYaw += lookInput.x * sensitivity.x;
        currentPitch -= lookInput.y * sensitivity.y;
        currentPitch = Mathf.Clamp(currentPitch, pitchLimits.x, pitchLimits.y);

        // Calculate up direction relative to planet
        Vector3 gravityUp = (playerTransform.position - planetTransform.position).normalized;

        // Create rotation based on yaw and pitch
        Quaternion rotation = Quaternion.AngleAxis(currentYaw, gravityUp) *
                             Quaternion.FromToRotation(Vector3.up, gravityUp);

        // Add pitch rotation
        Vector3 right = Vector3.Cross(gravityUp, rotation * Vector3.forward).normalized;
        Quaternion pitchRotation = Quaternion.AngleAxis(currentPitch, right);
        rotation = pitchRotation * rotation;

        // Calculate target position
        Vector3 targetPosition = CalculateTargetPosition();

        // Check for collisions
        RaycastHit hit;
        if (Physics.Linecast(playerTransform.position + gravityUp * height, targetPosition, out hit, collisionLayers))
        {
            targetPosition = hit.point + hit.normal * collisionOffset;
        }

        // Smoothly interpolate position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, Time.deltaTime * smoothSpeed);

        // Rotate the camera to look at player
        transform.rotation = rotation;
        transform.LookAt(playerTransform.position + gravityUp * height);

        // Rotate player to match camera direction when moving
        if (isMoving)
        {
            // Calculate target rotation for player (only yaw, not pitch)
            Quaternion targetPlayerRotation = Quaternion.AngleAxis(currentYaw, gravityUp) *
                                           Quaternion.FromToRotation(Vector3.up, gravityUp);

            // Apply rotation to player transform
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetPlayerRotation,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        // Calculate up direction relative to planet
        Vector3 gravityUp = (playerTransform.position - planetTransform.position).normalized;

        // Create rotation based on yaw and pitch
        Quaternion rotation = Quaternion.AngleAxis(currentYaw, gravityUp) *
                             Quaternion.FromToRotation(Vector3.up, gravityUp);

        // Add pitch rotation
        Vector3 right = Vector3.Cross(gravityUp, rotation * Vector3.forward).normalized;
        Quaternion pitchRotation = Quaternion.AngleAxis(currentPitch, right);
        rotation = pitchRotation * rotation;

        // Calculate target position behind player
        Vector3 targetPosition = playerTransform.position +
                                gravityUp * height -
                                (rotation * Vector3.forward) * distance;

        return targetPosition;
    }
}
