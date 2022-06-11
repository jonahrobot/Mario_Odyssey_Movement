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
        //Debug.Log("This Triggers twice on jump" + "  " + Time.deltaTime);
        core.Velocity = new Vector3(0f,-2f,0f);
        Debug.Log("THIS IS THE PROBLEM 2");
    }

    public override void UpdateMethod()
    {
        //print("Grounded");
       
    }
}
