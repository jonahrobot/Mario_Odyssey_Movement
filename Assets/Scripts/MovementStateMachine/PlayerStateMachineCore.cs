using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Data type for each state in player movement state machine.
public struct State
{
    public enum State_Types { IDLE, RUN, GROUND, JUMP, CROUCH, CROUCHJUMP, GROUNDPOUND, LONGJUMP, ROLL }

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
    private State sCrouch;
    private State sCrouchJump;
    private State sGroundPound;
    private State sLongJump;
    private State sRoll;

    // Core State Trackers 
    public State sA; // ( Idle, Running, Crouch, Roll)
    public State sB; // ( Grounded, Jumping, Crouch Jump, Ground Pound, Long Jump )

    // User Inputs
    public Vector2 movementInput;
    public float jumpInput;
    public bool isGrounded;
    public float isCrouching;

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

    // IEnumerator trackers, states depend on Core for IEnumerators
    [HideInInspector] public bool GroundPoundFall = false;
    [HideInInspector] public bool postGroundPoundJumpPossible = false;
    private bool longJumpWindow = false;

    // Private Variables
    float GroundCheckDistance = 0.4f;
    bool holdingJump = false;
    Coroutine reset = null;
    bool delayedGroundPoundFlip = false;



    private void Awake()
    {
        // State Referencing

        sJumping = new State(State.State_Types.JUMP, new Player_Jumping(this));
        sGrounded = new State(State.State_Types.GROUND, new Player_Grounded(this));
        sIdle = new State(State.State_Types.IDLE, new Player_Idle(this));
        sRunning = new State(State.State_Types.RUN, new Player_Running(this));
        sCrouch = new State(State.State_Types.CROUCH, new Player_Crouch(this));
        sCrouchJump = new State(State.State_Types.CROUCHJUMP, new Player_CrouchJump(this));
        sGroundPound = new State(State.State_Types.GROUNDPOUND, new Player_GroundPound(this));
        sLongJump = new State(State.State_Types.LONGJUMP, new Player_Long_Jump(this));
        sRoll = new State(State.State_Types.ROLL, new Player_Rolling(this));

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
        /// Input Tracking
        movementInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;
        jumpInput = InputController.Player.Jump.ReadValue<float>();
        isCrouching = InputController.Player.Crouch.ReadValue<float>();

        isGrounded = Physics.CheckSphere(GroundCheck.position, GroundCheckDistance, GroundMask);


        // holdingJump prevents repeated ghost jumps
        if (jumpInput == 0f) { holdingJump = false; }


        /// State Triggers

        // Jumping (sB)
        if (jumpInput == 1f && sB == "GROUND" && sA != "CROUCH" && holdingJump == false)
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

        /// sB

        // Grounded (sB)
        if (isGrounded && sB != "GROUND" && DisableGroundCheck == false)
        {
            if(sB == "LONGJUMP" && isCrouching == 1f)
            {
                sA = switchState(sA, sRoll);
            }
            reset = StartCoroutine(ResetJumpCount());
            sB = switchState(sB, sGrounded);
            
        }

        // Crouch Jump (sB)
        if (longJumpWindow == false && isCrouching == 1f && jumpInput == 1f && sB == "GROUND" && sA == "CROUCH" && holdingJump == false)
        {
            DisableGroundCheck = true;
            holdingJump = true;
            sB = switchState(sB, sCrouchJump);
        }

        // Ground Pound (sB)
        if (isCrouching == 1f && sB == "JUMP" && sB != "GROUNDPOUND")
        {
            sB = switchState(sB, sGroundPound);
            sA = switchState(sA, sIdle); // Can't move while GroundPounding
        }

        // Long Jump (sB)
        if (longJumpWindow && isCrouching == 1f && jumpInput == 1f && holdingJump == false)
        {
            sB = switchState(sB, sLongJump);
            DisableGroundCheck = true;
            holdingJump = true;
        }

        /// sA

        // Running (sA)
        if (isCrouching == 0f && movementInput.magnitude >= 0.1f && (sA == "IDLE" || sA == "CROUCH" || sA == "ROLL") && sA != "RUN" && sB != "CROUCHJUMP" && sB != "GROUNDPOUND")
        {
            sA = switchState(sA, sRunning);
        }

        // Idle (sA)
        if (isCrouching == 0f && movementInput.magnitude < 0.1f && (sA == "RUN" || sA == "CROUCH" || sA == "ROLL") && sA != "IDLE")
        {
            sA = switchState(sA, sIdle);
        }

        // Crouching (sA)
        if (isCrouching == 1f && sB == "GROUND" && sB != "LONGJUMP" && sA != "ROLL")
        {
            longJumpWindow = true;
            StartCoroutine(LongJumpWindow());
            sA = switchState(sA, sCrouch);
        }





        /// Run Scripts and Adjust Gravity

        VelocityChange = RateOfGravity * Time.deltaTime;

        sA.getScript().UpdateMethod();
        sB.getScript().UpdateMethod();

        // Reset Velocity when grounded, else update velocity!

        if (isGrounded && DisableGroundCheck == false) {
            Velocity.y = -2f;
        } else {
            Velocity.y += VelocityChange;
        }

        Character.Move(Velocity * Time.deltaTime);

        /// Ground Pound Animation Handler
        if (delayedGroundPoundFlip)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAnimation") == false && GroundPoundFall == false)
            {
                delayedGroundPoundFlip = false;
                animator.SetBool("jumpAnimation", true);
            }
        }
    }

    /// Helper Methods

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

    // Delay Ground Pound
    IEnumerator GroundPoundDelay()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAnimation") == false)
        {
            animator.SetBool("jumpAnimation", true);
        }
        else
        {
            delayedGroundPoundFlip = true;
        }
        animator.speed *= 2;
        yield return new WaitForSeconds(0.3f);
        GroundPoundFall = true;
        animator.speed = 1;
    }

    // Zone of time when you can long jump post ground pound
    IEnumerator PostGroundPoundJump()
    {
        postGroundPoundJumpPossible = true;
        yield return new WaitForSeconds(0.2f);
        postGroundPoundJumpPossible = false;
    }

    // In this time, a long jump is possible after crouching
    IEnumerator LongJumpWindow()
    {
        longJumpWindow = true;
        yield return new WaitForSeconds(0.2f);
        longJumpWindow = false;
    }
}
