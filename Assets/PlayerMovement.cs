using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // --- Variables --- //

    [Header("Movement Controls")]
    public float BaseSprintSpeed = 15f;
    public float MaxSprintSpeed = 25f;
    public float SprintAcceleration = 1.00125f;

    [Space]
    public float RateOfGravity = -50;
    public float InitialJumpVelocity = 25f;
    public float FallMultiplier = 2.0f;
    public float LetGoMultiplier = 3.0f;

    [Header("Editor Refrences")]
    public Transform Camera;
    public Transform GroundCheck;
    public LayerMask GroundMask;

    // Private Variables

    private InputMaster InputController;
    private CharacterController Character;

    private float TurnSpeed = 0.1f;
    private Vector3 Velocity;
    private float GroundCheckDistance = 0.4f;
    private bool IsGrounded;
    private bool HoldingJump;
    private bool AlreadyLetGo;
    private float CurrentSprintSpeed;
    private int JumpCombo = 0;
    private bool JumpLeftGround = false;

    private float CurrentJumpHeight = 0;

    private Coroutine Reset = null;

    // Refrences

    float turnSmoothVelocity;

    // --- Methods --- //

    void Awake()
    {
        InputController = new InputMaster();
        InputController.Enable();

        Character = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        CurrentSprintSpeed = BaseSprintSpeed;
    }

    IEnumerator ResetJumpCount()
    {
        Reset = null;
        yield return new WaitForSeconds(0.4f);
        JumpCombo = 0;

    }

    private void Update()
    {
        // Check if on ground
        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);

        // If On Ground, Stop Player
        if (IsGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f;
            AlreadyLetGo = false;

            // Only reset jump when coming back to ground, not while still on ground and jump was triggered!
            if (JumpLeftGround)
            {
                JumpLeftGround = false;
                if (Reset != null)
                {
                    StopCoroutine(Reset);
                }
                Reset = StartCoroutine(ResetJumpCount());
            }
        }

        if (!IsGrounded && Velocity.y > 0)
        {
            JumpLeftGround = true;
        }
        
        RunHandler();

        JumpHandler();

        Character.Move(Velocity * Time.deltaTime);
    }

    private void RunHandler()
    {
        var UserInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

        // Base Run Movement
        if (UserInput.magnitude >= 0.1f)
        {
            float TargetAngle = Mathf.Atan2(UserInput.x, UserInput.y) * Mathf.Rad2Deg + Camera.eulerAngles.y;
            float CurrentAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

            // Set Angles into range of 0-360

            var TargetRefined = TargetAngle % 360;
            if(TargetRefined < 0) { TargetRefined += 360; }

            var CurrentRefined = CurrentAngle % 360; 
            if (CurrentRefined < 0) { CurrentRefined += 360; }

            var DifferenceChange = 180;
            if(CurrentSprintSpeed < MaxSprintSpeed * 0.75)
            {
                DifferenceChange = 20;
            }

            // Slow Player on Quick Direction Switch

            if (Mathf.Abs(TargetRefined - CurrentRefined) > DifferenceChange)
            {
                CurrentSprintSpeed = Mathf.Max(CurrentSprintSpeed * 0.5f, BaseSprintSpeed);
            }

            transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

            Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
            
            // Accelerate player
            if(CurrentSprintSpeed < MaxSprintSpeed)
            {
                CurrentSprintSpeed *= SprintAcceleration;
            }

            Character.Move(Direction.normalized * CurrentSprintSpeed * Time.deltaTime);
        }
        else
        {
            CurrentSprintSpeed = BaseSprintSpeed;
        }
    }

    // ** Jumping **

    private void JumpHandler()
    {
        var JumpInput = InputController.Player.Jump.ReadValue<float>();

        // Only jump when on ground and not holding jump button from previous jump
        if (JumpInput == 1f && IsGrounded && HoldingJump == false)
        {
            if (Velocity.y < InitialJumpVelocity)
            {
                // inital Jump Vertical Velocity Boost, Jump Trigger

                var UserInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

                // Base Run Movement

                // If trying to tripple jump but not moving, cancel it
                if (JumpCombo == 2 && UserInput.magnitude < 0.1f)
                {
                    JumpCombo = 0;
                }

                CurrentJumpHeight = InitialJumpVelocity + 3 * (Mathf.Min(3, JumpCombo + 1));
                Velocity.y = CurrentJumpHeight;
                JumpCombo += 1;

                if(JumpCombo > 2)
                {
                    JumpCombo = 0;
                }

                if (Reset != null)
                {
                    StopCoroutine(Reset);
                }
            }
        }

        // Default Velocity Change
        var VelocityChange = RateOfGravity * Time.deltaTime;

        // Increase Falling Velocity
        if ((JumpInput == 0f && Velocity.y > CurrentJumpHeight / 2) || AlreadyLetGo)
        {
            // If Jump button was released before jumps apex, short jump occures
            if (Velocity.y < CurrentJumpHeight * 0.75f)
            {
                VelocityChange *= LetGoMultiplier;
            }
            AlreadyLetGo = true;
        }
        else
        {
            // If jump has reached apex, fall is faster
            if (Velocity.y < CurrentJumpHeight / 2)
            {
                VelocityChange *= FallMultiplier;
            }
        }

        // Handles Ghosting Jump Button
        HoldingJump = JumpInput == 1f;

        // Gravity Movement
        Velocity.y += VelocityChange;
    }

}
