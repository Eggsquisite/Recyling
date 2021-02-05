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

    [Header("Player Stats")]
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int maxEnergy;
    [SerializeField]
    private int maxStamina;

    private PlayerUI UI;
    private int currentHealth;

    [Header("Movement Properties")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField] [Range(0, 10f)]
    private float walkToRunTime;
    [SerializeField]
    private float verticalSpeedMult;
    [SerializeField]
    private float horizontalSpeedMult;

    private float xAxis;
    private float yAxis;
    private float walkTimer;
    private float stopBuffer;
    private float bufferTimer;
    private bool isStopped;
    private bool isWalking;
    private bool isRunning;
    private bool facingLeft;
    private Vector2 movement;
    private string currentState;

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
    private float dashHeight;

    private bool isDashing;
    private bool isFalling;
    private bool isLanding;
    private bool dashReady = true;
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
    private float runDashTimer;
    private float runDashMaxTime;
    private int attackCombo;
    private bool isAttacking;
    private bool runAttackDash;
    private bool isAttackPressed;
    private bool isRunAttackPressed;
    private bool isSuperAttackPressed;
    private bool canReceiveInput = true;
    private Vector2 runDirection;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();

        UI = GetComponent<PlayerUI>();
        ac = anim.runtimeAnimatorController;
        stopBuffer = .1f;

        UI.SetCurrentHealth(maxHealth);
        UI.SetCurrentEnergy(maxEnergy);

        ResetAttack();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStunned) { 
            WalkingToRunning();
            CheckForMovement();
            CheckForInput();
        }

        if (isStopped)
            StopBuffer();
        if (isHurt)
            DamageFlash();
        if (isFalling)
            DashFall();
        if (!dashReady && !isFalling && !isLanding)
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

            // run attack ----------------------------------------
            RunAttack();

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

    /// <summary>
    /// MOVEMENT CODE /////////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void Movement() {
        movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
        if (!isAttacking) {
            CheckDirection();

            if (isWalking && !isDashing)
                rb.MovePosition(rb.position + movement * walkSpeed * Time.fixedDeltaTime);
            else if (isRunning || isDashing)
                rb.MovePosition(rb.position + movement * runSpeed * Time.fixedDeltaTime);
        }
    }
    private void MovementAnimation() {
        // attack animations override run/idle anims
        if (!isAttacking && !isDashing && !isLanding)
        {
            if (xAxis == 0 && yAxis == 0 && !isFalling)
                PlayAnimation(PlayerAnimStates.PLAYER_IDLE);
            else if (isWalking)
                PlayAnimation(PlayerAnimStates.PLAYER_WALK);
            else if (isRunning)
                PlayAnimation(PlayerAnimStates.PLAYER_RUN);
        }
    }

    /// <summary>
    /// CHECKS ///////////////////////////////////////////////////////////////////////////////////
    /// </summary>
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
    private void CheckForMovement() {
        // movement inputs
        if (isAttacking)
            ResetWalk();
        else { 
            xAxis = Input.GetAxisRaw("Horizontal");
            yAxis = Input.GetAxisRaw("Vertical");
        } 

        // Check to see if stopped moving after walking
        if (xAxis == 0 && yAxis == 0 && (isWalking || isRunning) && !isStopped) { 
            isStopped = true;
            bufferTimer = 0f;
        } // Check to see if start moving after stopping
        else if ((xAxis != 0 || yAxis != 0) && !isRunning && !isWalking)
            isWalking = true;
        else if (isRunning && isWalking)
            isWalking = false;
    }

    private void CheckForInput() {
        // attack inputs
        if (Input.GetKeyDown(KeyCode.Mouse0) && canReceiveInput && !isRunning)
            isAttackPressed = true;
        else if (Input.GetKeyDown(KeyCode.Mouse0) && canReceiveInput && isRunning)
            isRunAttackPressed = true;
        else if (Input.GetKeyDown(KeyCode.Mouse1) && canReceiveInput)
            isSuperAttackPressed = true;

        // dash inputs
        if (Input.GetKeyDown(KeyCode.LeftShift) && canReceiveInput && dashReady && !isDashing)
        {
            ResetWalk();
            isDashing = true;
            dashReady = false;
            isInvincible = true;
            canReceiveInput = false;

            PlayAnimation(PlayerAnimStates.PLAYER_DASH);
        }
    }
    ///
    ///  WALK/RUN CODE ////////////////////////////////////////////////////////////////////////
    ///  
    private void WalkingToRunning() {
        if (isWalking && !isRunning) {
            if (walkTimer < walkToRunTime)
                walkTimer += Time.deltaTime;
            else if (walkTimer >= walkToRunTime) {
                isWalking = false;
                isRunning = true;
            }
        }
    }

    private void ResetWalk() {
        walkTimer = 0f;
        isWalking = false;
        isRunning = false;
    }

    private void StopBuffer() {
        if (bufferTimer < stopBuffer && (xAxis != 0 || yAxis != 0)) { 
            bufferTimer = 0f;
            isStopped = false;
            return;
        }

        if (bufferTimer < stopBuffer)
            bufferTimer += Time.deltaTime;
        else if (bufferTimer >= stopBuffer) {
            walkTimer = 0f;
            ResetWalk();
            isStopped = false;
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

            transform.localPosition = new Vector2(transform.localPosition.x, 
                                        Mathf.Lerp(transform.localPosition.y, 
                                        dashHeight, 
                                        dashSpeed * Time.deltaTime));
        } else if (dashTimer >= dashMaxTime) {
            dashTimer = 0f;
            isFalling = true;
            isDashing = false;
            PlayAnimation(PlayerAnimStates.PLAYER_FALL);
        }
    }

    private void DashFall()
    {
        if (transform.localPosition.y > 0) {
            transform.Translate(0, -6f * Time.deltaTime, 0f, transform.parent);
        }
        else if (transform.localPosition.y <= 0) {
            dashTimer = 0f;
            Stunned();
            isLanding = true;
            isFalling = false;
            isInvincible = false;
            transform.localPosition = Vector2.zero;

            PlayAnimation(PlayerAnimStates.PLAYER_LAND);
            Invoke("ResetLanding", GetAnimationLength(PlayerAnimStates.PLAYER_LAND));
        }
    }

    private void ResetLanding() {
        ResetStun();
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
        }
        // case for combo attacks
        else if (isAttackPressed && canReceiveInput && isAttacking && attackCombo < 3) {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isAttackPressed = false;
        }
        // case for run attack
        else if (isRunAttackPressed) {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isRunAttackPressed = false;
        }
        // case for super attack
        else if (isSuperAttackPressed) {
            AttackAnimation(10);

            isSuperAttackPressed = false;
        }
    }

    private void RunAttack() { 
        if (runAttackDash) {
            if (runDashTimer < runDashMaxTime) { 
                runDashTimer += Time.deltaTime;
                rb.MovePosition(rb.position + runDirection * runSpeed * Time.fixedDeltaTime);
            }
            else if (runDashTimer >= runDashMaxTime) {
                runDashTimer = 0f;
                runAttackDash = false;
            }
        }
    }

    private void AttackAnimation(int attackIndex) {
        // stop movement when attacking and not running
        if (!isRunning) { 
            runDirection = movement = Vector2.zero;
            runAttackDash = false;
            runDashTimer = 0f;
        }
        else {
            runDirection = new Vector2(xAxis, yAxis);
            runDashMaxTime = GetAnimationLength(PlayerAnimStates.PLAYER_RUNATTACK);

            runAttackDash = true;
        }

        isStopped = true;
        canReceiveInput = false;

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
        if (attackIndex == 1 && !isRunning) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_ATTACK1);
            PlayAnimation(PlayerAnimStates.PLAYER_ATTACK1);
        }
        else if (attackIndex == 1 && isRunning)
        {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_RUNATTACK);
            PlayAnimation(PlayerAnimStates.PLAYER_RUNATTACK);
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
        if (isHurt || isInvincible)
            return; 

        isHurt = true;
        isInvincible = true;

        Stunned();
        ResetAttack();

        stunDuration = GetAnimationLength(PlayerAnimStates.PLAYER_HURT);
        PlayAnimation(PlayerAnimStates.PLAYER_HURT);
        Invoke("ResetStun", stunDuration);

        UI.SetCurrentHealth(-damageNum);
        // take damage
    }

    private void Stunned() {
        ResetWalk();
        isStunned = true;
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
