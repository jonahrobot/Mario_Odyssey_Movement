using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;
    float turnSmoothVelocity;

    private float BaseSprintSpeed = 15f;
    private float MaxSprintSpeed = 25f;  
    private float MaxSprintSpeedOriginal = 25f;
    private float SprintAcceleration = 1.00125f; 
    private float CurrentSprintSpeed;
    private float TurnSpeed = 0.1f;



    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void ExitMethod()
    {
        //print("Stopped Running");
        CurrentSprintSpeed = BaseSprintSpeed;
    }

    public override void StartMethod()
    {
        //print("Running");
        CurrentSprintSpeed = BaseSprintSpeed;
    }

    public override void UpdateMethod()
    {
        Debug.Log(CurrentSprintSpeed);
        float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.Camera.eulerAngles.y;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        // Removed slow down thing
        /*
                    // Set Angles into range of 0-360

                    var TargetRefined = TargetAngle % 360;
                    if (TargetRefined < 0) { TargetRefined += 360; }

                    var CurrentRefined = CurrentAngle % 360;
                    if (CurrentRefined < 0) { CurrentRefined += 360; }

                    var DifferenceChange = 180;
                    if (CurrentSprintSpeed < MaxSprintSpeed * 0.75)
                    {
                        DifferenceChange = 20;
                    }

                    // Slow Player on Quick Direction Switch

                    if (Mathf.Abs(TargetRefined - CurrentRefined) > DifferenceChange)
                    {
                    CurrentSprintSpeed = Mathf.Max(CurrentSprintSpeed * 0.5f, BaseSprintSpeed);
                    }
        */
       
        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        // Accelerate player
        if (CurrentSprintSpeed < MaxSprintSpeed)
        {
            CurrentSprintSpeed *= SprintAcceleration;
        }
        else
        {
            CurrentSprintSpeed /= SprintAcceleration;
        }

        Direction = SlopeFix(Direction, core.transform.position);

        core.Character.Move(Direction.normalized * CurrentSprintSpeed * Time.deltaTime);
    }

    private Vector3 SlopeFix(Vector3 v, Vector3 pos)
    {
        var raycast = new Ray(pos, Vector3.down);
   
        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            // Get normal
            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            var adjustVel = slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill

            Debug.DrawLine(hitInfo.point + hitInfo.normal, hitInfo.point, Color.cyan);

            if (adjustVel.y < -0.2f)
            {
                Debug.Log("Running Down Hill");

                SprintAcceleration = 1.00125f + 0.00125f;
                MaxSprintSpeed = MaxSprintSpeedOriginal + 8f;

                // Dont apply adjustment if your jumping down the hill. When jumping up hill you need the adjustment!
                if (core.sB != "JUMP")
                {
                    return adjustVel;
                }
            }
            else
            {
                if (adjustVel.y > 0.2f)
                {
                    Debug.Log("Running UP THE HILL");
                    MaxSprintSpeed = MaxSprintSpeedOriginal - 4f;

                    return adjustVel;
                }
                else
                {
                    Debug.Log("At flat ground");

                    SprintAcceleration = 1.00125f;
                    MaxSprintSpeed = MaxSprintSpeedOriginal;

                }
            }
        }
        return v;

    }
}
