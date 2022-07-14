using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Idle : Player_State
{
    PlayerStateMachineCore core;

    // Constants
    private float startTime;
    private bool AbleToJump;

    public Player_Idle(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void StartMethod()
    {
        startTime = Time.time;
        AbleToJump = core.stateMemory.GetBool("AbleToJump", false);
    }

    public override void UpdateMethod()
    {
        if(core.isPressingSpace == false)
        {
            AbleToJump = true;
        }

        if(Time.time - startTime > 0.2f)
        {
            core.stateMemory.StoreFloat("CurrentSpeed", 0f);
            //core.stateMemory.StoreVector3("CurrentDirection", Vector3.zero);
        }
    }
    public override float GetUpdateToGravity()
    {
        return 0;
    }
    public override void ExitMethod()
    {
        core.stateMemory.StoreBool("AbleToJump", AbleToJump);
        core.isIdle = false;
    }
    public override void CheckForStateSwap()
    {
        if (core.isPressingSpace && AbleToJump == true)
        {
            AbleToJump = false;
            core.SwapState(new Player_Jumping(core));
            return;
        }
        if (core.isPressingWSAD)
        {
            core.SwapState(new Player_Running(core));
            return;
        }
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
    }
}