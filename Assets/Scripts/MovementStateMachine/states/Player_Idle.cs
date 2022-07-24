using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Idle : Player_State
{
    // Constants
    private float startTime;
    private bool AbleToJump;

    public Player_Idle(PlayerStateMachineCore core) : base(core)
    {
    }
    public override void StartMethod()
    {
        startTime = Time.time;
        AbleToJump = core.StateMemory.GetBool("AbleToJump", false);
    }

    public override void UpdateMethod()
    {
        if(StateContext.IsJumping == false)
        {
            AbleToJump = true;
        }

        if(Time.time - startTime > 0.2f)
        {
            core.StateMemory.StoreFloat("CurrentSpeed", 0f);
            //core.stateMemory.StoreVector3("CurrentDirection", Vector3.zero);
        }
    }

    public override void ExitMethod()
    {
        core.StateMemory.StoreBool("AbleToJump", AbleToJump);
        core.IsIdle = false;
    }
    public override void CheckStateSwaps()
    {
        if (StateContext.IsJumping && AbleToJump == true)
        {
            AbleToJump = false;
            core.SwapState(new Player_Jumping(core));
            return;
        }
        if (StateContext.IsMoving)
        {
            core.SwapState(new Player_Running(core));
            return;
        }
        if (StateContext.IsCrouched)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
        if (core.HasClicked && StateContext.HasHat)
        {
            core.SwapState(new Player_Hat_Throw(core));
            return;
        }
    }
}