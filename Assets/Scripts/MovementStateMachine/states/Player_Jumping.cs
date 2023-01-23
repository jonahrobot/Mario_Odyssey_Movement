using UnityEngine;

public class Player_Jumping : Player_State
{
    // Movement
    private float CurrentJumpHeight = 0;
    private float GravityOnFall;
    private float GravityOnShortFall;
    private float[] JumpHeight;

    private float CurrentSpeed;
    private float MaxSpeed;
    private float baseJumpHorzSpeed;

    // States of Jump
    private bool ReleasedJumpEarly = false;
    private bool MaxJump = false;
    private bool LongJumpDelay = true;
    private float TimeSinceJump = Time.time;

    // Constants
    private float CurrentJumpState;
    private bool StoppedMovingDuringJump;
    private bool LeftGround;

    // RefrencesW
    private float turnSmoothVelocity;
    private Vector2 Direction = Vector2.zero;

    public Player_Jumping(PlayerStateMachineCore core) : base(core)
    {
        GravityOnFall = core.gravityOnFall;
        GravityOnShortFall = core.gravityOnShortFall;
        JumpHeight = core.JumpHeights;
        MaxSpeed = core.MaxSpeed;
        baseJumpHorzSpeed = core.baseJumpHorzSpeed;
    }

    public override void StartMethod()
    {
        GetConstants();
        UpdateCurrentJumpState();
        StartInitialJumpAcceleration();
        StateContext.DisableGroundCheck = true;
    }

    private void GetConstants()
    {
        CurrentJumpState = data.GetFloat("CurrentJumpState", 0);

        CurrentSpeed = data.GetFloat("CurrentSpeed", baseJumpHorzSpeed);
        if (CurrentSpeed < baseJumpHorzSpeed)
        {
            CurrentSpeed = baseJumpHorzSpeed;
        }
        //CurrentSpeed = Mathf.Max(MaxSpeed, data.GetFloat("CurrentSpeed", MaxSpeed) + 2f);

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
                CurrentJumpHeight = JumpHeight[0];
                AnimationController.ChangeAnimationState("Jump_1", true);
                break;
            case 1:
                CurrentJumpHeight = JumpHeight[1];
                AnimationController.ChangeAnimationState("Jump_2", true);
                break;
            case 2:
                CurrentJumpHeight = JumpHeight[2]; 
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

        if (StateContext.IsMoving == false)
        {
            data.StoreBool("StoppedMovingDuringJump", true);
        }

        if(StateContext.IsJumping == false)
        {
            data.StoreBool("AbleToJump", true);
        }

        if (StateContext.IsGrounded == false)
        {
            LeftGround = true;
        }
    }
    private void MidAirStrafe()
    {
        core.SpeedDebug = CurrentSpeed;
        if (StateContext.IsMoving == true)
        {
            Direction = StateContext.MovementInput;
            core.MovePlayer(GetCurrentDirection(StateContext.MovementInput), CurrentSpeed);
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
        float TargetAngle = Mathf.Atan2(Input.x, Input.y) * Mathf.Rad2Deg + StateContext.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        var CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        return CurrentDirection;
    }

    public override float GetUpdateToGravity()
    {
        bool JumpReachedApex = core.GetVelocity().y < CurrentJumpHeight / 2;
        bool JumpReachedShortApex = core.GetVelocity().y < CurrentJumpHeight * 0.75f;

        if (JumpReachedApex == false && StateContext.IsJumping == false && MaxJump == false)
        {
            ReleasedJumpEarly = true;
        }

        if (JumpReachedShortApex && ReleasedJumpEarly == true && MaxJump == false)
        {
            StateContext.DisableGroundCheck = false;
            return GravityOnShortFall;
        }

        if (JumpReachedApex && ReleasedJumpEarly == false)
        {
            StateContext.DisableGroundCheck = false;
            return GravityOnFall;
        }

        return 0;
    }

    public override void ExitMethod()
    {
        // Stop Animations
        AnimationController.ChangeAnimationState("Jump_1", false);
        AnimationController.ChangeAnimationState("Jump_2", false);

        // Save Jump Time to keep track of the jump cooldown
        data.StoreFloat("TimeSinceLastJump", Time.time);
        //data.StoreFloat("CurrentSpeed", CurrentSpeed);

        StateContext.DisableGroundCheck = false;
    }

    /// Helper Methods

    public override void CheckStateSwaps()
    {
        if (StateContext.IsCrouched && LongJumpDelay == false)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }
        if (StateContext.IsCrouched && LongJumpDelay)
        {
            core.SwapState(new Player_Long_Jump(core));
            return;
        }
        if (StateContext.IsGrounded && LeftGround)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
        if (StateContext.HasClicked && StateContext.HasHat)
        {
            core.SwapState(new Player_Hat_Throw(core));
            return;
        }
        if (core.CollidingWithHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }
    }

}
