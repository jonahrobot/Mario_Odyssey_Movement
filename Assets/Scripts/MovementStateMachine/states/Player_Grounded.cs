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

    public override void CheckForStateSwap()
    {
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
    public override float GetUpdateToGravity()
    {
        return 0;
    }
}
