using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Rolling : Player_State
{
    PlayerStateMachineCore core;

    // Stats
    private float CurrentJumpHeight = 0;
    private float JumpVelocity = 15f;
    private float FallMultiplier = 2.0f;

    // Constants
    private float Speed = 45f;
    private float TurnSpeed = 0.1f;

    // Trackers 
    bool Setup;
    bool reversing = false;
    float turnSmoothVelocity;

    public Player_Rolling(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void ExitMethod()
    {
        // Reset Tracker
        Setup = false;
        Speed = 45f;
        reversing = false;
    }

    public override void StartMethod()
    {
        core.DisableGroundCheck = true;
        Speed = 45f;
        reversing = false;
    }

    public override void UpdateMethod()
    {

        MoveForwards();

    }

    // When Long Jumping, you shoot forward
    private void MoveForwards()
    {
        float TargetAngle = core.transform.eulerAngles.y;

        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        if (reversing) {
            if (Speed < 45f)
            {
                Speed += 0.125f;
            }
        }

        if (Speed > 0f && reversing == false)
        {
            Speed -= 0.125f;
        }
        else
        {
            reversing = true;
        }

        if (reversing == false)
        {
            Direction = SlopeFix(Direction, core.transform.position);
        }
        else
        {
            Direction = FindDirectionDownHill(core.transform.position);
        }
     
        core.Character.Move(Direction.normalized * Speed * Time.deltaTime);

    }

    // If player on slope, adjust running angle
    private Vector3 SlopeFix(Vector3 v, Vector3 pos)
    {

        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            var adjustVel = slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill

            /// Adjust for steepness of slopes

            if (adjustVel.y > 0.2f) // Going Down Slope, Adjust Direction
            {
                return adjustVel;
            }
        }

        // If no adjustments made, return the old velocity
        return v;
    }


    // Find and return the Vector3 that represents the slope down the hill relative from a given position
    private Vector3 FindDirectionDownHill(Vector3 pos)
    {

        // Only works if on terrain
        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            // To start get the normal of the raycast without the y
            Vector3 normalFlat = new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z);

            // Find the angle between the normal and the flattened normal
            var offsetAngle = Quaternion.FromToRotation(hitInfo.normal, normalFlat);

            // Rotate our flattend normal by the offset to get our angle down the hill!
            var downHillDirection = offsetAngle * normalFlat;

            Debug.DrawRay(hitInfo.point, normalFlat, Color.yellow);
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.green);
            Debug.DrawRay(hitInfo.point, downHillDirection, Color.cyan);

            return downHillDirection;
        }

        // If no adjustments made, return a empty Vector3
        return Vector3.zero;
    }
}
