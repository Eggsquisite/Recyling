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
    private LayerMask playerLayer;

    private RaycastHit2D playerDetected;
    private RaycastHit2D hitBox;

    [Header("Enemy Stats")]
    [SerializeField]
    private int health;

    /*[SerializeField]
    private float deathFadeTime;*/

    private bool isDead;
    private bool isInvincible;

    [Header("Attack Properties")]
    [SerializeField]
    private float stunDelay;

    [SerializeField]
    private int damage;

    [SerializeField] [Range(0, 5f)]
    private float attackRange;

    [SerializeField] [Range(0, 5f)]
    private float minAttackDelay;
    [SerializeField] [Range(0, 5f)]
    private float maxAttackDelay;

    private bool inRange;
    private bool isStunned;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private float attackDelay;
    private float stunDuration;

    [Header("Follow Player")]
    [SerializeField] [Range(0, 5f)]
    private float minMoveSpeed;
    [SerializeField] [Range(0, 5f)]
    private float maxMoveSpeed;

    [SerializeField] [Range(0.5f, 5f)]
    private float minOffset;
    [SerializeField] [Range(0.5f, 5f)]
    private float maxOffset;

    private bool canFollow;
    private bool leftOfPlayer;
    private float baseMoveSpeed;
    private float xScaleValue;
    private Vector2 leftOffset, rightOffset, playerChar;

    // Start is called before the first frame update
    void Start() {
        SetupVariables();
        InvokeRepeating("FindPlayer", 1f, 0.25f);
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHitbox) 
            CheckHitBox();

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerInRange();
        FollowPlayer();

        /////////////////////////// Attack Animation Activated //////////////////
        if (inRange && !isStunned && !isAttacking && attackReady) {
            AttackAnimation();
        }
    }

    private void SetupVariables()
    {
        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        canFollow = true;
        attackReady = true;
        xScaleValue = transform.localScale.x;

        if (playerChar.x > transform.position.x) { 
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x) { 
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

        baseMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
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
        if (canFollow) { 
            playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
        }
    }

    private void FollowPlayer() {
        /*if (inRange && !canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            Debug.Log("1");
        }
        else if (!canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            Debug.Log("2");
        }*/
        if (inRange && !attackReady && !isStunned && !isAttacking) { 
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
        }
        // ENEMY MOVEMENT ///////////////////////////////////////////////
        else if (canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_RUN);

            if (leftOfPlayer) {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, baseMoveSpeed * Time.deltaTime);
            }
            else {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, baseMoveSpeed * Time.deltaTime);
            }
        }
    }

    private void CheckPlayerPos()
    {
        // Set variable of leftOfPlayer
        if (playerChar.x > transform.position.x && !leftOfPlayer) { 
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer) { 
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }
    }

    private void CheckPlayerInRange()
    {
        CheckPlayerPos();

        if (leftOfPlayer)
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.right, attackRange, playerLayer);
        else
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.left, attackRange, playerLayer);

        if (playerDetected.collider != null && !inRange) { 
            inRange = true;
            /*Debug.Log(name + " is IN range!");*/
        }
        else if (playerDetected.collider == null && inRange) { 
            inRange = false;
            /*Debug.Log(name + " is OUT OF range!");*/
        }
    }

    private void ResetFollow() {
        canFollow = true;
        FindPlayer();
    }
    private void ResetStun() {
        ResetFollow();
        isStunned = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", stunDelay);
    }
    ////////////////// Attack Code /////////////////////////////
    private void ResetAttack() {
        attackReady = true;
    }
    private void FinishAttack() {
        ResetFollow();
        isAttacking = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", attackDelay);
    }
    private void AttackActivated() {
        //called thru animation event
        attackHitbox = true;
    }
    private void AttackDeactivated() {
        //called thru animation event
        attackHitbox = false;
    }

    private void CheckHitBox() {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (leftOfPlayer)
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.right, attackRange, playerLayer);
        else
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.left, attackRange, playerLayer);

        if (hitBox.collider != null) { 
                hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(damage);
        }
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
        if (isDead || isInvincible)
            return;

        isStunned = true;
        canFollow = false;
        attackReady = false;
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

    /*IEnumerator FadeAway() {
        float alpha = sp.color.a;

        for (float t = 0.0f; t < deathFadeTime; t += Time.deltaTime) {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, 0.25f, t / deathFadeTime));
            sp.color = newColor;
            yield return null;
        }

        //Destroy(gameObject);
    }*/

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (leftOfPlayer)
            Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRange));
        else
            Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.left * attackRange));
    }   
}
