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
            Direction = SlopeFixRollingBack(Direction, core.transform.position);
        }

        

        core.Character.Move(Direction.normalized * Speed * Time.deltaTime);

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

            if (adjustVel.y > 0.2f) // Going Down Slope, Adjust Direction
            {
                return adjustVel;
            }
        }

        // If no adjustments made, return the old velocity
        return v;
    }


    // If player on slope, adjust running speed
    private Vector3 SlopeFixRollingBack(Vector3 v, Vector3 pos)
    {

        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            Vector3 test = new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z);
            var adjustVel = Vector3.Reflect(test,hitInfo.normal).normalized;

            Debug.DrawRay(hitInfo.point, test, Color.yellow);
            Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.green);
            Debug.DrawRay(hitInfo.point, adjustVel, Color.cyan);

            // Debug.DrawRay(hitInfo.point, adjustVel);
            //slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill

            /// Adjust for steepness of slopes

                return adjustVel;
            
        }

        // If no adjustments made, return the old velocity
        return v;
    }
}
