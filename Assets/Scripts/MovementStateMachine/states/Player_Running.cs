using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;

    // Movement
    private float CurrentSpeed = 15f;
    private float MaxSpeed = 25f;
    private float SprintAcceleration = 1.00125f;

    // Reference Var
    float turnSmoothVelocity;
    float MaxSpeedOriginal;

    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        MaxSpeedOriginal = MaxSpeed;
        core.ChangeAnimationState("Run", true);
    }

    public override void UpdateMethod()
    {
        if (CurrentSpeed < MaxSpeed)
        {
            CurrentSpeed *= SprintAcceleration;
        }
        else
        {
            CurrentSpeed /= SprintAcceleration;
        }

        var Direction = GetCurrentDirection();

        Direction = AlignVectorToTerrainSlope(Direction, core.transform.position);

        UpdateSpeedBasedOnVelocity(Direction);

        core.MovePlayer(Direction, CurrentSpeed);
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

    /// Helper Methods

    private Vector3 AlignVectorToTerrainSlope(Vector3 VectorToAlign, Vector3 currentPosition)
    {
        var raycast = new Ray(currentPosition, Vector3.down);
        var raycastFoundGround = Physics.Raycast(raycast, out RaycastHit hitInfo, 200f);

        if (raycastFoundGround == false)
        {
            return VectorToAlign;
        }

        var AngleOfCorrection = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

        return AngleOfCorrection * VectorToAlign;
    }
}
