using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float verticalSpeedMult = 0.75f;
    [SerializeField]
    private float horizontalSpeedMult = 1.25f;

    private float xAxis;
    private float yAxis;
    private bool facingLeft;
    private Vector2 movement;

    [Header("Damaged Properties")]
    [SerializeField]
    private float isHurtMaxTime;
    private float isHurtTimer;
    private float flashInterval = 0.1f;
    private float flashTimer;
    private bool isHurt;
    private bool isInvincible;
    private bool isStunned;
    private float stunDuration;

    private string currentState;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackLengthMultiplier;

    [SerializeField]
    private float attackWidthMultiplier;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Attack Properties")]
    [SerializeField]
    private int damage;

    [SerializeField]
    private int specialAttackDmg;

    [SerializeField]
    private float attackRangeVisualizer;

    [SerializeField]
    private float pushbackDistance;

    [SerializeField]
    private float specialPushbackMultiplier;

    private float attackDelay;
    private int attackCombo;
    private bool isAttackPressed;
    private bool isSuperAttackPressed;
    private bool isAttacking;
    private bool canReceiveInput = true;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        ResetAttack();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStunned)
        {
            // movement inputs
            xAxis = Input.GetAxisRaw("Horizontal");
            yAxis = Input.GetAxisRaw("Vertical");

            // attack inputs
            if (Input.GetKeyDown(KeyCode.Mouse0) && canReceiveInput)
                isAttackPressed = true;
            else if (Input.GetKeyDown(KeyCode.Mouse1) && canReceiveInput)
                isSuperAttackPressed = true;
        }

        if (isHurt)
            DamageFlash();
    }

    private void FixedUpdate() {
        if (!isStunned) {
            // movement ---------------------------------
            Movement();

            // idle/run animation --------------------------------
            MovementAnimation();

            //attack ------------------------------------
            Attack();
        }
    }

    // Animation Helper Functions ////////////////////////////////////////
    private void PlayAnimation(string newAnim)
    {
        AnimHelper.ChangeAnimationState(anim, ref currentState, newAnim);
    }
    private float GetAnimationLength(string newAnim)
    {
        return AnimHelper.GetAnimClipLength(ac, newAnim);
    }
    // Animation Helper Functions ////////////////////////////////////////

    private void Movement() {
        movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
        if (!isAttacking) {
            CheckDirection();
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void CheckDirection() {
        if (xAxis < 0 && !facingLeft) {
            facingLeft = true;
            transform.localScale = new Vector2(-1, 1);
            Debug.Log("facing left");
        }
        else if (xAxis > 0 && facingLeft) {
            facingLeft = false;
            transform.localScale = new Vector2(1, 1);
            Debug.Log("Facing right");
        }
    }

    private void MovementAnimation() {
        // attack animations override run/idle anims
        if (!isAttacking)
        {
            if (xAxis != 0 || yAxis != 0)
                PlayAnimation(PlayerAnimStates.PLAYER_RUN);
            else
                PlayAnimation(PlayerAnimStates.PLAYER_IDLE);
        }
    }

    private void Attack() {
        // case for first attack
        if (isAttackPressed && canReceiveInput && !isAttacking && attackCombo == 0) {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isAttackPressed = false;
            canReceiveInput = false;
        }
        // case for combo attacks
        else if (isAttackPressed && canReceiveInput && isAttacking && attackCombo < 3) {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isAttackPressed = false;
            canReceiveInput = false;
        }
        // case for super attack
        else if (isSuperAttackPressed) {
            AttackAnimation(10);

            isSuperAttackPressed = false;
            canReceiveInput = false;
        }
    }

    private void AttackAnimation(int attackIndex) {
        // stop movement when attacking
        movement = Vector2.zero;

        // case for first attack in combo/super attack
        if (!isAttacking) {
            isAttacking = true;

            GetAttackDelay(attackIndex);

            Invoke("ResetAttack", attackDelay);
        }
        // case for combos
        else if (isAttacking) {
            CancelInvoke("ResetAttack");

            GetAttackDelay(attackIndex);

            Invoke("ResetAttack", attackDelay);
        }
    }

    private void GetAttackDelay(int attackIndex) {
        if (attackIndex == 1) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_ATTACK1);
            PlayAnimation(PlayerAnimStates.PLAYER_ATTACK1);
        }
        else if (attackIndex == 2) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_ATTACK2);
            PlayAnimation(PlayerAnimStates.PLAYER_ATTACK2);
        }
        else if (attackIndex == 3) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_ATTACK3);
            PlayAnimation(PlayerAnimStates.PLAYER_ATTACK3);
        }
        else if (attackIndex == 10) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_SUPERATTACK);
            PlayAnimation(PlayerAnimStates.PLAYER_SUPERATTACK);
        }
        else
            return;
    }

    private void AttackHitboxActivated(float attackRange) {
        Collider2D[] hitEnemies;
        if (facingLeft) { 
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.left * attackRange), enemyLayer);
            Debug.Log("Attacking left");
        }
        else
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRange), enemyLayer);

        foreach (Collider2D enemy in hitEnemies) {
            if (enemy.tag == "Enemy")
                enemy.GetComponent<BasicEnemy>().EnemyHurt(Mathf.RoundToInt(damage), pushbackDistance, transform);
        }
    }

    private void SpecialAttackHitboxActivated(float attackRange)
    {
        Collider2D[] hitEnemies;
        if (facingLeft)
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.left * attackRange), enemyLayer);
        else
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRange), enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.tag == "Enemy")
                enemy.GetComponent<BasicEnemy>().EnemyHurt(Mathf.RoundToInt(specialAttackDmg), pushbackDistance * specialPushbackMultiplier, transform);
        }
    }

    private void ResetAttack() {
        attackCombo = 0;
        isAttacking = false;
        canReceiveInput = true;
    }

    private void ComboInput() {
        // called during attack animation, allows input such as combos or dodging 
        if (isAttacking && attackCombo > 0)
            canReceiveInput = true;
        else
            return;
    }

    public void PlayerHurt(int damageNum) {
        if (!isHurt && !isInvincible) {
            isHurt = true;
            isStunned = true;
            isInvincible = true;
            ResetAttack();

            stunDuration = GetAnimationLength(PlayerAnimStates.PLAYER_HURT);
            PlayAnimation(PlayerAnimStates.PLAYER_HURT);
            Invoke("ResetStun", stunDuration);

            // take damage
        }
    }

    private void ResetStun() {
        isStunned = false;
    }

    private void DamageFlash() { 
        if (flashTimer < flashInterval)
            flashTimer += Time.deltaTime;
        else if (flashTimer >= flashInterval) {
            sp.enabled = !sp.enabled;
            flashTimer = 0;
        }

        if (isHurtTimer < isHurtMaxTime)
            isHurtTimer += Time.deltaTime;
        else if (isHurtTimer >= isHurtMaxTime) {
            isHurt = false;
            isInvincible = false;
            sp.enabled = true;

            isHurtTimer = 0;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRangeVisualizer));
    }
}
