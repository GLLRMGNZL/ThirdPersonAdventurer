using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float dodgeSpeed = 5f;
    public int dodgeDuration = 30; // in frames
    public float rotationSpeed = 5f;
    public Transform planet;

    private SphereCollider planetCollider;
    private Transform cameraMainTransform;

    private int dodgeDurationCounter = 0;
    private Vector3 dodgeDirection;
    private bool isDodging = false;
    private bool isJumping = false;
    private bool isGrounded = false;

    private Rigidbody rb;
    private PlayerInput input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = new PlayerInput();
        input.Player.Enable();
        planetCollider = planet.GetComponent<SphereCollider>();
        cameraMainTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        input.Player.Jump.performed += OnJump;
        input.Player.Dodge.performed += OnDodge;
    }

    private void OnDisable()
    {
        input.Player.Jump.performed -= OnJump;
        input.Player.Dodge.performed -= OnDodge;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == planetCollider)
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider == planetCollider)
        {
            isGrounded = false;
            isJumping = true;
        }
    }

    private void FixedUpdate()
    {
        // Get player input movement (left joystick position)
        Vector2 movementInput = input.Player.Move.ReadValue<Vector2>();

        if (movementInput.magnitude > 0.1f && !isDodging)
        {
            // Calculate camera-relative movement directions
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraMainTransform.forward, transform.up).normalized;
            Vector3 cameraRight = Vector3.Cross(transform.up, cameraForward).normalized;

            // Create movement vector based on camera orientation
            Vector3 moveDirection = (cameraRight * movementInput.x + cameraForward * movementInput.y).normalized;

            // Move the player
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);

            // Rotate player to face movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, transform.up);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else if (isDodging)
        {
            // Dodge movement
            Vector3 dodgeMovement = dodgeDirection * dodgeSpeed * Time.fixedDeltaTime;

            // Keep distance to planet
            float distanceToPlanet = Vector3.Distance(rb.position, planet.position);
            Vector3 targetPosition = rb.position + dodgeMovement;
            Vector3 directionToPlanet = (targetPosition - planet.position).normalized;
            targetPosition = planet.position + directionToPlanet * distanceToPlanet;

            rb.MovePosition(targetPosition);

            dodgeDurationCounter++;

            // After dodgeDuration, dodgeDurationCounter = 0 and return to normal movement
            if (dodgeDurationCounter >= dodgeDuration)
            {
                isDodging = false;
                dodgeDurationCounter = 0;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isJumping)
        {
            Debug.Log("Already jumping!");
            return;
        }
        else
        {
            Debug.Log("Jump");
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (isDodging)
        {
            Debug.Log("Already dodging!");
            return;
        }
        else
        {
            Debug.Log("Dodge");

            // Get joystick input
            Vector2 inputVector = input.Player.Move.ReadValue<Vector2>();

            // Calculate camera-relative movement directions
            Vector3 cameraForward = Vector3.ProjectOnPlane(cameraMainTransform.forward, transform.up).normalized;
            Vector3 cameraRight = Vector3.Cross(transform.up, cameraForward).normalized;

            // Create movement vector based on camera orientation
            Vector3 moveDirection = (cameraRight * inputVector.x + cameraForward * inputVector.y).normalized;

            // Ensure movement has no vertical component (keep it on the player's plane)
            moveDirection = Vector3.ProjectOnPlane(moveDirection, transform.up).normalized;

            // Set dodge direction
            dodgeDirection = moveDirection;

            isDodging = true;
        }
    }
}