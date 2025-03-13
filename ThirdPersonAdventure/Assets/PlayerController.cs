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
     public GameObject frontView;

     private SphereCollider planetCollider;
     private Transform cameraMainTransform;
     private Quaternion cameraInitialLocalRotation;

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
         Vector3 movementInputVector = input.Player.Move.ReadValue<Vector2>();

        // Convert player input movement into Vector3
        // Vector3 moveDirection = (transform.right * movementInputVector.x + transform.forward * movementInputVector.y).normalized;
        Vector3 moveDirection = new Vector3(movementInputVector.x, 0, movementInputVector.y).normalized;
        /*  3rd person rotation
        // Get player input look (right joystick position)
        Vector3 lookInputVector = input.Player.Look.ReadValue<Vector2>();

        // Convert player input look into Vector3 for 3rd person camera
        Vector3 lookDirection = (cameraMainTransform.right * lookInputVector.x + cameraMainTransform.forward * lookInputVector.y).normalized;

        // Rotate player 3rd person camera
        targetRotation = Mathf.Atan2(lookInputVector.x, lookInputVector.z) * Mathf.Rad2Deg;*/

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            // Create a target rotation based on the target angle
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            // Smoothly interpolate between the current rotation and the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

         if (isDodging)
         {
            // Calcular el movimiento del dodge
            //Vector3 dodgeMovement = dodgeDirection * dodgeSpeed * Time.fixedDeltaTime;
            Vector3 dodgeMovement = moveDirection * dodgeSpeed * Time.fixedDeltaTime;

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