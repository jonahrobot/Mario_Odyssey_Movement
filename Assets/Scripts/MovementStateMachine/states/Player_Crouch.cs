using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Crouch : Player_State
{
    // Adjustable Stats
    private float Speed = 5f;

    // Constants
    private float TurnSpeed = 0.1f;

    // References and Trackers
    PlayerStateMachineCore core;

    float turnSmoothVelocity;

    bool walking;
    public Player_Crouch(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void CheckForStateSwap()
    {
        if (core.isPressingSpace)
        {
            core.SwapState(new Player_CrouchJump(core));
        }
        if (!core.isPressingCrouch)
        {
            core.SwapState(new Player_Idle(core));
        }
    }

    public override void ExitMethod()
    {
        core.ChangeAnimationState("Crouch", false);
        core.ChangeAnimationState("CrouchWalk", false);
    }

    public override void StartMethod()
    {
        core.ChangeAnimationState("Crouch", true);
    }

    public override void UpdateMethod()
    {
        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.CameraRotation.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        Direction = SlopeFix(Direction, core.transform.position);

        if (core.movementInput.magnitude > 0.1f)
        {
            core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
            core.MovePlayer(Direction, Speed);

            if (walking == false)
            {
                core.ChangeAnimationState("Crouch", false);
                core.ChangeAnimationState("CrouchWalk", true);
                walking = true;
            }
        }
        else
        {
            if (walking == true)
            {
                core.ChangeAnimationState("CrouchWalk", false);
                core.ChangeAnimationState("Crouch", true);
                walking = false;
            }
        }
    }

    // If player on slope, adjust running speed
    private Vector3 SlopeFix(Vector3 v, Vector3 pos)
    {

        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            var adjustVel = slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill

            /// Adjust for steepness of slopes

            if (adjustVel.y < -0.2f || adjustVel.y > 0.2f) // Going Down Slope, Adjust Direction
            {
                return adjustVel;
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

