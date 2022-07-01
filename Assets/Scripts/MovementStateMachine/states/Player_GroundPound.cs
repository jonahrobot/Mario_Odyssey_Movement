using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_GroundPound : Player_State
{
    PlayerStateMachineCore core;
    float SavedRateOfGravity = 0f;

    public Player_GroundPound(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
        }
    }
    public override void ExitMethod()
    {
        core.ChangeAnimationState("GroundPound", false);

        core.StartCoroutine("PostGroundPoundJump");
        core.GroundPoundFall = false;
        core.EnableGravity(true);
    }

    public override void StartMethod()
    {
        core.ChangeAnimationState("GroundPound", true);

        core.GroundPoundFall = false;

        core.StartCoroutine("GroundPoundDelay");
        core.EnableGravity(false);
        
    }

    public override void UpdateMethod()
    {

        

        if (core.DisableGroundCheck == true)
        {
            core.DisableGroundCheck = false;
        }
    }

    public override float GetUpdateToGravity()
    {
        if (core.GroundPoundFall == true)
        {
            return 8f;
        }
        return 0f;
    }

}