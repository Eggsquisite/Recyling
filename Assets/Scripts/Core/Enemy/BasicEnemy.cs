using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;
    private string currentState;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackLengthMultiplier;

    [SerializeField]
    private float attackWidthMultiplier;

    [SerializeField]
    private LayerMask playerLayer;

    private RaycastHit2D hit;

    [Header("Enemy Stats")]
    [SerializeField]
    private int health;

    [SerializeField]
    private float deathFadeTime;

    private bool isDead;

    [Header("Attack Properties")]
    [SerializeField]
    private int damage;

    [SerializeField]
    private float minAttackDelay;

    [SerializeField]
    private float maxAttackDelay;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private Transform detectPlayerPos;

    public bool inRange;
    private bool isStunned;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private float attackDelay;
    private float stunDuration;

    [Header("Follow Player")]
    [SerializeField]
    private float minMoveSpeed;
    [SerializeField]
    private float maxMoveSpeed;

    [SerializeField]
    private float minOffset;
    [SerializeField]
    private float maxOffset;

    public bool canFollow;
    private float moveSpeed;
    private float xScaleValue;
    private Vector2 leftOffset, rightOffset, playerChar;

    // Start is called before the first frame update
    void Start() {
        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        SetupVariables();
        InvokeRepeating("FindPlayer", 1f, 0.5f);
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHitbox) 
            CheckHitBox();

        ///////////////////// Follow Player /////////////////////////////////////
        FollowPlayer();

        /////////////////////////// Attack Animation Activated //////////////////
        if (inRange && !isStunned && !isAttacking && attackReady) {
            AttackAnimation();
        }
    }

    private void SetupVariables()
    {
        canFollow = true;
        attackReady = true;
        xScaleValue = transform.localScale.x;

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);

        leftOffset = new Vector2(Random.Range(-minOffset, -maxOffset), 0);
        rightOffset = new Vector2(Random.Range(minOffset, maxOffset), 0);
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

    private void FollowPlayer() {
        if (inRange && !canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
        }
        else if (canFollow && !isStunned && !isAttacking) {
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
            else if (playerChar.x <= transform.position.x) {
                transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, moveSpeed * Time.deltaTime);

                hit = Physics2D.Raycast(detectPlayerPos.position, Vector2.left, attackRange, playerLayer);
                if (hit.collider != null && !inRange)
                    inRange = true;
                else if (hit.collider == null && inRange)
                    inRange = false;
            }
        }
    }

    private void ResetFollow() {
        canFollow = true;
        FindPlayer();
    }

    private void ResetStun() {
        ResetFollow();
        isStunned = false;
    }

    ////////////////// Attack Code ///////////////////////
    private void CheckHitBox() {
        Collider2D[] hitPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);

        foreach (Collider2D player in hitPlayer)
        {
            if (player.tag == "Player")
                player.GetComponent<Player>().PlayerHurt(damage);
        }
    }

    private void FinishAttack() {
        isAttacking = false;
        Invoke("ResetAttack", attackDelay);
    }

    private void ResetAttack() {
        ResetFollow();
        attackReady = true;
    }

    private void AttackActivated() {
        //called thru animation event
        attackHitbox = true;
    }

    private void AttackDeactivated() {
        //called thru animation event
        attackHitbox = false;
    }

    private void AttackAnimation() {
        canFollow = false;
        isAttacking = true;
        attackReady = false;

        PlayAnimation(EnemyAnimStates.ENEMY_ATTACK1);
        Invoke("FinishAttack", GetAnimationLength(EnemyAnimStates.ENEMY_ATTACK1));
    }

    /////////////// Enemy Is Hit //////////////////
    public void EnemyHurt(int damageNum, float distance, Transform playerRef) {
        if (isDead)
            return;

        isStunned = true;
        canFollow = false;
        PushBack(distance, playerRef);
        if (IsInvoking("ResetStun"))
            CancelInvoke("ResetStun");

        health -= damageNum;
        if (health <= 0)
            Death();
        else { 
            ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
            stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

            Invoke("ResetStun", stunDuration + 0.25f);
        }
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

    private void Death() {
        isDead = true;
        CancelInvoke("FindPlayer");
        //StartCoroutine(FadeAway());
        PlayAnimation(EnemyAnimStates.ENEMY_DEATH);
    }

    IEnumerator FadeAway() {
        float alpha = sp.color.a;

        for (float t = 0.0f; t < deathFadeTime; t += Time.deltaTime) {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, 0.25f, t / deathFadeTime));
            sp.color = newColor;
            yield return null;
        }

        //Destroy(gameObject);
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
