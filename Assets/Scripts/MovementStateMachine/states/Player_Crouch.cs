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

    public Player_Crouch(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void ExitMethod()
    {
    }

    public override void StartMethod()
    {
    }

    public override void UpdateMethod()
    {
        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.Camera.eulerAngles.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        if (core.movementInput.magnitude > 0.1f)
        {
            core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
        }

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        Direction = SlopeFix(Direction, core.transform.position);

        if (core.movementInput.magnitude > 0.1f)
        {
            core.Character.Move(Direction.normalized * Speed * Time.deltaTime);
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
                if (core.sB != "ROLL") { return adjustVel; }
            }
        }

        // If no adjustments made, return the old velocity
        return v;
    }
}

