using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Animation_Controller : ScriptableObject
{
    Animator Animator;

    public State_Animation_Controller(Animator Animator)
    {
        this.Animator = Animator;
    }

    public void ChangeAnimationSpeed(float speed)
    {
        Animator.speed = speed;
    }

    public void ChangeAnimationState(string animation, bool newState)
    {
        Animator.SetBool(animation, newState);
    }

    public bool CheckAnimationProgress(string name, float seconds)
    {
        if (Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == name)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= seconds && !Animator.IsInTransition(0);
        }
        return false;
    }

}
