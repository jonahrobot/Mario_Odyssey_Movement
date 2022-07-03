using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Long_Jump : Player_State
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
    float turnSmoothVelocity;

    public Player_Long_Jump(PlayerStateMachineCore core)
    {
        this.core = core;
    }
    public override void CheckForStateSwap()
    {
        if (core.isGrounded)
        {
            core.SwapState(new Player_Idle(core));
        }
    }
    public override void ExitMethod()
    {
        // Reset Tracker

        core.ChangeAnimationState("LongJump", false);
        Setup = false;
    }

    public override void StartMethod()
    {

   
        core.ChangeAnimationState("LongJump", true);
        core.DisableGroundCheck = true;
        core.DisableGroundCheck = true;
  
    }

    public override void UpdateMethod()
    {

        MoveForwards();

        if (Setup == false)
        {
            if (core.GetVelocity().y < JumpVelocity)
            {

                CurrentJumpHeight = 15f;

                core.SetVerticalVelocity(CurrentJumpHeight);

         
                Setup = true;
            }
        }

     

    }

    public override float GetUpdateToGravity()
    {
        // If jump has reached apex, fall is faster
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

        // When Long Jumping, you shoot forward
        private void MoveForwards()
    {
        float TargetAngle = core.transform.eulerAngles.y;

        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

        //core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
        

        Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
        core.stateMemory.StoreVector3("LongJumpDirection", Direction);
      
        
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

            if (adjustVel.y > 0.2f) // Going Down Slope, Adjust Direction
            {
                return adjustVel;
            }
        }

        // If no adjustments made, return the old velocity
        return v;
    }
}