using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player_State
{
    public abstract void StartMethod();
    public abstract void UpdateMethod();
    public abstract void ExitMethod();
    public abstract void CheckForStateSwap();
    public abstract float GetUpdateToGravity();
}
