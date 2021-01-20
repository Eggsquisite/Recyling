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
    private bool attackHit;
    private string currentState;
    private float time;

    [Header("Attack Collider")]
    [SerializeField]
    private float attackDelay;

    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackLengthMultiplier;

    [SerializeField]
    private float attackWidthMultiplier;

    /*[SerializeField]
    private LayerMask playerLayer;*/

    [Header("Attack Stats")]
    [SerializeField]
    private int damage;

    private bool hurt;
    private bool isStunned;
    private float stunDuration;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        ResetAttack();
        //InvokeRepeating("AttackAnimation", 2f, attackDelay);
    }

    private void Update()
    {
        if (attackHit)
        {
            Collider2D[] hitPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);

            foreach (Collider2D player in hitPlayer)
            {
                if (player.tag == "Player")
                    player.GetComponent<Player>().Hurt(1);
            }
        }
    }

    private void AttackAnimation()
    {
        if (!isStunned)
        {
            isAttacking = true;
            AnimHelper.ChangeAnimationState(anim, ref currentState, EnemyAnimStates.ENEMY_ATTACK1);

            Invoke("ResetAttack", AnimHelper.GetAnimClipLength(ac, EnemyAnimStates.ENEMY_ATTACK1));
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;

        if (!isStunned)
            AnimHelper.ChangeAnimationState(anim, ref currentState, EnemyAnimStates.ENEMY_IDLE);
    }
    private void AttackOn()
    {
        //called thru animation event
        attackHit = true;
    }

    private void AttackOff()
    {
        //called thru animation event
        attackHit = false;
    }

    public void Hurt(float damageNum)
    {
        if (!hurt)
        {
            Debug.Log("Enemy Hit + " + name);
            hurt = true;
            isStunned = true;

            stunDuration = AnimHelper.GetAnimClipLength(ac, EnemyAnimStates.ENEMY_HURT);
            AnimHelper.ChangeAnimationState(anim, ref currentState, EnemyAnimStates.ENEMY_HURT);
            Invoke("ResetStun", stunDuration);
        }
    }

    private void ResetStun()
    {
        hurt = false;
        isStunned = false;
        AnimHelper.ChangeAnimationState(anim, ref currentState, EnemyAnimStates.ENEMY_IDLE);
    }

    private void OnDrawGizmosSelected()
    {
        //DebugExtension.DrawCapsule(attackPoint.position, attackPoint.position + Vector3.right * attackDistanceMultiplier, Color.grey, attackRadius);
    }
}
