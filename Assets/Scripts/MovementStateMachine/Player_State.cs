using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player_State
{
    public abstract void  StartMethod();
    public abstract void  UpdateMethod();
    public abstract float GetUpdateToGravity();
    public abstract void  ExitMethod();
    public abstract void  CheckForStateSwap();

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
}
