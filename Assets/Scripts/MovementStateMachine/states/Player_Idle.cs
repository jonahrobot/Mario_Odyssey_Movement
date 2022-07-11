using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Idle : Player_State
{
    PlayerStateMachineCore core;

    public Player_Idle(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void StartMethod()
    {
    }
    public override void UpdateMethod()
    {
    }
    public override float GetUpdateToGravity()
    {
        return 0;
    }
    public override void ExitMethod()
    {
    }
    public override void CheckForStateSwap()
    {
        if (core.isPressingSpace)
        {
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