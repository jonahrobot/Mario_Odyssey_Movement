using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;

    // Movement
    private float CurrentSpeed;
    private float MaxSpeed = 25f;
    private float SprintAcceleration = 1.00125f;

    // Refrences
    private float turnSmoothVelocity;
    private float MaxSpeedOriginal;
    private Player_Timers data;

    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
        data = core.stateMemory;
    }

    public override void StartMethod()
    {
        MaxSpeedOriginal = MaxSpeed;
        CurrentSpeed = data.GetFloat("CurrentSpeed", 15f);
        core.ChangeAnimationState("Run", true);
    }

    public override void UpdateMethod()
    {
        Vector3 Direction = GetCurrentDirection();
        Vector3 AlignedDirection = AlignVectorToSlope(Direction, core.transform.position);

        if (CurrentSpeed < MaxSpeed)
        {
            CurrentSpeed *= SprintAcceleration;
        }
        else
        {
            CurrentSpeed /= SprintAcceleration;
        }

        UpdateSpeedBasedOnVelocity(AlignedDirection);

        core.MovePlayer(AlignedDirection, CurrentSpeed);
    }
    private Vector3 GetCurrentDirection()
    {
        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        var CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        return CurrentDirection;
    }
    private void UpdateSpeedBasedOnVelocity(Vector3 CurrentVelocity)
    {
        var Velocity = CurrentVelocity.y;
        bool GoingDownHill = Velocity < -0.2f;
        bool GoingUpHill = Velocity > 0.2f;

        if (GoingDownHill)
        {
            SprintAcceleration = 1.00125f + 0.00125f;
            MaxSpeed = MaxSpeedOriginal + 8f;
        }

        if (GoingUpHill)
        {
            MaxSpeed = MaxSpeedOriginal - 4f;
        }

        if (!GoingDownHill && !GoingUpHill)
        {
            SprintAcceleration = 1.00125f;
            MaxSpeed = MaxSpeedOriginal;
        }
    }
    public override float GetUpdateToGravity()
    {
        return 0;
    }

    public override void ExitMethod()
    {
        data.StoreFloat("CurrentSpeed", CurrentSpeed);
        core.ChangeAnimationState("Run", false);
    }

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
        if (core.isPressingSpace)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }
    }
}
