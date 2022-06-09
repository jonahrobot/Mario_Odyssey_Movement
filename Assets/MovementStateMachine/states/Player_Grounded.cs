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
        //print("Grounded");
    }

    public override void StartMethod()
    {
        //print("Grounded");
        core.Velocity.y = -2f;
    }

    public override void UpdateMethod()
    {
        //print("Grounded");
    }
}
