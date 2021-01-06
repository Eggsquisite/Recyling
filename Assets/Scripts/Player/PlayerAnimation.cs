using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;

    private string currentState;
    private float time;
    RuntimeAnimatorController ac;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        currentState = AnimStates.PLAYER_IDLE;

        ac = anim.runtimeAnimatorController;

        Debug.Log(currentState);
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        anim.Play(newState);

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
