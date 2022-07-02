using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachineCore : MonoBehaviour
{

    // Used by every State
    public bool isPressingCrouch;
    public bool isPressingSpace;
    public bool isPressingWSAD;

    public Vector2 movementInput;
    public bool isGrounded;

    public Vector3 CameraRotation;

    public Player_Timers stateMemory;

    // Only State Editable in public
    public bool DisableGroundCheck;

    // Used Internally 
    private Transform Camera;
    private Animator Animator;
    private Transform GroundCheck;
    [SerializeField] private LayerMask GroundMask;
    private InputMaster InputController;
    private CharacterController Character;

    private Player_State currentStateEX;
    private float VelocityChange;
    private bool UsingGravity = true;

    private float RateOfGravity = -50f;
    private Vector3 Velocity;

    

    


    // Private Variables
    float GroundCheckDistance = 0.4f;
    public bool holdingJump = false;


    


    private void Awake()
    {
        // Component Referencing
        stateMemory = new Player_Timers();


        Camera = GameObject.Find("Camera").transform;
        GroundCheck = GameObject.Find("GroundCheck").transform;
        currentStateEX = new Player_Idle(this);
        currentStateEX.StartMethod();

        Character = GetComponent<CharacterController>();
        InputController = new InputMaster();
        InputController.Enable();

        Animator = GetComponentInChildren<Animator>();

        // Initial Setup
        Velocity = new Vector3(0f, -2f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void ReadInputs()
    {
        movementInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);

        isPressingCrouch = InputController.Player.Crouch.ReadValue<float>() == 1f;
        isPressingSpace = InputController.Player.Jump.ReadValue<float>() == 1f;
        isPressingWSAD = movementInput.magnitude >= 0.2f;

        CameraRotation = Camera.eulerAngles;
    }

    private void Update()
    {
        ReadInputs();

        if(isGrounded && !isPressingCrouch && !isPressingSpace && !isPressingWSAD)
        {
            SwapState(new Player_Idle(this));
        }

        // holdingJump prevents repeated ghost jumps
        if (!isPressingSpace) { holdingJump = false; }

        VelocityChange = RateOfGravity * Time.deltaTime;

        currentStateEX.CheckForStateSwap();
        currentStateEX.UpdateMethod();

        float GravityUpdate = currentStateEX.GetUpdateToGravity();

        if(GravityUpdate != 0)
        {
            VelocityChange *= GravityUpdate;
        }

        // Reset Velocity when grounded, else update velocity!

        if (isGrounded && DisableGroundCheck == false)
        {
            Velocity.y = -2f;
        }
        else
        {
            Velocity.y += VelocityChange;
        }

        if (UsingGravity)
        {
            Character.Move(Velocity * Time.deltaTime);
        }

        /// Ground Pound Animation Handler
   
    }

    /// Helper Methods

    public void SwapState(Player_State input)
    {
        currentStateEX.ExitMethod();
        currentStateEX = input;
        currentStateEX.StartMethod();
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
