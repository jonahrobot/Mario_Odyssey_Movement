using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Ground_Pound : Player_State
{
    PlayerStateMachineCore core;

    public Player_Ground_Pound(PlayerStateMachineCore core)
    {
        this.core = core;
    }


    public override void ExitMethod()
    {
        //print("GroundPound!");
    }

    public override void StartMethod()
    {
        //print("GroundPound!");
    }

    public override void UpdateMethod()
    {
        //print("GroundPound!");
    }
}
