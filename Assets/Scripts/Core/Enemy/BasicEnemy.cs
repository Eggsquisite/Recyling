using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private Animator anim;
    private RuntimeAnimatorController ac;

    private bool isAttacking;
    private bool attackHit;
    public bool inRange;
    private string currentState;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private float attackDelay;

    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackLengthMultiplier;

    [SerializeField]
    private float attackWidthMultiplier;

    [SerializeField]
    private LayerMask playerLayer;

    private RaycastHit2D hit;

    [Header("Attack Properties")]
    [SerializeField]
    private int damage;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private Transform detectPlayerPos;

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
        ac = anim.runtimeAnimatorController;
        xScaleValue = transform.localScale.x;

        ResetAttack();
        //InvokeRepeating("AttackAnimation", 2f, attackDelay);
        InvokeRepeating("FindPlayer", 1f, 0.5f);
    }

    private void Update() {
        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHit) {
            Collider2D[] hitPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);

            foreach (Collider2D player in hitPlayer) {
                if (player.tag == "Player")
                    player.GetComponent<Player>().PlayerHurt(1);
            }
        }

        ///////////////////// Follow Player /////////////////////////////////////
        if (canFollow && !isStunned) {
            PlayAnimation(EnemyAnimStates.ENEMY_RUN);

            // player is to the right of enemy
            if (playerChar.x > transform.position.x) {
                transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, moveSpeed * Time.deltaTime);

                hit = Physics2D.Raycast(detectPlayerPos.position, Vector2.right, attackRange, playerLayer);
                if (hit.collider != null && !inRange)
                    inRange = true;
                else if (hit.collider == null && inRange)
                    inRange = false;
            }
            // player is to the left of enemy
            else if (playerChar.x <= transform.position.x)  {
                transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, moveSpeed * Time.deltaTime);

                hit = Physics2D.Raycast(detectPlayerPos.position, Vector2.left, attackRange, playerLayer);
                if (hit.collider != null && !inRange)
                    inRange = true;
                else if (hit.collider == null && inRange)
                    inRange = false;
            }
        }
        else if (!canFollow && !isStunned && !isAttacking) { 
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            Debug.Log("IDleing");
        }

        /////////////////////////// Attack Animation Activated /////////////////
        if (inRange && !isStunned && !isAttacking) {
            AttackAnimation();
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (playerChar.x <= transform.position.x)
            Gizmos.DrawRay(detectPlayerPos.position, Vector2.left);
        else if (playerChar.x > transform.position.x)
            Gizmos.DrawRay(detectPlayerPos.position, Vector2.right);
    }   
}
