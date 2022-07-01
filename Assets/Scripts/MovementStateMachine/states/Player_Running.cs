using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    // Adjustable Stats
    private float BaseSprintSpeed = 15f;
    private float DefaultMaxSprintSpeed = 25f;

    // Constants
    private float SprintAcceleration = 1.00125f;
    private float TurnSpeed = 0.1f;

    // References and Trackers
    PlayerStateMachineCore core;

    float turnSmoothVelocity;
    float CurrentSprintSpeed;
    float MaxSprintSpeed;

    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch)
        {
            core.SwapState(new Player_Crouch(core));
        }
        if (core.isPressingSpace)
        {
            core.SwapState(new Player_Jumping(core));
        }
    }
    public override void ExitMethod()
    {
        CurrentSprintSpeed = BaseSprintSpeed;
        MaxSprintSpeed = DefaultMaxSprintSpeed;

        core.ChangeAnimationState("Run", false);
    }

    public override void StartMethod()
    {
        CurrentSprintSpeed = BaseSprintSpeed;
        MaxSprintSpeed = DefaultMaxSprintSpeed;

        core.ChangeAnimationState("Run", true);

    }

    public override void UpdateMethod()
    {

        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        // Accelerate player
        if (CurrentSprintSpeed < MaxSprintSpeed)
        {
            CurrentSprintSpeed *= SprintAcceleration;
        }
        else
        {
            // If current speed higher than max, slow player down!
            CurrentSprintSpeed /= SprintAcceleration;
        }

        Direction = SlopeFix(Direction, core.transform.position);

        core.MovePlayer(Direction, CurrentSprintSpeed);
    }

    // If player on slope, adjust running speed
    private Vector3 SlopeFix(Vector3 v, Vector3 pos)
    {
        //Debug.Log(v);

        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.cyan);

            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            var adjustVel = slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill

            /// Adjust for steepness of slopes

            if (adjustVel.y < -0.2f) // Player Running Down Hill
            {
                SprintAcceleration = 1.00125f + 0.00125f;
                MaxSprintSpeed = DefaultMaxSprintSpeed + 8f;

                // Don't adjust angle if jumping down hill.
                return adjustVel;
            }
            else if (adjustVel.y > 0.2f) // Player Running Up Hill
            {
                MaxSprintSpeed = DefaultMaxSprintSpeed - 4f;
                return adjustVel;
            }
            else // At Flat Ground
            {
                SprintAcceleration = 1.00125f;
                MaxSprintSpeed = DefaultMaxSprintSpeed;
            }
        }

        // If no adjustments made, return the old velocity
        return v;
    }
    public override float GetUpdateToGravity()
    {
        return 0;
    }
}
