using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private RuntimeAnimatorController ac;

    private string currentState;
    private float time;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        ac = anim.runtimeAnimatorController;
    }

    public void ChangeAnimationState(string newState)
    {
        // guard to stop an animation from overriding itself
        if (currentState == newState) return;

        // play the new animation clip
        anim.Play(newState);

        // set the current state to the new state 
        currentState = newState;
    }

    public float GetAnimationClipLength(string animation)
    {
        time = 0;

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
}
