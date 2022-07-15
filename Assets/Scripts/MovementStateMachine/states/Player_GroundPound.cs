using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_GroundPound : Player_State
{
    PlayerStateMachineCore core;

    // Refrences
    private float StartTime;
    private bool HasFallen;

    public Player_GroundPound(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        StartTime = Time.time;

        core.ChangeAnimationState("GroundPound", true);
        core.EnableGravity(false);

        core.DisableGroundCheck = false;
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
        core.stateMemory.StoreFloat("GroundPoundExitTime", Time.time);

        core.ChangeAnimationState("GroundPound", false);
        core.EnableGravity(true);
    }

    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
}