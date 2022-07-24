using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player_State
{
    protected PlayerStateMachineCore core;
    protected State_Animation_Controller AnimationController;
    protected Player_Timers data;
    protected State_Context_Handler StateContext;
    protected Player_State(PlayerStateMachineCore core)
    {
        this.core = core;
        data = core.StateMemory;
        AnimationController = core.AnimationController;
        StateContext = core.StateContext;
    }

    public abstract void  StartMethod();
    public abstract void  UpdateMethod();
    public virtual float GetUpdateToGravity() {
        return 0;
    }
    public abstract void  ExitMethod();
    public abstract void  CheckStateSwaps();

    public Vector3 AlignVectorToSlope(Vector3 VectorToAlign, Vector3 currentPosition)
    {
        var raycast = new Ray(currentPosition, Vector3.down);
        var raycastFoundGround = Physics.Raycast(raycast, out RaycastHit hitInfo, 200f);

        if (raycastFoundGround == false)
        {
            return VectorToAlign;
        }

        var AngleOfCorrection = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

        return AngleOfCorrection * VectorToAlign;
    }

    public float TargetAngleCameraRelative(PlayerStateMachineCore core)
    {
        return Mathf.Atan2(StateContext.MovementInput.x, StateContext.MovementInput.y) * Mathf.Rad2Deg + StateContext.CameraRotation.y;
    }
}
