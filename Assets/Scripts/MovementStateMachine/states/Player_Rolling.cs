using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Rolling : Player_State
{


    // Movement
    private float CurrentSpeed = 40f;
    private float MaxSpeed = 50f;
    private float SprintAcceleration = 1.0025f;
    private float SlowDownStart = 0.0225f;

    // Refrences
    float turnSmoothVelocity;
    Vector3 CurrentDirection;
    Vector3 TargetDirection;
    bool stopRoll;

    public Player_Rolling(PlayerStateMachineCore core) :base(core)
    {
             CurrentSpeed = core.CurrentSpeed;
             MaxSpeed = core.MaxRollSpeed;
             SprintAcceleration = core.SprintAcceleration;
             SlowDownStart = core.SlowDownStart;
    }

    public override void StartMethod()
    {
        AnimationController.ChangeAnimationState("Roll", true);

        CurrentDirection = core.StateMemory.GetVector3("LongJumpDirection", Vector3.zero);
        TargetDirection = CurrentDirection;
    }

    public override void UpdateMethod()
    {
        UpdateDirectionAndSpeed(CurrentDirection);

        UpdateRotation();

        // Slope Adjustment
        Vector3 AlignedDirection = CurrentDirection;//AlignVectorToSlope(CurrentDirection, core.transform.position);

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
        // Slowly Redirect to new Direction
        CurrentDirection = Vector3.Lerp(CurrentDirection, TargetDirection, 0.01f);
 
        var Velocity = AlignVectorToSlope(CurrentDirection, core.transform.position).y;
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
            CurrentSpeed = Mathf.Clamp(CurrentSpeed - SlowDownStart, 0, 100);

            if (CurrentSpeed < 5)
            {
                SlowDownStart = 0.2125f;
                TargetDirection = FindDirectionDownHill(core.transform.position);
                
                CurrentSpeed = 5f;
            }
        }

        bool NotOnSlope = !GoingDownHill && !GoingUpHill;

        if (NotOnSlope)
        {
            CurrentSpeed = Mathf.Clamp(CurrentSpeed - SlowDownStart, 0, 100);

            if (CurrentSpeed < 5)
            {
                stopRoll = true;
            }
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
        AnimationController.ChangeAnimationState("Roll", false);
    }

    public override void CheckStateSwaps()
    {
        if (core.CollidingWithHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }
        if ((stopRoll || StateContext.IsCrouched == false) && StateContext.IsMoving)
        {
            core.SwapState(new Player_Running(core));
            return;
        }
        if ((stopRoll || StateContext.IsCrouched == false) && !StateContext.IsMoving)
        {
            core.SwapState(new Player_Idle(core));
            return;
        }
        if (StateContext.IsJumping)
        {
            core.SwapState(new Player_Long_Jump(core));
            return;
        }
    }
}