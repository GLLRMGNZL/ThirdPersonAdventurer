using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float dodgeForce = 1f;
    public int dodgeDuration = 30; // in frames

    private int dodgeDurationCounter = 0;
    private Vector3 dodgeDirection;
    private bool isDodging = false;
    private bool isJumping = false;

    private Rigidbody rb;
    private PlayerInput input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = new PlayerInput();
        input.Player.Enable();
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

    private void FixedUpdate()
    {
        // Get player input movement (joystick position)
        Vector3 inputVector = input.Player.Move.ReadValue<Vector2>();

        // Convert player input into Vector3
        Vector3 moveDirection = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized;

        if (isDodging)
        {
            rb.AddForce(dodgeDirection * dodgeForce, ForceMode.VelocityChange);
            dodgeDurationCounter++;

            // After dodgeDuration, dodgeDurationCounter = 0 and return to normal movement
            if (dodgeDurationCounter >= dodgeDuration)
            {
                isDodging = false;
                dodgeDurationCounter = 0;
            }
        }
        else
        {
            rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

            // Obtain player movement direction
            Vector2 inputVector = input.Player.Move.ReadValue<Vector2>();
            dodgeDirection = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized;

            isDodging = true;
        }
    }
}