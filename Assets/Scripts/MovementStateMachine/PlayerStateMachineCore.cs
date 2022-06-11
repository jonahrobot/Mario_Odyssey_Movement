using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Data type for each state in player movement state machine.
struct State
{
    public enum State_Types { IDLE, RUN, GROUND, JUMP }

    State_Types currentStateTag;
    Player_State scriptReference;

    public State(State_Types tag, Player_State scriptRef)
    {
        currentStateTag = tag;
        scriptReference = scriptRef;
    }

    public static bool operator ==(State c1, string c2)
    {
        return c1.currentStateTag.ToString() == c2;
    }

    public static bool operator !=(State c1, string c2)
    {
        return c1.currentStateTag.ToString() != c2;
    }

    public Player_State getScript()
    {
        return scriptReference;
    }

    public string getStateAsString()
    {
        return currentStateTag.ToString();
    }
}


public class PlayerStateMachineCore : MonoBehaviour
{
    // States
    private State sJumping;
    private State sGrounded;
    private State sIdle;
    private State sRunning;

    // Core State Trackers 
    private State sA; // ( Idle, Running )
    private State sB; // ( Grounded, Jumping )

    // User Inputs
    public Vector2 movementInput;
    public float jumpInput;
    public bool isGrounded;

    // Component Referencing
    public Transform Camera;
    public Transform GroundCheck;
    public LayerMask GroundMask;
    public GameObject Head;
    public Animator animator;

    [HideInInspector] public InputMaster InputController;
    [HideInInspector] public CharacterController Character;

    // Shared Variables 
    [HideInInspector] public float VelocityChange;
    [HideInInspector] public float RateOfGravity = -50f;
    [HideInInspector] public Vector3 Velocity;

    [HideInInspector] public bool DisableGroundCheck;

    // Variables saved between jumps
    [HideInInspector] public int JumpCombo = 1;
    [HideInInspector] public bool AbleToTripleJump = true;


    // Private Variables
    float GroundCheckDistance = 0.4f;
    bool holdingJump = false;
    Coroutine reset = null;


    private void Awake()
    {
        // State Referencing

        sJumping = new State(State.State_Types.JUMP, new Player_Jumping(this));
        sGrounded = new State(State.State_Types.GROUND, new Player_Grounded(this));
        sIdle = new State(State.State_Types.IDLE, new Player_Idle(this));
        sRunning = new State(State.State_Types.RUN, new Player_Running(this));

        sA = sIdle;
        sB = sGrounded;

        // Component Referencing

        Character = GetComponent<CharacterController>();
        InputController = new InputMaster();
        InputController.Enable();

        animator = GetComponentInChildren<Animator>();

        // Initial Setup
        Velocity = new Vector3(0f, -2f, 0f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        movementInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;
        jumpInput = InputController.Player.Jump.ReadValue<float>();
        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);

        // holdingJump prevents repeated ghost jumps
        if (jumpInput == 0f) { holdingJump = false; }


        /// State Triggers

        // Jumping (sB)
        if (jumpInput == 1f && sB == "GROUND" && holdingJump == false)
        {
            if (reset != null)
            {
                StopCoroutine(reset);
                reset = null;
            }

            sB = switchState(sB, sJumping);
            DisableGroundCheck = true;
            holdingJump = true;
        }

        // Grounded (sB)
        if (isGrounded && sB != "GROUND" && DisableGroundCheck == false)
        {
            reset = StartCoroutine(ResetJumpCount());
            sB = switchState(sB, sGrounded);
        }

        // Running (sA)
        if (movementInput.magnitude >= 0.1f && (sA == "IDLE") && sA != "RUN")
        {
            sA = switchState(sA, sRunning);
        }

        // Idle (sA)
        if (movementInput.magnitude < 0.1f && sA == "RUN" && sA != "IDLE")
        {
            sA = switchState(sA, sIdle);
        }

        // Run Scripts and Adjust Gravity

        VelocityChange = RateOfGravity * Time.deltaTime;

        sA.getScript().UpdateMethod();
        sB.getScript().UpdateMethod();

        // Reset Velocity when grounded, else update velocity!

        if (isGrounded && DisableGroundCheck == false){
            Velocity.y = -2f;
        }else{
            Velocity.y += VelocityChange;
        }

        Character.Move(Velocity * Time.deltaTime);
    }


    // Swap States
    private State switchState(State c, State newState)
    {
        c.getScript().ExitMethod();
        newState.getScript().StartMethod();
        return newState;
    }

    // Reset Jumping Combo Tracker
    IEnumerator ResetJumpCount()
    {
        yield return new WaitForSeconds(0.2f);
        JumpCombo = 0;
        Head.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", Color.white);
    }
}
