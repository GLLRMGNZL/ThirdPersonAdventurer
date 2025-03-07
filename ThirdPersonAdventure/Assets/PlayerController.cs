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
    public Transform planet;
    public GameObject frontView;

    private SphereCollider planetCollider;

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
        // Get player input movement (joystick position)
        Vector3 inputVector = input.Player.Move.ReadValue<Vector2>();

        // Convert player input into Vector3
        Vector3 moveDirection = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized;

        if (isDodging)
        {
            // Calcular el movimiento del dodge
            Vector3 dodgeMovement = dodgeDirection * dodgeSpeed * Time.fixedDeltaTime;

            // Mantener la distancia al planeta
            float distanceToPlanet = Vector3.Distance(rb.position, planet.position);
            Vector3 targetPosition = rb.position + dodgeMovement;
            Vector3 directionToPlanet = (targetPosition - planet.position).normalized;
            targetPosition = planet.position + directionToPlanet * distanceToPlanet;

            // Aplicar el movimiento
            rb.MovePosition(targetPosition);

            dodgeDurationCounter++;

            // Aplicar el movimiento
            rb.MovePosition(targetPosition);

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

            // Obtener el input del joystick
            Vector2 inputVector = input.Player.Move.ReadValue<Vector2>();

            // Convertir el input del joystick en un vector en el espacio de la cámara
            Vector3 moveDirection = (frontView.transform.right * inputVector.x + frontView.transform.forward * inputVector.y).normalized;

            // Asegurar que el movimiento no tenga componente vertical (mantenerlo en el plano del personaje)
            moveDirection = Vector3.ProjectOnPlane(moveDirection, transform.up).normalized;

            // Asignar la dirección de dodge
            dodgeDirection = moveDirection;

            isDodging = true;
        }
    }
}