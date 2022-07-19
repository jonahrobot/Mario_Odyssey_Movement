using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Hat_Throw : Player_State
{
    PlayerStateMachineCore core;

    private bool hatThrown;
    private Vector3 defaultDirection;
    private string AnimationName;
    private bool falling;

    private float turnSmoothVelocity;

    public Player_Hat_Throw(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        defaultDirection = core.stateMemory.GetVector3("CurrentDirection",Vector3.zero);

        core.ChangeAnimationSpeed(1f);

        if (core.isGrounded)
        {
            core.ChangeAnimationState("HatThrow", true);
            AnimationName = "ThrowHat";
        }
        else
        {
            core.ChangeAnimationState("HatThrowMidair", true);
            AnimationName = "Hat Throw Midair";
        }
        core.getMarioHatCore().linkToArm();
        core.EnableGravity(false);
        core.setHasHat(false);
        core.setPreventIdleSwap(true);
    }

    public override void UpdateMethod()
    {
        if(hatThrown == false && core.CheckAnimationProgress(AnimationName, 0.5f))
        {
            Vector3 CurrentDirection;
            if (core.isPressingWSAD)
            {
                float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
                float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);
                CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

                core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
            }
            else
            {
                CurrentDirection = defaultDirection;
            }

            core.getMarioHatCore().startThrow(CurrentDirection);
            hatThrown = true;
        }
    }

    public override float GetUpdateToGravity()
    {
        return 0f;
    }

    public override void ExitMethod()
    {
        core.ChangeAnimationState("HatThrow", false);
        core.ChangeAnimationState("HatThrowMidair", false);
        core.setPreventIdleSwap(false);
    }
    public override void CheckForStateSwap()
    {
        if (core.CheckAnimationProgress(AnimationName, 1f))
        {
            falling = true;
            core.EnableGravity(true);
        }

        if (falling && core.isPressingCrouch && core.isPressingSpace && core.isPressingWSAD)
        {
            core.SwapState(new Player_Long_Jump(core));
            return;
        }

        if (falling && core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
}