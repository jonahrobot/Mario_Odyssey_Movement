using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_CrouchJump : Player_State
{

    // Movement
    private float JumpVelocity = 43f;
    private float FallMultiplier = 4.0f;
    private float Speed = 5f;

    // Refrences
    private bool ReleasedCrouch;

    public Player_CrouchJump(PlayerStateMachineCore core):base(core)
    {
    }

    public override void StartMethod()
    {
        core.DisableGroundCheck = true;

        AnimationController.ChangeAnimationState("LongJump", true);
        core.SetVerticalVelocity(JumpVelocity);
    }

    public override void UpdateMethod()
    {
        // If no overiding movement, continue backflip
        if (StateContext.IsMoving == false)
        {
            MoveBackwards();
        }

        if (!StateContext.IsCrouched)
        {
            ReleasedCrouch = true;
        }
    }

    private void MoveBackwards()
    {
        float TargetAngle = core.transform.eulerAngles.y - 180;

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        Vector3 AlignedDirection = AlignVectorToSlope(Direction, core.transform.position);

        core.MovePlayer(AlignedDirection, Speed);
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
    public override void ExitMethod()
    {
        AnimationController.ChangeAnimationState("LongJump", false);
        core.StateMemory.StoreFloat("CurrentSpeed", 0f);
    }

    public override void CheckStateSwaps()
    {
        if (StateContext.IsCrouched && ReleasedCrouch)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }
        if (StateContext.IsGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }

    }
}
