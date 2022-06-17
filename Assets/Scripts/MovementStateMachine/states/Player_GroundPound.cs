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

    public override void ExitMethod()
    {
        core.StartCoroutine("PostGroundPoundJump");
        core.GroundPoundFall = false;
        if (SavedRateOfGravity != 0f)
        {
            core.RateOfGravity = SavedRateOfGravity;
            SavedRateOfGravity = 0f;
        }
    }

    public override void StartMethod()
    {
        core.GroundPoundFall = false;
        if (core.RateOfGravity != 0f)
        {
            core.StartCoroutine("GroundPoundDelay");
            SavedRateOfGravity = core.RateOfGravity;
            core.RateOfGravity = 0f;
            core.Velocity.y = 0f;
        }
    }

    public override void UpdateMethod()
    {

        if (core.GroundPoundFall == true)
        {
            if (core.RateOfGravity != SavedRateOfGravity && SavedRateOfGravity != 0f)
            {
                core.RateOfGravity = SavedRateOfGravity;
            }
            core.VelocityChange *= 8f;
        }

        if (core.DisableGroundCheck == true)
        {
            core.DisableGroundCheck = false;
        }
    }
}