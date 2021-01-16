using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;

    private bool isAttacking;
    private string currentState;
    private float time;

    [Header("Attack Collider")]
    [SerializeField]
    private float attackDelay;

    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackDistanceMultiplier;

    [SerializeField]
    private float attackRadius;

    [SerializeField]
    private LayerMask playerLayer;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        ResetAttack();
        InvokeRepeating("AttackAnimation", 2f, attackDelay);
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

    private void AttackAnimation()
    {
        isAttacking = true;
        ChangeAnimationState(EnemyAnimStates.ENEMY_ATTACK1);


        Invoke("ResetAttack", GetAnimationClipLength(EnemyAnimStates.ENEMY_ATTACK1));
    }

    private void Attack()
    {
        Debug.Log("attacking");
        Collider2D[] hitPlayer = Physics2D.OverlapCapsuleAll(new Vector2(attackPoint.transform.position.x, attackPoint.transform.position.y), 
                                                        new Vector2(1, 0.3f) * attackDistanceMultiplier, 
                                                        CapsuleDirection2D.Horizontal,
                                                        0f);

        foreach (Collider2D player in hitPlayer)
        { 
            if (player.tag == "Player")
                Debug.Log("Hit + " + player.name);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        ChangeAnimationState(EnemyAnimStates.ENEMY_IDLE);
    }

    private void OnDrawGizmosSelected()
    {
        DebugExtension.DrawCapsule(attackPoint.position, attackPoint.position + Vector3.right * attackDistanceMultiplier, Color.grey, attackRadius);
    }
}
