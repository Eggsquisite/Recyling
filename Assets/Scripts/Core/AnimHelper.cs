using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimHelper
{
    public static float GetAnimClipLength(RuntimeAnimatorController ac, string animation)
    {
        float time = 0;

        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == animation)
            {
                time = ac.animationClips[i].length;
                return time;
            }
        }
        return 0;
    }

    public static void ChangeAnimationState(Animator anim, ref string currentState, string newState)
    {
        // guard to stop an animation from overriding itself
        if (currentState == newState) return;

        // play the new animation clip
        anim.Play(newState);

        // set the current state to the new state 
        currentState = newState;
    }

    public static void ReplayAnimation(Animator anim, ref string currentState, string newState)
    {
        anim.Play(newState, -1, 0f);
        currentState = newState;
    }
}
