using UnityEngine;

public class Player_Jumping : Player_State
{
    PlayerStateMachineCore core;

    // Movement
    private float CurrentJumpHeight = 0;
    private float GravityOnFall = 2.0f;
    private float GravityOnShortFall = 3.0f;

    // States of Jump
    private bool ReleasedJumpEarly = false;
    private bool MaxJump = false;

    // Constants
    private float CurrentJumpState;
    private bool StoppedMovingDuringJump;
    private bool LeftGround;

    // Refrences
    private Player_Timers data;

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

        StoppedMovingDuringJump = data.GetBool("StoppedMovingDuringJump", false);
    }

    private void UpdateCurrentJumpState()
    {
        // Check for a Ground Pound Jump
        var lastGroundPoundTime = data.GetFloat("GroundPoundExitTime", 0);
        var secondsSinceGroundPound = Time.time - lastGroundPoundTime;

        if (secondsSinceGroundPound < 0.3f)
        {
            CurrentJumpState = 2;
        }

        // Check for a Jump Chain Break
        var lastJumpTime = data.GetFloat("TimeSinceLastJump", 0);
        var secondsBetweenJumps = Time.time - lastJumpTime;

        if (secondsBetweenJumps > 0.3f)
        {
            CurrentJumpState = 0;
        }

        // Check for a Idle Triple Jump
        var idleBeforeTripleJump = CurrentJumpState == 2 && StoppedMovingDuringJump;

        if (idleBeforeTripleJump)
        {
            CurrentJumpState = 0;
            data.StoreBool("StoppedMovingDuringJump", false);
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
                CurrentJumpHeight = 25f;
                core.ChangeAnimationState("Jump_1", true);
                break;
            case 1:
                CurrentJumpHeight = 26f;
                core.ChangeAnimationState("Jump_2", true);
                break;
            case 2:
                CurrentJumpHeight = 35f;
                MaxJump = true;
                break;
        }

        // Update for next Jump
        CurrentJumpState += 1;
        data.StoreFloat("CurrentJumpState", CurrentJumpState);

        core.SetVerticalVelocity(CurrentJumpHeight);
    }

    public override void UpdateMethod()
    {
        if (core.isPressingWSAD == false)
        {
            data.StoreBool("StoppedMovingDuringJump", true);
        }
        if (core.isGrounded == false)
        {
            LeftGround = true;
        }
    }

    public override float GetUpdateToGravity()
    {
        bool JumpReachedApex = core.GetVelocity().y < CurrentJumpHeight / 2;
        bool JumpReachedShortApex = core.GetVelocity().y < CurrentJumpHeight * 0.75f;

        if (JumpReachedApex == false && core.isPressingSpace == false)
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

        core.DisableGroundCheck = false;
    }

    /// Helper Methods

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }
        if (core.isGrounded && LeftGround)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }

}
