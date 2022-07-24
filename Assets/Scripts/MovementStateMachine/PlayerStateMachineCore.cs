using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent] should add!
public class PlayerStateMachineCore : MonoBehaviour
{
    // Player Context
    public Player_Timers StateMemory;
    public State_Animation_Controller AnimationController;
    public State_Context_Handler StateContext;


    [HideInInspector] public bool IsIdle;

    [HideInInspector] public bool CollidingWithHat;


    // Component Refrences 
    private CharacterController _character;
    private MarioHatCore _marioHat;
    [SerializeField] private LayerMask _groundMask;

    // Movement Stats

    private Vector3 _velocity;
    private static float s_rateOfGravity = -50f;
    private bool _usingGravity = true;

    // State Info

    

    private bool _swappedThisFrame;


    //private bool _hasHat = true;
    private bool _preventIdleSwap = false;

    /// Variables I like really need
    private Player_State _currentState;


    //[DebugGUIGraph(min: 0, max: 30, r: 1, g: 0, b: 0, autoScale: false)]
    public float SpeedDebug;

    private void Awake()
    {
        // Extentions
        AnimationController = new State_Animation_Controller(GetComponentInChildren<Animator>());
        StateContext = new State_Context_Handler(_groundMask);
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
        _swappedThisFrame = false;

        _currentState.CheckStateSwaps();

        // Can be put into states
        if (StateContext.IsGrounded && !StateContext.IsCrouched && !StateContext.IsJumping && !StateContext.IsMoving && IsIdle == false && _preventIdleSwap == false)
        {
            SwapState(new Player_Idle(this));
            IsIdle = true;
        }

        _currentState.UpdateMethod();

        UpdateMovePlayer();
    }

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

            float DeltaV = s_rateOfGravity * Time.deltaTime;
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
