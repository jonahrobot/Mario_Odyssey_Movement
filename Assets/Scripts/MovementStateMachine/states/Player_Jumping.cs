using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Jumping : Player_State
{
    PlayerStateMachineCore core;

    // Stats
    private float CurrentJumpHeight = 0;
    private float InitialJumpVelocity = 25f;
    private float FallMultiplier = 2.0f;
    private float LetGoMultiplier = 3.0f;

    // Trackers 
    bool AlreadyLetGo;
    bool Setup;
    bool DoingFinalJump;
    bool FailedGroundPoundCheck;
    float jumpGroundCheckTimeDelay = 0.3f;

    bool JumpCoolDownOver;

    //Coroutine Reset;

    public Player_Jumping(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_GroundPound(core));
        }
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
        }
    }

    public override void ExitMethod()
    {
        // Reset Tracker
        AlreadyLetGo = false;
        DoingFinalJump = false;
        FailedGroundPoundCheck = false;
        Setup = false;
        core.ChangeAnimationState("Jump_1", false);
        core.ChangeAnimationState("Jump_2", false);

        core.stateMemory.StoreFloat("TimeSinceLastJump", Time.time);
    }

    public override void StartMethod()
    {
        var lastJumpTime = core.stateMemory.GetFloat("TimeSinceLastJump");

        if (lastJumpTime == null)
        {
            JumpCoolDownOver = true;
        }

        if (core.stateMemory.IsVariableStored("JumpCounter") == false)
        {
            core.stateMemory.StoreFloat("JumpCounter", 0);
        }

        if (core.stateMemory.IsVariableStored("AbleToTripleJump") == false)
        {
            core.stateMemory.StoreBool("AbleToTripleJump", true);
        }

        core.DisableGroundCheck = true;


        core.holdingJump = true;

    }



    public override void UpdateMethod()
    {
        var UserInput = core.movementInput;

        // Triple jumps can't occure if player idle jumps
        if (core.isPressingWSAD == false)
        {
            core.stateMemory.StoreBool("AbleToTripleJump", false);
        }

        // Setup initial jump acceleration 
        if (core.isPressingSpace && core.isGrounded && Setup == false)
        {
            if (core.GetVelocity().y < InitialJumpVelocity)
            {
                var jumpCounter = core.stateMemory.GetFloat("JumpCounter");

                if (JumpCoolDownOver == false)
                {
                    var lastJumpTime = core.stateMemory.GetFloat("TimeSinceLastJump");
                    if (Time.time - lastJumpTime > 0.3f)
                    {
                        jumpCounter = 0;
                    }
                    JumpCoolDownOver = true;
                }
            

            // If trying to triple jump but not moving, cancel it
            if (jumpCounter == 2 && core.isPressingWSAD == false)
            {
                jumpCounter = 0;
            }

            // If not able to triple jump, just default jump
            if (jumpCounter == 2 && core.stateMemory.GetBool("AbleToTripleJump") == false)
            {
                jumpCounter = 0;

                core.stateMemory.StoreBool("AbleToTripleJump", true);
            }

            /// Handle each stage of jumping
            jumpCounter += 1;

            if (FailedGroundPoundCheck == false)
            {

                var GroundPoundExitTime = core.stateMemory.GetFloat("GroundPoundExitTime");
                if (GroundPoundExitTime != null)
                {
                    var TimeDiffrence = Time.time - GroundPoundExitTime;
                    if (TimeDiffrence < jumpGroundCheckTimeDelay)
                    {
                        jumpCounter = 3;
                    }
                }
                FailedGroundPoundCheck = true;
            }

            if (jumpCounter == 1)
            {

                CurrentJumpHeight = 25f;
                core.ChangeAnimationState("Jump_1", true);
            }
            if (jumpCounter == 2)
            {

                CurrentJumpHeight = 26f;
                core.ChangeAnimationState("Jump_2", true);
            }
            if (jumpCounter == 3)
            {


                CurrentJumpHeight = 35f;
            }



            core.SetVerticalVelocity(CurrentJumpHeight);

            // Finishing setup
            Setup = true;

            if (jumpCounter > 2)
            {
                DoingFinalJump = true;
                jumpCounter = 0;
            }
            core.stateMemory.StoreFloat("JumpCounter", jumpCounter);

        }
    }

    // Increase Falling Velocity


}

public override float GetUpdateToGravity()
{
    if (((!core.isPressingSpace && core.GetVelocity().y > CurrentJumpHeight / 2) || AlreadyLetGo) && !DoingFinalJump)
    {
        // If Jump button was released before jumps apex, short jump occures
        if (core.GetVelocity().y < CurrentJumpHeight * 0.75f)
        {
            return LetGoMultiplier;

            if (core.DisableGroundCheck == true)
            {
                core.DisableGroundCheck = false;
            }
        }
        AlreadyLetGo = true;
    }
    else
    {
        // If jump has reached apex, fall is faster
        if (core.GetVelocity().y < CurrentJumpHeight / 2)
        {
            return FallMultiplier;

            if (core.DisableGroundCheck == true)
            {
                core.DisableGroundCheck = false;
            }
        }
    }

    return 0;
}

}
