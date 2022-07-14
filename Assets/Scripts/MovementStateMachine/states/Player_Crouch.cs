using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Crouch : Player_State
{
    private PlayerStateMachineCore core;

    // Movement
    private float CurrentSpeed;
    private Vector3 CurrentDirection;
    private float CrouchSpeed = 5f;
    private bool  walking;
    private bool StoppedSlide = false;

    // Refrences
    private float turnSmoothVelocity;
    private float timeSinceCrouch;

    public Player_Crouch(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        CurrentDirection = core.stateMemory.GetVector3("CurrentDirection", Vector3.zero);
        CurrentSpeed = core.stateMemory.GetFloat("CurrentSpeed", 5f);
        core.ChangeAnimationState("Crouch", true);
        timeSinceCrouch = Time.time;
    }

    public override void UpdateMethod()
    {
        Vector3 Direction = GetCurrentDirection(core.movementInput);
        Direction = AlignVectorToSlope(Direction, core.transform.position);

        if (!walking && core.isPressingWSAD && StoppedSlide)
        {
            walking = true;
            core.ChangeAnimationState("Crouch", false);
            core.ChangeAnimationState("CrouchWalk", true);
        }

        if (walking && core.isPressingWSAD == false && StoppedSlide)
        {
            walking = false;
            core.ChangeAnimationState("Crouch", true);
            core.ChangeAnimationState("CrouchWalk", false);
        }

        if (walking && StoppedSlide)
        {
            core.MovePlayer(Direction, CrouchSpeed);
        }
        
        if(StoppedSlide == false)
        {
            if(CurrentSpeed > CrouchSpeed)
            {
                CurrentSpeed *= 0.99f;
            }
            else
            {
                StoppedSlide = true;
            }
            core.MovePlayer(CurrentDirection, CurrentSpeed);
        }
    }

    private Vector3 GetCurrentDirection(Vector2 input)
    {
        float TargetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        if (core.isPressingWSAD && StoppedSlide)
        {
            core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
        }

        return Direction;
    }

    public override float GetUpdateToGravity()
    {
        return 0;
    }

    public override void ExitMethod()
    {
        core.ChangeAnimationState("Crouch", false);
        core.ChangeAnimationState("CrouchWalk", false);
    }

    public override void CheckForStateSwap()
    {
        bool notCrouchedTooLong = Time.time - timeSinceCrouch < 0.3f;

        if (core.isPressingSpace && notCrouchedTooLong == false)
        {
            core.SwapState(new Player_CrouchJump(core));
        }

        if (core.isPressingSpace && core.isPressingWSAD && notCrouchedTooLong)
        {
            core.SwapState(new Player_Long_Jump(core));
        }

        if (!core.isPressingCrouch)
        {
            core.SwapState(new Player_Idle(core));
        }
    }
}

