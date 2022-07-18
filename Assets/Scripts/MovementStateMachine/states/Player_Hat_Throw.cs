using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Hat_Throw : Player_State
{
    PlayerStateMachineCore core;

    public Player_Hat_Throw(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        core.ChangeAnimationSpeed(1f);
        if (core.isGrounded)
        {
            core.ChangeAnimationState("HatThrow", true);
        }
        else
        {
            core.ChangeAnimationState("HatThrowMidair", true);
        }
        core.EnableGravity(false);
        //core.setHasHat(false);
        core.setPreventIdleSwap(true);
    }

    public override void UpdateMethod()
    {
    }

    public override float GetUpdateToGravity()
    {
        return 0f;
    }

    public override void ExitMethod()
    {
        core.EnableGravity(true);
        core.ChangeAnimationState("HatThrow", false);
        core.ChangeAnimationState("HatThrowMidair", false);
        core.setPreventIdleSwap(false);
    }
    public override void CheckForStateSwap()
    {
        if (core.isGrounded && core.CheckIfAnimationsOver("ThrowHat"))
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
        if (!core.isGrounded && core.CheckIfAnimationsOver("Hat Throw Midair"))
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
}