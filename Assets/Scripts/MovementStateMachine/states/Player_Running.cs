using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;
    Player_Timers data;

    // Movement
    private float CurrentSpeed = 0f;
    private float MaxSpeed = 20f;

    private float Velocity = 0;
    private float SlowVelocity = 0;

    private float Acceleration = 0.5f;
    private float Deceleration = 0.5f / 4f;

    // Refrences
    private float turnSmoothVelocity;
    private float MaxSpeedOriginal;
    private Vector3 Direction;
    private bool AbleToJump;

    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
        data = core.stateMemory;
    }

    public override void StartMethod()
    {
        AbleToJump = data.GetBool("AbleToJump", false);

        MaxSpeedOriginal = MaxSpeed;
        CurrentSpeed = Mathf.Min(data.GetFloat("CurrentSpeed", 0f),MaxSpeed);
        core.speedDebug = CurrentSpeed;

        if (CurrentSpeed < MaxSpeed)
        {
            core.ChangeAnimationSpeed(2);

        }
        core.ChangeAnimationState("Run", true);
    }

    public override void UpdateMethod()
    {
        Vector3 Direction = GetCurrentDirection();
        Vector3 AlignedDirection = AlignVectorToSlope(Direction, core.transform.position);

        SlopeCheck(AlignedDirection);

        if(core.isPressingSpace == false)
        {
            AbleToJump = true;
        }

        if (CurrentSpeed < MaxSpeed)
        {
            Accelerate();
        }
        if (CurrentSpeed > MaxSpeed)
        {
            Decelerate();
        }
        if (CurrentSpeed == MaxSpeed)
        {
            Velocity = 0f;
            SlowVelocity = 0f;
        }

        core.MovePlayer(AlignedDirection, CurrentSpeed);
    }

    private void Accelerate()
    {
        SlowVelocity = 0f;

        Velocity += Acceleration * Time.deltaTime;
        CurrentSpeed = Mathf.Min(CurrentSpeed + Velocity, MaxSpeed);

        core.ChangeAnimationSpeed(2 - CurrentSpeed / MaxSpeed);
        core.speedDebug = CurrentSpeed;
    }
    private void Decelerate()
    {
        Velocity = 0f;

        SlowVelocity += Deceleration * Time.deltaTime;
        CurrentSpeed = Mathf.Max(CurrentSpeed - SlowVelocity, MaxSpeed);

        core.speedDebug = CurrentSpeed;
    }

    private Vector3 GetCurrentDirection()
    {
        

        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        var CurrentDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
        Direction = CurrentDirection;//new Vector3(core.movementInput.x, core.movementInput.y, 0);

        return CurrentDirection;
    }
    private void SlopeCheck(Vector3 CurrentVelocity)
    {
        var Velocity = CurrentVelocity.y;
        bool GoingDownHill = Velocity < -0.2f;
        bool GoingUpHill = Velocity > 0.2f;

        if (GoingDownHill)
        {
            MaxSpeed = MaxSpeedOriginal + 12f;
        }

        if (GoingUpHill)
        {
            MaxSpeed = MaxSpeedOriginal - 4f;
        }

        if (!GoingDownHill && !GoingUpHill)
        {
            MaxSpeed = MaxSpeedOriginal;
        }
    }
    public override float GetUpdateToGravity()
    {
        return 0;
    }

    public override void ExitMethod()
    {
        data.StoreBool("AbleToJump", AbleToJump);
        data.StoreFloat("CurrentSpeed", CurrentSpeed);
        data.StoreVector3("CurrentDirection", Direction);
        core.ChangeAnimationState("Run", false);
    }

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
        if (core.isPressingSpace && AbleToJump == true)
        {
            AbleToJump = false;
            core.SwapState(new Player_Jumping(core));
            return;
        }
    }
}
