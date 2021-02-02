using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;

    private SpriteRenderer sp;
    private Animator anim;
    private RuntimeAnimatorController ac;

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

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Dash Properties")]
    [SerializeField]
    private float dashSpeed;
    [SerializeField]
    private float dashCooldown;
    [SerializeField]
    private float dashMaxTime;
    [SerializeField]
    private float landingDelay;  

    private bool dashReady;
    private bool isDashing;
    private bool isLanding;
    private float dashTimer;
    private float dashCooldownTimer;

    [Header("Attack Properties")]
    [SerializeField]
    private int damage;

    [SerializeField] [Range(0, 5f)]
    private float attackRangeVisualizer;

    [SerializeField] [Range(0, 0.5f)]
    private float pushbackDistance;

    [SerializeField]
    private int specialAttackDmg;

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
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
        ac = anim.runtimeAnimatorController;
        dashReady = true;

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

            // dash inputs
            if (Input.GetKeyDown(KeyCode.LeftShift) && canReceiveInput && dashReady && !isDashing) {
                isDashing = true;
                dashReady = false;
                canReceiveInput = false;

                Debug.Log("Starting dash...");
                PlayAnimation(PlayerAnimStates.PLAYER_DASH);

                // dash cases for standing still 
                /*if (xAxis == 0 && yAxis == 0 && facingLeft)
                    dashDirection = new Vector2(-1f, 0f);
                else if (xAxis == 0 && yAxis == 0 && !facingLeft)
                    dashDirection = new Vector2(1f, 0f);
                else
                    dashDirection = new Vector2(xAxis, yAxis);*/
            }
        }

        if (isHurt)
            DamageFlash();
        if (isLanding)
            DashLand();
        if (!dashReady && !isLanding)
            ResetDash();
    }

    private void FixedUpdate() {
        if (!isStunned) {
            // movement ---------------------------------
            Movement();

            // idle/run animation --------------------------------
            MovementAnimation();

            // dash -----------------------------------------------
            Dashing();

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
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
        else if (xAxis > 0 && facingLeft) {
            facingLeft = false;
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }

    private void MovementAnimation() {
        // attack animations override run/idle anims
        if (!isAttacking && !isDashing && !isLanding)
        {
            if (xAxis != 0 || yAxis != 0)
                PlayAnimation(PlayerAnimStates.PLAYER_RUN);
            else
                PlayAnimation(PlayerAnimStates.PLAYER_IDLE);
        }
    }

    /// <summary>
    /// DASH CODE /////////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void Dashing() {
        if (!isDashing)
            return;

        if (dashReady) { 
            dashReady = false;
        }

        if (dashTimer < dashMaxTime) {
            dashTimer += Time.deltaTime;
            //rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            transform.Translate(0, dashSpeed * Time.deltaTime, 0f, transform.parent);
        } else if (dashTimer >= dashMaxTime) {
            dashTimer = 0f;
            isDashing = false;
            isLanding = true;
            PlayAnimation(PlayerAnimStates.PLAYER_FALL);
        }
    }

    private void DashLand()
    {
        if (transform.position.y > 0) {
            transform.Translate(0, -dashSpeed * Time.deltaTime, 0f, transform.parent);
        }
        else if (transform.position.y <= 0) {
            dashTimer = 0f;
            
            //transform.position = new Vector2(0f, 0f);
            PlayAnimation(PlayerAnimStates.PLAYER_LAND);
            Invoke("ResetLanding", GetAnimationLength(PlayerAnimStates.PLAYER_LAND) + landingDelay);
        }
    }

    private void ResetLanding() {
        isLanding = false;
        if (!isAttacking)
            canReceiveInput = true;
    }

    private void ResetDash() {
        if (dashCooldownTimer < dashCooldown)
            dashCooldownTimer += Time.deltaTime;
        else if (dashCooldownTimer >= dashCooldown) {
            dashReady = true;
            dashCooldownTimer = 0f;
            Debug.Log("Dash Ready!");
        }
    }

    /// <summary>
    /// ATTACK CODE ////////////////////////////////////////////////////////////////////////////
    /// </summary>
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
