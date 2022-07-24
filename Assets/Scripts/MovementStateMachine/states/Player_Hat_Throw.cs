using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Hat_Throw : Player_State
{

    private bool hatThrown;
    private Vector3 defaultDirection;
    private string AnimationName;
    private bool falling;

    private float turnSmoothVelocity;

    public Player_Hat_Throw(PlayerStateMachineCore core):base(core)
    {
    }

    public override void StartMethod()
    {
        defaultDirection = core.StateMemory.GetVector3("CurrentDirection",Vector3.zero);

        AnimationController.ChangeAnimationSpeed(1f);

        if (StateContext.IsGrounded)
        {
            AnimationController.ChangeAnimationState("HatThrow", true);
            AnimationName = "ThrowHat";
        }
        else
        {
            AnimationController.ChangeAnimationState("HatThrowMidair", true);
            AnimationName = "Hat Throw Midair";
        }
        core.GetMarioHatCore().linkToArm();
        core.EnableGravity(false);
        StateContext.HasHat = false;
        core.SetPreventIdleSwap(true);
    }

    public override void UpdateMethod()
    {
        if(hatThrown == false && AnimationController.CheckAnimationProgress(AnimationName, 0.5f))
        {
            Vector3 CurrentDirection;
            if (StateContext.IsMoving)
            {
                float TargetAngle = Mathf.Atan2(core.MovementInput.x, core.MovementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
                float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);
                CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

                core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
            }
            else
            {
                CurrentDirection = defaultDirection;
            }

            core.GetMarioHatCore().startThrow(CurrentDirection);
            hatThrown = true;
        }
    }

    public override void ExitMethod()
    {
        AnimationController.ChangeAnimationState("HatThrow", false);
        AnimationController.ChangeAnimationState("HatThrowMidair", false);
        core.SetPreventIdleSwap(false);
    }
    public override void CheckStateSwaps()
    {
        if (AnimationController.CheckAnimationProgress(AnimationName, 1f))
        {
            falling = true;
            core.EnableGravity(true);
        }

        if (falling && StateContext.IsCrouched && StateContext.IsJumping && StateContext.IsMoving)
        {
            core.SwapState(new Player_Long_Jump(core));
            return;
        }

        if (falling && StateContext.IsGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
}