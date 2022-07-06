using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachineCore : MonoBehaviour
{
    // Player Context

    public bool DisableGroundCheck;
    public bool isPressingCrouch;
    public bool isPressingSpace;
    public bool isPressingWSAD;
    public bool isGrounded;

    public Vector2 movementInput;
    public Vector3 CameraRotation;

    // Component Refrences 

    private Animator Animator;
    private Transform Camera;
    private Transform GroundCheck;
    private InputMaster InputController;
    private CharacterController Character;
    [SerializeField] private LayerMask GroundMask;

    // Movement Stats

    private Vector3 Velocity;
    private float RateOfGravity = -50f;
    private float GroundCheckDistance = 0.4f;
    private bool UsingGravity = true;

    // State Info

    public Player_Timers stateMemory;
    private Player_State currentState;
    private bool SwappedThisFrame;

    private void Awake()
    {
        // Get Components
       
        GroundCheck = GameObject.Find("GroundCheck").transform;

        Character = GetComponent<CharacterController>();

        Animator = GetComponentInChildren<Animator>();

        Camera = GameObject.Find("Camera").transform;
       
        // Create Instances

        stateMemory = new Player_Timers();

        currentState = new Player_Idle(this);
        currentState.StartMethod();

        InputController = new InputMaster();
        InputController.Enable();

        // Initial Setup
        Velocity = new Vector3(0f, -2f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }


    private void Update()
    {
        SwappedThisFrame = false;

        UpdatePlayerContext();

        // If no inputs, default state is idle
        if (isGrounded && !isPressingCrouch && !isPressingSpace && !isPressingWSAD)
        {
            SwapState(new Player_Idle(this));
        }

        currentState.CheckForStateSwap();
        currentState.UpdateMethod();

        UpdateMovePlayer();
    }

    private void UpdateMovePlayer()
    {
        var TouchingGround = isGrounded && DisableGroundCheck == false;

        if (TouchingGround)
        {
            Velocity.y = -2f;
        }
        else
        {
            // Falling so increase gravity

            float DeltaV = RateOfGravity * Time.deltaTime;
            float GravityUpdate = currentState.GetUpdateToGravity();

            if (GravityUpdate != 0) { DeltaV *= GravityUpdate; }

            Velocity.y += DeltaV;
        }

        if (UsingGravity)
        { 
            Character.Move(Velocity * Time.deltaTime); 
        }
    }
    
    private void UpdatePlayerContext()
    {
        movementInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);

        isPressingCrouch = InputController.Player.Crouch.ReadValue<float>() == 1f;
        isPressingSpace = InputController.Player.Jump.ReadValue<float>() == 1f;
        isPressingWSAD = movementInput.magnitude >= 0.2f;

        CameraRotation = Camera.eulerAngles;
    }

    /// Helper Methods

    public void SwapState(Player_State input)
    {
        if (SwappedThisFrame == false)
        {
            currentState.ExitMethod();
            currentState = input;
            currentState.StartMethod();
            SwappedThisFrame = true;
        }
    }
    public void ChangeAnimationState(string animation, bool newState)
    {
        Animator.SetBool(animation, newState);
    }

    public void MovePlayer(Vector3 direction, float speed)
    {
        Character.Move(direction.normalized * speed * Time.deltaTime);
    }

    public void EnableGravity(bool state)
    {
        UsingGravity = state;
    }

    public Vector3 GetVelocity()
    {
        return Velocity;
    }

    public void SetVerticalVelocity(float newYVelocity)
    {
        Velocity = new Vector3(Velocity.x, newYVelocity, Velocity.z);
    }
   
}
