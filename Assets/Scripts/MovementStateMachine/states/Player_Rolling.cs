using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Rolling : Player_State
{
    // Adjustable Stats
    private float BaseSprintSpeed = 35f;
    private float DefaultMaxSprintSpeed = 45f;

    // Constants
    private float SprintAcceleration = 1.00125f;
    private float TurnSpeed = 0.1f;

    // References and Trackers
    PlayerStateMachineCore core;

    float turnSmoothVelocity;
    float CurrentSprintSpeed;
    float MaxSprintSpeed;

    bool GoingUpHill;
    bool OnFlatGround;
    Vector3 CurrentDirection;

    public Player_Rolling(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void CheckForStateSwap()
    {
       //leaf node
    }
    public override void ExitMethod()
    {
        CurrentSprintSpeed = BaseSprintSpeed;
        MaxSprintSpeed = DefaultMaxSprintSpeed;
        GoingUpHill = false;
        core.animator.speed = 1;
        OnFlatGround = false;
        core.animator.SetBool("Roll", false);
    }

    public override void StartMethod()
    {
        foreach (AnimatorControllerParameter parameter in core.animator.parameters)
        {
            core.animator.SetBool(parameter.name, false);
        }
        core.animator.SetBool("Roll", true);

        CurrentDirection = core.LongJumpDirection;
        CurrentSprintSpeed = BaseSprintSpeed;
        MaxSprintSpeed = DefaultMaxSprintSpeed;
        core.animator.speed *= 3;
        GoingUpHill = false;
        OnFlatGround = false;
    }

    public override void UpdateMethod()
    {
        if (core.animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAnimation") == false) { 
            //core.animator.SetBool("jumpAnimation", true);
        }
        core.Model.transform.Rotate(20, 0, 0);
        var reversedDirectonThisFrame = false;
        Vector3 Direction = CurrentDirection;

        float TargetAngle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg;
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);
        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
        //Debug.DrawRay(core.transform.position, Direction, Color.magenta, 2);
        if (GoingUpHill == true)
        {
            CurrentSprintSpeed -= 0.1125f;
            CurrentSprintSpeed = Mathf.Clamp(CurrentSprintSpeed, 0, 100);
            if(CurrentSprintSpeed == 0)
            {
                CurrentDirection = FindDirectionDownHill(core.transform.position);
                //CurrentDirection = new Vector3(CurrentDirection.x, 0f, CurrentDirection.z);
                Debug.Log("Flipped Direction");
                reversedDirectonThisFrame = true;

                CurrentSprintSpeed = 5f;
                GoingUpHill = false;
                
            }
        }
        else
        {
            if (OnFlatGround)
            {
                CurrentSprintSpeed -= 0.0025f;
                CurrentSprintSpeed = Mathf.Clamp(CurrentSprintSpeed, 0, 100);
            }
            else
            {
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
            }
        }
        if (reversedDirectonThisFrame == false)
        {
            Direction = SlopeFix(Direction, core.transform.position);

            core.Character.Move(Direction.normalized * CurrentSprintSpeed * Time.deltaTime);
        }
    }


    // If player on slope, adjust running speed
    private Vector3 SlopeFix(Vector3 v, Vector3 pos)
    {
        //Debug.Log(v);

        var raycast = new Ray(pos, Vector3.down);

        if (Physics.Raycast(raycast, out RaycastHit hitInfo, 200f))
        {
            //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.cyan);

            var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // The direction needed for correction

            var adjustVel = slopeRotation * v; // This rotates the players direction vector to the direction perpendicular to the hill
            
            /// Adjust for steepness of slopes
            /// 
            //Debug.Log("Velocity" + v);
            //Debug.Log("Adjusted " + adjustVel);
            if (adjustVel.y < -0.05f) // Player Running Down Hill
            {
                Debug.Log("Thinks its going down hill!");
                GoingUpHill = false;
                OnFlatGround = false;
                SprintAcceleration = 1.00125f + 0.00125f;
                MaxSprintSpeed = DefaultMaxSprintSpeed + 8f;

                // Don't adjust angle if jumping down hill.
                return adjustVel;
            }
            else if (adjustVel.y > 0.05f) // Player Running Up Hill
            {
                Debug.Log("Thinks its going up hill!");
                GoingUpHill = true;
                OnFlatGround = false;
                return adjustVel;
            }
            else // At Flat Ground
            {
                Debug.Log("Thinks its FLAT GROUND");
                OnFlatGround = true;
                GoingUpHill = false;
                SprintAcceleration = 1.00125f;
                MaxSprintSpeed = DefaultMaxSprintSpeed;
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
            return normalFlat;
            // Find the angle between the normal and the flattened normal
            var offsetAngle = Quaternion.FromToRotation(hitInfo.normal, normalFlat);

            // Rotate our flattend normal by the offset to get our angle down the hill!
            var downHillDirection = offsetAngle * normalFlat;

            //Debug.DrawRay(hitInfo.point, normalFlat, Color.yellow);
           // Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.green);
            //Debug.DrawRay(hitInfo.point, downHillDirection, Color.cyan);

            //return downHillDirection;
        }

        // If no adjustments made, return a empty Vector3
        return Vector3.zero;
    }
}
