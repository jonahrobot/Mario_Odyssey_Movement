using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent] should add!
public class PlayerStateMachineCore : MonoBehaviour
{
    // Player Context

    [HideInInspector] public bool DisableGroundCheck;

    // Could be put in each state, don't want to be finding crouch when not actually needed.
    [HideInInspector] public bool isPressingCrouch;
    [HideInInspector] public bool isPressingSpace;
    [HideInInspector] public bool isPressingWSAD;
   
    [HideInInspector] public bool isIdle;
    [HideInInspector] public bool hasClicked;
    [HideInInspector] public bool onHat;

    // Harder to find 
    [HideInInspector] public bool isGrounded;

    [HideInInspector] public Vector2 movementInput;
    [HideInInspector] public Vector3 CameraRotation;

    // Component Refrences 

    private Animator Animator; // Could seperate animation stuff into its own system
    private Transform Camera; // Only used once!
    private Transform GroundCheck;
    private InputMaster InputController;
    private MouseTest ClickController;
    private CharacterController Character;
    private MarioHatCore marioHat;
    [SerializeField] private LayerMask GroundMask;

    // Movement Stats

    private Vector3 Velocity;
    private float RateOfGravity = -50f;
    private float GroundCheckDistance = 0.6f;
    private bool UsingGravity = true;

    // State Info

    public Player_Timers stateMemory;
    private Player_State currentState;
    private bool SwappedThisFrame;
    private bool hasHat = true;
    private bool preventIdleSwap = false;


    [DebugGUIGraph(min: 0, max: 30, r: 1, g: 0, b: 0, autoScale: false)]
    public float speedDebug;

    private void Awake()
    {
        // Get Components
        marioHat = GameObject.Find("MarioHat").GetComponent<MarioHatCore>();

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

        ClickController = new MouseTest();
        ClickController.Enable();

        // Initial Setup
        Velocity = new Vector3(0f, -2f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }


    private void Update()
    {
        SwappedThisFrame = false;

        UpdatePlayerContext();

        currentState.CheckForStateSwap();

        // If no inputs, default state is idle
        if (isGrounded && !isPressingCrouch && !isPressingSpace && !isPressingWSAD && isIdle == false && preventIdleSwap == false)
        {
            SwapState(new Player_Idle(this));
            isIdle = true;
        }
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
        if (DisableGroundCheck == true)
        {
            isGrounded = false;
        }

        isPressingCrouch = InputController.Player.Crouch.ReadValue<float>() == 1f;
        isPressingSpace = InputController.Player.Jump.ReadValue<float>() == 1f;
        isPressingWSAD = movementInput.magnitude >= 0.2f;

        hasClicked = ClickController.Mouse.MouseClick.ReadValue<float>() == 1f;

        CameraRotation = Camera.eulerAngles;
    }

    /// Helper Methods

    public void SwapState(Player_State input)
    {
        if (SwappedThisFrame == false)
        {
            Animator.speed = 1;
            currentState.ExitMethod();
            currentState = input;
            currentState.StartMethod();
            SwappedThisFrame = true;
        }
    }

    public void ChangeAnimationSpeed(float speed)
    {
        Animator.speed = speed;
    }

    public void ChangeAnimationState(string animation, bool newState)
    {
        Animator.SetBool(animation, newState);
    }

    public bool CheckAnimationProgress(string name, float time)
    {
        if (Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == name)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= time && !Animator.IsInTransition(0);
        }
        return false;
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

    public bool getHasHat()
    {
        return hasHat;
    }

    public void setHasHat(bool value)
    {
        hasHat = value;
    }

    public bool getPreventIdleSwap()
    {
        return preventIdleSwap;
    }

    public void setPreventIdleSwap(bool value)
    {
        preventIdleSwap = value;
    }

    public MarioHatCore getMarioHatCore()
    {
        return marioHat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hat" && hasHat == false)
        {
            onHat = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Hat")
        {
            onHat = false;
        }
    }
}
