using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Crouch : Player_State
{

    // Movement
    private float CurrentSpeed;
    private Vector3 CurrentDirection;
    private float CrouchSpeed = 5f;
    private bool  walking;
    private bool StoppedSlide = false;

    // Refrences
    private float turnSmoothVelocity;
    private float timeSinceCrouch;

    public Player_Crouch(PlayerStateMachineCore core) :base(core)
    {
    }

    public override void StartMethod()
    {
        CurrentDirection = core.StateMemory.GetVector3("CurrentDirection", Vector3.zero);
        CurrentSpeed = core.StateMemory.GetFloat("CurrentSpeed", 5f);
        AnimationController.ChangeAnimationState("Crouch", true);
        timeSinceCrouch = Time.time;
    }

    public override void UpdateMethod()
    {
        Vector3 Direction = GetCurrentDirection(core.MovementInput);
        Direction = AlignVectorToSlope(Direction, core.transform.position);

        if (!walking && StateContext.IsMoving && StoppedSlide)
        {
            walking = true;
            AnimationController.ChangeAnimationState("Crouch", false);
            AnimationController.ChangeAnimationState("CrouchWalk", true);
        }

        if (walking && StateContext.IsMoving == false && StoppedSlide)
        {
            walking = false;
            AnimationController.ChangeAnimationState("Crouch", true);
            AnimationController.ChangeAnimationState("CrouchWalk", false);
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

        if (StateContext.IsMoving && StoppedSlide)
        {
            core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
        }

        return Direction;
    }

    public override void ExitMethod()
    {
        AnimationController.ChangeAnimationState("Crouch", false);
        AnimationController.ChangeAnimationState("CrouchWalk", false);
    }

    public override void CheckStateSwaps()
    {
        bool notCrouchedTooLong = Time.time - timeSinceCrouch < 0.3f;

        if (StateContext.IsJumping && notCrouchedTooLong == false)
        {
            core.SwapState(new Player_CrouchJump(core));
        }

        if (StateContext.IsJumping && StateContext.IsMoving && notCrouchedTooLong)
        {
            core.SwapState(new Player_Long_Jump(core));
        }

        if (!StateContext.IsCrouched)
        {
            core.SwapState(new Player_Idle(core));
        }
    }
}

