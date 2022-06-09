using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;
    float turnSmoothVelocity;

    private float BaseSprintSpeed = 15f; // Running
    private float MaxSprintSpeed = 25f;  // Running
    private float SprintAcceleration = 1.00125f; // Running
    private float CurrentSprintSpeed; // Running
    private float TurnSpeed = 0.1f; // Running

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
            float TargetAngle = Mathf.Atan2(core.movementInput.x, core.movementInput.y) * Mathf.Rad2Deg + core.Camera.eulerAngles.y;
            float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, TargetAngle, ref turnSmoothVelocity, TurnSpeed);

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

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);

            Vector3 Direction = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;

            // Accelerate player
            if (CurrentSprintSpeed < MaxSprintSpeed)
            {
            CurrentSprintSpeed *= SprintAcceleration;
            }

        core.Character.Move(Direction.normalized * CurrentSprintSpeed * Time.deltaTime);
    }


}
