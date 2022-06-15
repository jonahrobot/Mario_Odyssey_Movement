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

    Coroutine Reset;

    public Player_Jumping(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void ExitMethod()
    {
        // Reset Tracker
        AlreadyLetGo = false;
        DoingFinalJump = false;
        Setup = false;
    }

    public override void StartMethod()
    {
        core.DisableGroundCheck = true;
    }

    public override void UpdateMethod()
    {
        var UserInput = core.InputController.Player.Movement.ReadValue<Vector2>().normalized;

        // Triple jumps can't occure if player idle jumps
        if (UserInput.magnitude < 0.1f)
        {
            core.AbleToTripleJump = false;
        }

        // Setup initial jump acceleration 
        if (core.jumpInput == 1f && core.isGrounded && Setup == false)
        {
            if (core.Velocity.y < InitialJumpVelocity)
            {
                // If trying to triple jump but not moving, cancel it
                if (core.JumpCombo == 2 && UserInput.magnitude < 0.1f)
                {
                    core.JumpCombo = 0;
                }

                // If not able to triple jump, just default jump
                if (core.JumpCombo == 2 && core.AbleToTripleJump == false)
                {
                    core.JumpCombo = 0;

                    core.AbleToTripleJump = true;
                }

                /// Handle each stage of jumping
                core.JumpCombo += 1;

                if (core.JumpCombo == 1)
                {
                    core.Head.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", Color.blue);
                    CurrentJumpHeight = 25f;
                }
                if (core.JumpCombo == 2)
                {
                    core.Head.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", Color.magenta);
                    CurrentJumpHeight = 26f;
                }
                if (core.JumpCombo == 3)
                {
                    core.Head.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", Color.red);
                    core.animator.SetBool("jumpAnimation", true);
                    CurrentJumpHeight = 35f;
                }

                core.Velocity.y = CurrentJumpHeight;

                // Finishing setup
                Setup = true;

                if (core.JumpCombo > 2)
                {
                    DoingFinalJump = true;
                    core.JumpCombo = 0;
                }

                if (Reset != null)
                {
                    core.StopCoroutine(Reset);
                }
            }
        }

        // Increase Falling Velocity

        if (((core.jumpInput == 0f && core.Velocity.y > CurrentJumpHeight / 2) || AlreadyLetGo) && !DoingFinalJump)
        {
            // If Jump button was released before jumps apex, short jump occures
            if (core.Velocity.y < CurrentJumpHeight * 0.75f)
            {
                core.VelocityChange *= LetGoMultiplier;

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
            if (core.Velocity.y < CurrentJumpHeight / 2)
            {
                core.VelocityChange *= FallMultiplier;

                if (core.DisableGroundCheck == true)
                {
                    core.DisableGroundCheck = false;
                }
            }
        }
    }
}
