using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator anim;
    private RuntimeAnimatorController ac;
    private string currentState;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        ac = anim.runtimeAnimatorController;
    }

    public void PlayAnimation(string newAnim) {
        AnimHelper.ChangeAnimationState(anim, ref currentState, newAnim);
    }
    public void ReplayAnimation(string newAnim) {
        AnimHelper.ReplayAnimation(anim, ref currentState, newAnim);
    }
    public float GetAnimationLength(string newAnim) {
        return AnimHelper.GetAnimClipLength(ac, newAnim);
    }
}
