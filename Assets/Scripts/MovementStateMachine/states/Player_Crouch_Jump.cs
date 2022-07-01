using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_CrouchJump : Player_State
{
    PlayerStateMachineCore core;

    // Stats
    private float CurrentJumpHeight = 0;
    private float JumpVelocity = 32f;
    private float FallMultiplier = 2.0f;

    // Constants
    private float Speed = 5f;
    private float TurnSpeed = 0.1f;

    // Trackers 
    bool Setup;
    float turnSmoothVelocity;
    bool ReleasedCrouch;

    public Player_CrouchJump(PlayerStateMachineCore core)
    {
        this.core = core;
    }

    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
        }
        if (core.isPressingCrouch && ReleasedCrouch)
        {
            core.SwapState(new Player_GroundPound(core));
        }
    }

    public override void ExitMethod()
    {
        // Reset Tracker
        Setup = false;
    }

    public override void StartMethod()
    {
        core.DisableGroundCheck = true;
        core.holdingJump = true;
        core.DisableGroundCheck = true;
    }

    public override void UpdateMethod()
    {
        if (!core.isPressingCrouch)
        {
            ReleasedCrouch = true;
        }
        if (core.movementInput.magnitude <= 0.1f)
        {
            MoveBackwards();
        }
        
        if (Setup == false)
        {
            if (core.GetVelocity().y < JumpVelocity)
            {
                // core.Head.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", Color.red);
                core.ChangeAnimationState("LongJump", true);
                CurrentJumpHeight = 32f;
                core.SetVerticalVelocity(CurrentJumpHeight);

                // Finishing setup
                Setup = true;
            }
        }

        // If jump has reached apex, fall is faster
     
        
    }
    public override float GetUpdateToGravity()
    {
        if (core.GetVelocity().y < CurrentJumpHeight / 2)
        {
            return FallMultiplier;

            if (core.DisableGroundCheck == true)
            {
                core.DisableGroundCheck = false;
            }
        }
        return 0;
    }

    // When Idle Crouch Jumping, you go backwards
    private void MoveBackwards()
    {
        float TargetAngle = core.transform.eulerAngles.y - 180;
       
        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

        Direction = SlopeFix(Direction, core.transform.position);


        core.MovePlayer(Direction,Speed);

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
}
