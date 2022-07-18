using UnityEngine;

public class Player_Jumping : Player_State
{
    PlayerStateMachineCore core;

    // Movement
    private float CurrentJumpHeight = 0;
    private float GravityOnFall = 3.0f;
    private float GravityOnShortFall = 4.0f;

    private float CurrentSpeed;

    // States of Jump
    private bool ReleasedJumpEarly = false;
    private bool MaxJump = false;
    private bool LongJumpDelay = true;
    private float TimeSinceJump = Time.time;

    // Constants
    private float CurrentJumpState;
    private bool StoppedMovingDuringJump;
    private bool LeftGround;

    // Refrences
    private Player_Timers data;
    private float turnSmoothVelocity;
    private Vector2 Direction = Vector2.zero;

    public Player_Jumping(PlayerStateMachineCore core)
    {
        this.core = core;
        data = core.stateMemory;
    }

    public override void StartMethod()
    {
        GetConstants();
        UpdateCurrentJumpState();
        StartInitialJumpAcceleration();

        core.DisableGroundCheck = true;
    }

    private void GetConstants()
    {
        CurrentJumpState = data.GetFloat("CurrentJumpState", 0);

        CurrentSpeed = Mathf.Max(20f,data.GetFloat("CurrentSpeed", 20f) + 5f);

        StoppedMovingDuringJump = data.GetBool("StoppedMovingDuringJump", false);
    }

    private void UpdateCurrentJumpState()
    {
        // Check for a Idle Triple Jump
        var idleBeforeTripleJump = CurrentJumpState == 2 && StoppedMovingDuringJump;

        if (idleBeforeTripleJump)
        {
            CurrentJumpState = 0;
            data.StoreBool("StoppedMovingDuringJump", false);
        }

        // Check for a Ground Pound Jump
        var lastGroundPoundTime = data.GetFloat("GroundPoundExitTime", 0);
        var secondsSinceGroundPound = Time.time - lastGroundPoundTime;

        if (secondsSinceGroundPound < 0.3f)
        {
            CurrentJumpState = 2;
            return;
        }

        // Check for a Jump Chain Break
        var lastJumpTime = data.GetFloat("TimeSinceLastJump", 0);
        var secondsBetweenJumps = Time.time - lastJumpTime;

        if (secondsBetweenJumps > 0.15f)
        {
            CurrentJumpState = 0;
        }

        // Handle Overflow
        if (CurrentJumpState > 2)
        {
            CurrentJumpState = 0;
        }

        data.StoreFloat("CurrentJumpState", CurrentJumpState);
    }

    private void StartInitialJumpAcceleration()
    {
        switch (CurrentJumpState)
        {
            case 0:
                CurrentJumpHeight = 35f;
                core.ChangeAnimationState("Jump_1", true);
                break;
            case 1:
                CurrentJumpHeight = 37f;
                core.ChangeAnimationState("Jump_2", true);
                break;
            case 2:
                CurrentJumpHeight = 43f; 
                break;
        }

        // Update for next Jump
        CurrentJumpState += 1;
        data.StoreFloat("CurrentJumpState", CurrentJumpState);

        core.SetVerticalVelocity(CurrentJumpHeight);
    }

    public override void UpdateMethod()
    {
        // Makes sure user wont trigger grounpound accidentally instead of a long jump
        if(LongJumpDelay == true)
        {
            if(Time.time - TimeSinceJump > 0.1f)
            {
                LongJumpDelay = false;
            }
        }

        MidAirStrafe();

        if (core.isPressingWSAD == false)
        {
            data.StoreBool("StoppedMovingDuringJump", true);
        }

        if(core.isPressingSpace == false)
        {
            data.StoreBool("AbleToJump", true);
        }

        if (core.isGrounded == false)
        {
            LeftGround = true;
        }
    }
    private void MidAirStrafe()
    {
        core.speedDebug = CurrentSpeed;
        if (core.isPressingWSAD == true)
        {
            Direction = core.movementInput;
            core.MovePlayer(GetCurrentDirection(core.movementInput), CurrentSpeed);
        }
        else
        {
            if (Direction != Vector2.zero)
            {
                core.MovePlayer(GetCurrentDirection(Direction), CurrentSpeed / 2);
            }
        }
    }
    private Vector3 GetCurrentDirection(Vector2 Input)
    {
        float TargetAngle = Mathf.Atan2(Input.x, Input.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        var CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        return CurrentDirection;
    }

    public override float GetUpdateToGravity()
    {
        bool JumpReachedApex = core.GetVelocity().y < CurrentJumpHeight / 2;
        bool JumpReachedShortApex = core.GetVelocity().y < CurrentJumpHeight * 0.75f;

        if (JumpReachedApex == false && core.isPressingSpace == false && MaxJump == false)
        {
            ReleasedJumpEarly = true;
        }

        if (JumpReachedShortApex && ReleasedJumpEarly == true && MaxJump == false)
        {
            core.DisableGroundCheck = false;
            return GravityOnShortFall;
        }

        if (JumpReachedApex && ReleasedJumpEarly == false)
        {
            core.DisableGroundCheck = false;
            return GravityOnFall;
        }

        return 0;
    }

    public override void ExitMethod()
    {
        // Stop Animations
        core.ChangeAnimationState("Jump_1", false);
        core.ChangeAnimationState("Jump_2", false);

        // Save Jump Time to keep track of the jump cooldown
        data.StoreFloat("TimeSinceLastJump", Time.time);
        //data.StoreFloat("CurrentSpeed", CurrentSpeed);

        core.DisableGroundCheck = false;
    }

    /// Helper Methods

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch && LongJumpDelay == false)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }
        if (core.isPressingCrouch && LongJumpDelay)
        {
            core.SwapState(new Player_Long_Jump(core));
            return;
        }
        if (core.isGrounded && LeftGround)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
        if (core.hasClicked && core.getHasHat())
        {
            core.SwapState(new Player_Hat_Throw(core));
            return;
        }
    }

}
