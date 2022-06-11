using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Grounded : Player_State
{

    PlayerStateMachineCore core;

    public Player_Grounded(PlayerStateMachineCore core)
    {
        this.core = core;
    }


    public override void ExitMethod()
    {
    }

    public override void StartMethod()
    {
        // Reset Velocity when grounded
        core.Velocity = new Vector3(0f,-2f,0f);
    }

    public override void UpdateMethod()
    {
    }
}
