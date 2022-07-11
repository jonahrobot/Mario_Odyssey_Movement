using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Rolling : Player_State
{
    PlayerStateMachineCore core;

    // Movement
    private float CurrentSpeed = 15f;
    private float MaxSpeed = 33f;
    private float SprintAcceleration = 1.0025f;

    // Refrences
    float turnSmoothVelocity;
    Vector3 CurrentDirection;

    public Player_Rolling(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void StartMethod()
    {
        core.ChangeAnimationState("Roll", true);

        CurrentDirection = core.stateMemory.GetVector3("LongJumpDirection", Vector3.zero);
    }

    public override void UpdateMethod()
    {
        UpdateDirectionAndSpeed(CurrentDirection);

        UpdateRotation();

        // Slope Adjustment
        Vector3 AlignedDirection = AlignVectorToSlope(CurrentDirection, core.transform.position);

        bool OnSlope = AlignedDirection.y < -0.05f || AlignedDirection.y > 0.05f;

        if (OnSlope)
        {
            CurrentDirection = AlignedDirection;
        }

        core.MovePlayer(CurrentDirection, CurrentSpeed);
    }

    private void UpdateRotation()
    {
        float TargetAngle = Mathf.Atan2(CurrentDirection.x, CurrentDirection.z) * Mathf.Rad2Deg;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
    }

    private void UpdateDirectionAndSpeed(Vector3 CurrentVelocity)
    {
        var Velocity = CurrentVelocity.y;
        bool GoingDownHill = Velocity < -0.05f;
        bool GoingUpHill = Velocity > 0.05f;

        if (GoingDownHill)
        {
            // Accelerate player
            if (CurrentSpeed < MaxSpeed)
            {
                CurrentSpeed *= SprintAcceleration;
            }
            else
            {
                CurrentSpeed /= SprintAcceleration;
            }
        }

        if (GoingUpHill)
        {
            CurrentSpeed = Mathf.Clamp(CurrentSpeed - 0.1125f, 0, 100);

            if (CurrentSpeed == 0)
            {
                CurrentDirection = FindDirectionDownHill(core.transform.position);
                CurrentSpeed = 5f;
            }
        }

        bool NotOnSlope = !GoingDownHill && !GoingUpHill;

        if (NotOnSlope)
        {
            CurrentSpeed = Mathf.Clamp(CurrentSpeed - 0.0025f, 0, 100);
        }
    }

    // Find and return the Vector3 that represents the slope down the hill relative from a given position
    private Vector3 FindDirectionDownHill(Vector3 pos)
    {
        var raycast = new Ray(pos, Vector3.down);

        var raycastFoundGround = Physics.Raycast(raycast, out RaycastHit hitInfo, 200f);

        if (raycastFoundGround == false)
        {
            return Vector3.zero;
        }

        Vector3 DirectionDownHill = new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z);

        return DirectionDownHill;
    }

    public override void ExitMethod()
    {
        core.ChangeAnimationState("Roll", false);
    }

    public override void CheckForStateSwap()
    {
    }

    public override float GetUpdateToGravity()
    {
        return 0;
    }
}