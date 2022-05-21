using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player_State : MonoBehaviour
{
    public abstract void StartMethod();
    public abstract void UpdateMethod();
    public abstract void ExitMethod();

}
