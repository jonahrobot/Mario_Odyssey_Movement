using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // --- Variables --- //

    [Header("Movement Controls")]
    public float SprintSpeed;
   
    [Header("Editor Refrences")]
    public Transform Camera;
    public Transform GroundCheck;
    public LayerMask GroundMask;

    // Private Variables

    private InputMaster InputController;
    private CharacterController Character;
    
    private float TurnSpeed = 0.1f;

    private float RateOfGravity = -9.81f;
    private Vector3 Velocity;

    private float GroundCheckDistance = 0.4f;
    private bool IsGrounded;

    private float JumpHeight = 3f;

    // Refrences

    float turnSmoothVelocity;

    // --- Methods --- //

    void Awake()
    {
        InputController = new InputMaster();
        InputController.Enable();

        Character = GetComponent<CharacterController>();

        Cursor.visible = false;
    }

    private void Update()
    {
        // Check if on ground
        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);

        // If On Ground, Stop Player
        if(IsGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f;
        }

        var UserInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

        // Base Run Movement
        if (UserInput.magnitude >= 0.1f)
        {
            float TargetAngle = Mathf.Atan2(UserInput.x, UserInput.y) * Mathf.Rad2Deg + Camera.eulerAngles.y;
            float CurrentAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

            transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

            Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
            Character.Move(Direction.normalized * SprintSpeed * Time.deltaTime);
        }

        var JumpInput = InputController.Player.Jump.ReadValue<float>();

        if (JumpInput == 1f && IsGrounded)
        {
            Velocity.y = Mathf.Sqrt(-2f * RateOfGravity * JumpHeight);
        }


        // Gravity Movement
        Velocity.y += RateOfGravity * Time.deltaTime;

        Character.Move(Velocity * Time.deltaTime);
    }
}
