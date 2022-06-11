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

    public override void ExitMethod()
    {
    }

    public override void StartMethod()
    {
    }

    public override void UpdateMethod()
    {
    }
}
