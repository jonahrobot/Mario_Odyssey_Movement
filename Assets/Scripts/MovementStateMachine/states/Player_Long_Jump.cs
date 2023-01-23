using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Long_Jump : Player_State
{

    // Stats
    private float HorizontalSpeed;
    private float JumpVelocity;
    private float FallMultiplier;

    private float turnSmoothVelocity;

    public Player_Long_Jump(PlayerStateMachineCore core):base(core)
    {
         HorizontalSpeed = core.HorizontalSpeed;
         JumpVelocity = core.LongJumpVelocity;
         FallMultiplier = core.LongFallMultiplier;
    }

    public override void StartMethod()
    {
        AnimationController.ChangeAnimationState("LongJump", true);
        core.SetVerticalVelocity(JumpVelocity);

        StateContext.DisableGroundCheck = true;
    }

    public override void UpdateMethod()
    {
        MoveForwards();
    }

    public override float GetUpdateToGravity()
    {
        bool JumpReachedApex = core.GetVelocity().y < JumpVelocity / 2;

        if (JumpReachedApex)
        {
            StateContext.DisableGroundCheck = false;

            return FallMultiplier;
        }
        return 0;
    }

    // When Long Jumping, you shoot forward
    private void MoveForwards()
    {
        float TargetAngle = Mathf.Atan2(StateContext.MovementInput.x, StateContext.MovementInput.y) * Mathf.Rad2Deg + StateContext.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);


        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
        Vector3 AlignedDirection = AlignVectorToSlope(Direction, core.transform.position);

        // Save Direction for post Long Jump Roll
        core.StateMemory.StoreVector3("LongJumpDirection", Direction);

        bool GoingDownHill = AlignedDirection.y > 0.2f;

        if (GoingDownHill)
        {
            // Only use terrian aligned direction when going down hill
            Direction = AlignedDirection;
        }

        core.MovePlayer(Direction, HorizontalSpeed);
    }

    public override void ExitMethod()
    {
        AnimationController.ChangeAnimationState("LongJump", false);
    }

    public override void CheckStateSwaps()
    {
        if (StateContext.IsGrounded && !core.CollidingWithHat)
        {
            if (StateContext.IsCrouched)
            {
                if (StateContext.IsJumping)
                {
                    core.SwapState(new Player_Long_Jump(core));
                }
                else
                {
                    core.SwapState(new Player_Rolling(core));
                }
            }
            else
            {
                core.SwapState(new Player_Idle(core));
            }
        }

        if (core.CollidingWithHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }
    }
}