using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_GroundPound : Player_State
{

    // Refrences
    private float StartTime;
    private bool HasFallen;

    public Player_GroundPound(PlayerStateMachineCore core):base(core)
    {
    }

    public override void StartMethod()
    {
        StartTime = Time.time;

        AnimationController.ChangeAnimationState("GroundPound", true);
        core.EnableGravity(false);

        StateContext.DisableGroundCheck = false;
    }

    public override void UpdateMethod()
    {
        bool FallDelayOver = Time.time - StartTime > 0.2f;

        if (FallDelayOver && HasFallen == false)
        {
            core.EnableGravity(true);
            HasFallen = true;
        }    
    }

    public override float GetUpdateToGravity()
    {
        if (HasFallen)
        {
            return 12f;
        }
        return 0f;
    }

    public override void ExitMethod()
    {

        Debug.Log("Left GroundPound");
        core.StateMemory.StoreFloat("GroundPoundExitTime", Time.time);

        AnimationController.ChangeAnimationState("GroundPound", false);
        core.EnableGravity(true);
    }

    public override void CheckStateSwaps()
    {
        if (core.CollidingWithHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }

        if (StateContext.IsGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
}