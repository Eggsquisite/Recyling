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

    private bool isStunned;
    private float stunDuration;

    [Header("Follow Player")]
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Vector2 rightOffset;

    [SerializeField]
    private Vector2 leftOffset;

    private Vector2 playerChar;
    public bool canFollow;
    private float xScaleValue;

    // Start is called before the first frame update
    void Start() {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;
        xScaleValue = transform.localScale.x;

        ResetAttack();
        //InvokeRepeating("AttackAnimation", 2f, attackDelay);
        InvokeRepeating("FindPlayer", 1f, 0.5f);
    }

    private void Update() {
        // Attack Hitbox Activated
        if (attackHit) {
            Collider2D[] hitPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);

            foreach (Collider2D player in hitPlayer) {
                if (player.tag == "Player")
                    player.GetComponent<Player>().PlayerHurt(1);
            }
        }

        // Follow Player
        if (canFollow && !isStunned) {
            PlayAnimation(EnemyAnimStates.ENEMY_RUN);

            // player is to the right of enemy
            if (playerChar.x > transform.position.x) {
                transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, moveSpeed * Time.deltaTime);
            }
            // player is to the left of enemy
            else if (playerChar.x <= transform.position.x)  {
                transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, moveSpeed * Time.deltaTime);
            }
        }
        else if (!canFollow && !isStunned && !isAttacking)
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
    }

    /////////////////// Animation Helper Functions ////////
    private void PlayAnimation(string newAnim) {
        AnimHelper.ChangeAnimationState(anim, ref currentState, newAnim);
    }
    private void ReplayAnimation(string newAnim) {
        AnimHelper.ReplayAnimation(anim, ref currentState, newAnim);
    }
    private float GetAnimationLength(string newAnim)
    {
        return AnimHelper.GetAnimClipLength(ac, newAnim);
    }

    ////////////////// Find Player AI ////////////////////
    private void FindPlayer() {
        if (canFollow)
            playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    private void ResetFollow() {
        canFollow = true;
    }

    private void ResetStun() {
        ResetFollow();
        isStunned = false;
    }

    ////////////////// Attack Code ///////////////////////
    private void ResetAttack() {
        ResetFollow();
        isAttacking = false;
    }

    private void AttackActivated() {
        //called thru animation event
        attackHit = true;
    }

    private void AttackDeactivated() {
        //called thru animation event
        attackHit = false;
    }

    private void AttackAnimation() {
        if (!isStunned && !isAttacking) {
            isAttacking = true;
            canFollow = false;

            PlayAnimation(EnemyAnimStates.ENEMY_ATTACK1);
            Invoke("ResetAttack", GetAnimationLength(EnemyAnimStates.ENEMY_ATTACK1));
        }
    }

    /////////////// Enemy Is Hit //////////////////
    public void EnemyHurt(float damageNum, float distance, Transform playerRef) {
        isStunned = true;
        canFollow = false;
        CancelInvoke("ResetStun");
        PushBack(distance, playerRef);

        ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
        stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

        Invoke("ResetStun", stunDuration + 0.25f);
    }

    private void PushBack(float distance, Transform reference) {
        Vector2 newPosition;
        if (reference.transform.position.x > transform.position.x) {
            newPosition = new Vector2(-distance, 0f) + (Vector2)transform.position;
            transform.position = newPosition;
        }
        else if (reference.transform.position.x <= transform.position.x) {
            newPosition = new Vector2(distance, 0f) + (Vector2)transform.position;
            transform.position = newPosition;
        }
    }
}
