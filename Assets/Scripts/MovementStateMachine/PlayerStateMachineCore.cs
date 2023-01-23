using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachineCore : MonoBehaviour
{


    // PUBLIC VARIABLES  -------------------------------------------------------------------------

    // Movement
    [Header("Jump")]
    public float baseJumpHorzSpeed = 5f;
    public float[] JumpHeights = { 35f, 37f, 43f };
    public float gravityOnFall = 3.0f;
    public float gravityOnShortFall = 4.0f;

    [Header("Running")]
    public float Acceleration = 0.5f;
    public float Deceleration = -0.125f;
    public float MaxSpeed = 20f;

    [Header("Crouching")]
    public float CrouchSpeed = 5f;

    [Header("Crouch Jumping")]
    public float JumpVelocity = 43f;
    public float FallMultiplier = 4.0f;
    public float Speed = 5f;

    [Header("Long Jump")]
    public float HorizontalSpeed = 30f;
    public float LongJumpVelocity = 15f;
    public float LongFallMultiplier = 3.0f;

    [Header("Roll")]
    public float CurrentSpeed = 40f;
    public float MaxRollSpeed = 50f;
    public float SprintAcceleration = 1.0025f;
    public float SlowDownStart = 0.0225f;

    [Header("Base Stats")]
    public float rateOfGravity = -50f;

    [Header("Environment")]
    [SerializeField] private LayerMask _groundMask;


    // PRIVATE / HIDDEN VARIABLES -------------------------------------------------------------------------


    // Parts of State Machine
    [HideInInspector] public Player_Timers StateMemory;
    [HideInInspector] public State_Animation_Controller AnimationController;
    [HideInInspector] public State_Context_Handler StateContext;

    // Component Refrences 
    private CharacterController _character;
    private MarioHatCore _marioHat;

    // Movement Information
    private Vector3 _velocity;
    private bool _usingGravity = true;

    // General State Information
    private Player_State _currentState;
    [HideInInspector] public bool IsIdle;
    [HideInInspector] public bool CollidingWithHat;
    private bool _swappedThisFrame;
    private bool _preventIdleSwap = false;


    // GRAPH DEBUG VARIABLES -------------------------------------------------------------------------


    //[DebugGUIGraph(min: 0, max: 30, r: 1, g: 0, b: 0, autoScale: false)]
    [HideInInspector] public float SpeedDebug;


    // Scripts -------------------------------------------------------------------------

    private void Awake()
    {
        // Extentions
        StateContext = new State_Context_Handler(_groundMask);

        AnimationController = new State_Animation_Controller(GetComponentInChildren<Animator>());
        
        StateMemory = new Player_Timers();

        // Get Components
        _marioHat = GameObject.Find("MarioHat").GetComponent<MarioHatCore>();

        _character = GetComponent<CharacterController>();

        // Initial Setup
        _currentState = new Player_Idle(this);
        _currentState.StartMethod();

        _velocity = new Vector3(0f, -2f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        Debug.Log(_currentState);

        // Get Current User Inputs
        StateContext.UpdateInputs();

        _swappedThisFrame = false;

        /*
         * Swap states if conditions met
         * 
         * Example: IDLE state, user presses space, now swap to JUMP state
         */
        _currentState.CheckStateSwaps();

        /*
         * Check if idle state conditions met
         * Placed here as all states have idle option
         */
        if (StateContext.IsGrounded && !StateContext.IsCrouched && !StateContext.IsJumping && !StateContext.IsMoving && IsIdle == false && _preventIdleSwap == false)
        {
            SwapState(new Player_Idle(this));
            IsIdle = true;
        }

        // Preform current states update method
        _currentState.UpdateMethod();

        // Move Player
        UpdateMovePlayer();
    }

    // Handles players vertical velocity movement
    // Includes gravity and whatever current states (GetUpdateToGravity alteration)
    private void UpdateMovePlayer()
    {
        var TouchingGround = StateContext.IsGrounded && StateContext.DisableGroundCheck == false;

        if (TouchingGround)
        {
            _velocity.y = -2f;
        }
        else
        {
            // Falling so increase gravity

            float DeltaV = rateOfGravity * Time.deltaTime;
            float GravityUpdate = _currentState.GetUpdateToGravity();

            if (GravityUpdate != 0) { DeltaV *= GravityUpdate; }

            _velocity.y += DeltaV;
        }

        if (_usingGravity)
        {
            _character.Move(_velocity * Time.deltaTime);
        }
    }


    /// Helper Methods

    /*
     * Swap current state to "input" state
     * 
     * Params
     *  - Player_State input - The new state that will be swapped in for the old state
     *  
     */
    public void SwapState(Player_State input)
    {
        if (_swappedThisFrame == false)
        {
            AnimationController.ChangeAnimationSpeed(1);
            _currentState.ExitMethod();
            _currentState = input;
            _currentState.StartMethod();
            _swappedThisFrame = true;
        }
    }

    /*
     * Move players character controller by a direction and speed, framerate relative
     * 
     * Params
     *  - Vector3 direction - Direction to move player
     *  - float speed       - speed of player movement
     * 
     */
    public void MovePlayer(Vector3 direction, float speed)
    {
        _character.Move(direction.normalized * speed * Time.deltaTime);
    }

    public void EnableGravity(bool state)
    {
        _usingGravity = state;
    }

    public Vector3 GetVelocity()
    {
        return _velocity;
    }

    public void SetVerticalVelocity(float newYVelocity)
    {
        _velocity = new Vector3(_velocity.x, newYVelocity, _velocity.z);
    }

    public bool GetPreventIdleSwap()
    {
        return _preventIdleSwap;
    }

    public void SetPreventIdleSwap(bool value)
    {
        _preventIdleSwap = value;
    }

    public MarioHatCore GetMarioHatCore()
    {
        return _marioHat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hat" && StateContext.HasHat == false)
        {
            CollidingWithHat = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Hat")
        {
            CollidingWithHat = false;
        }
    }
}
