using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_GroundPound : Player_State
{
    PlayerStateMachineCore core;
    float SavedRateOfGravity = 0f;
    float startTime;
    bool GroundPoundFall;

    public Player_GroundPound(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
    }
    public override void ExitMethod()
    {
        core.ChangeAnimationState("GroundPound", false);

        core.stateMemory.StoreFloat("GroundPoundExitTime", Time.time);
        core.EnableGravity(true);
    }

    public override void StartMethod()
    {
        startTime = Time.time;
        core.ChangeAnimationState("GroundPound", true);

        GroundPoundFall = false;
        core.EnableGravity(false);
        
    }

    public override void UpdateMethod()
    {
        if(Time.time - startTime > 0.2f && GroundPoundFall ==false)
        {
            core.EnableGravity(true);
            GroundPoundFall = true;
        }
        

        if (core.DisableGroundCheck == true)
        {
            core.DisableGroundCheck = false;
        }
    }

    public override float GetUpdateToGravity()
    {
        if (GroundPoundFall == true)
        {
            return 8f;
        }
        return 0f;
    }

}