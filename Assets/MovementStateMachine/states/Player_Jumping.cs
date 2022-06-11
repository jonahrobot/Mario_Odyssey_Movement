using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Jumping : Player_State
{
    PlayerStateMachineCore core;
    private bool HoldingJump; // Jumping
    private bool AlreadyLetGo;  // Jumping
    private float CurrentJumpHeight = 0; // Jumping
    private float InitialJumpVelocity = 25f; // Jumping
    private float FallMultiplier = 2.0f; // Jumping
    private float LetGoMultiplier = 3.0f; // Jumping
    private bool setup;
    private Coroutine Reset;
    private bool doingFinalJump;

    public Player_Jumping(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void ExitMethod()
    {
        //print("JUMP!");
        doingFinalJump = false;
        AlreadyLetGo = false;
        setup = false;
    }

    public override void StartMethod()
    {
        //print("JUMP!");
        core.DisableGroundCheck = true;
        setup = false;

    }

    public override void UpdateMethod()
    {
        var UserInput = core.InputController.Player.Movement.ReadValue<Vector2>().normalized;

        if (UserInput.magnitude < 0.1f)
        {
            core.AbleToTripleJump = false;
        }
        
        if (core.jumpInput == 1f && core.isGrounded && setup == false) //  && HoldingJump == false
        {
            if (core.Velocity.y < InitialJumpVelocity)
            {
                // inital Jump Vertical Velocity Boost, Jump Trigger

                // Base Run Movement

                // If trying to tripple jump but not moving, cancel it
                if (core.JumpCombo == 2 && UserInput.magnitude < 0.1f)
                {
                    core.JumpCombo = 0;
                }

                if(core.JumpCombo == 2 && core.AbleToTripleJump == false)
                {
                    core.JumpCombo = 0;
                    
                    core.AbleToTripleJump = true;
                }
            
                //CurrentJumpHeight = InitialJumpVelocity + 2 * (Mathf.Min(3, core.JumpCombo + 1)); 
                
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
                setup = true;
                Debug.Log("This Triggers!");
                core.Velocity.y = CurrentJumpHeight;

                if (core.JumpCombo > 2)
                {
                    doingFinalJump = true;
                    core.JumpCombo = 0;
                }

                if (Reset != null)
                {
                    StopCoroutine(Reset);
                }
            }
        }

        // Default Velocity Change

        // Increase Falling Velocity
        if (((core.jumpInput == 0f && core.Velocity.y > CurrentJumpHeight / 2) || AlreadyLetGo) && !doingFinalJump)
        {
            // If Jump button was released before jumps apex, short jump occures
            if (core.Velocity.y < CurrentJumpHeight * 0.75f)
            {
                core.VelocityChange *= LetGoMultiplier;
                if (core.DisableGroundCheck == true)
                {
                    Debug.Log("FAILSAFE 1 FIRES WAY TO EARLY!");
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
                    Debug.Log("FAILSAFE 2 FIRES WAY TO EARLY!  " + CurrentJumpHeight +  "  " + core.Velocity.y);
                    core.DisableGroundCheck = false;
                }
            }
        }
    }


}
