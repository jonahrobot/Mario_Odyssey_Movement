using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Long_Jump : Player_State
{
    PlayerStateMachineCore core;

    // Stats
    private float HorizontalSpeed = 45f;
    private float JumpVelocity = 15f;
    private float FallMultiplier = 2.0f;

    public Player_Long_Jump(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        core.ChangeAnimationState("LongJump", true);
        core.SetVerticalVelocity(JumpVelocity);

        core.DisableGroundCheck = true;
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
            core.DisableGroundCheck = false;

            return FallMultiplier;
        }
        return 0;
    }

    // When Long Jumping, you shoot forward
    private void MoveForwards()
    {
        float TargetAngle = core.transform.eulerAngles.y;

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
        Vector3 AlignedDirection = AlignVectorToSlope(Direction, core.transform.position);

        // Save Direction for post Long Jump Roll
        core.stateMemory.StoreVector3("LongJumpDirection", Direction);

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
        core.ChangeAnimationState("LongJump", false);
    }

    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            if (core.isPressingCrouch)
            {
                core.SwapState(new Player_Rolling(core));
            }
            else
            {
                core.SwapState(new Player_Idle(core));
            }
        }
    }
}