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
    private PlayerStats playerStats;
    private PlayerSounds playSounds;
    private PlayerUI UI;
    private bool isDead;

    [Header("Movement Properties")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float verticalSpeedMult;
    [SerializeField]
    private float horizontalSpeedMult;
    [SerializeField]
    private float walkToRunTime;
    [SerializeField]
    private float stopBuffer;

    private float xAxis;
    private float yAxis;
    private float walkTimer;
    private float bufferTimer;
    private bool shiftKeyHeld;
    private bool isStopped;
    private bool isWalking;
    private bool isRunning;
    private bool facingLeft;
    private Vector2 movement;
    private string currentState;

    [Header("Damaged Properties")]
    [SerializeField]
    private float isHurtMaxTime;

    private bool isHurt;
    private bool isStunned;
    private bool isInvincible;
    private float flashTimer;
    private float isHurtTimer;
    private float stunDuration;
    private float flashInterval = 0.1f;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Dash Properties")]
    //[SerializeField]
    //private float dashMinTime;
    [SerializeField]
    private float dashMoveSpeed;
    [SerializeField]
    private float dashCooldown;
    [SerializeField]
    private float dashHeight;

    private bool isDashing;
    private bool isFalling;
    private bool isLanding;
    private bool stopDash;
    private bool dashReady = true;
    private float dashTimer;
    private float dashCooldownTimer;

    [Header("Stamina/Energy Consumption")]
    [SerializeField]
    private int dashEnergy;
    [SerializeField]
    private int runStamina;
    [SerializeField]
    private int runAttackStamina;
    [SerializeField]
    private int basicAttackStamina;
    [SerializeField]
    private int superAttackStamina1;
    [SerializeField]
    private int superAttackEnergy1;
    [SerializeField]
    private int superAttackStamina2;
    [SerializeField]
    private int superAttackEnergy2;

    [Header("Attack Properties")]
    [SerializeField]
    private int damage;
    [SerializeField] 
    private float attackRangeVisualizer;
    [SerializeField]
    private float pushbackDistance;
    [SerializeField]
    private int specialAttackDmg;
    [SerializeField]
    private float specialPushbackMultiplier;
    [SerializeField]
    private float attackFollowThruDistance;

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

    private Coroutine resetStunRoutine, resetAttackRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playSounds == null) playSounds = GetComponent<PlayerSounds>();
        if (UI == null) UI = GetComponent<PlayerUI>();

        canReceiveInput = true;
        ac = anim.runtimeAnimatorController;
        runDashMaxTime = GetAnimationLength(PlayerAnimStates.PLAYER_RUNATTACK);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        ResetRun();
        DamageFlash();
        WalkingToRunning();
    }

    private void FixedUpdate() {
        if (isDead)
            return;

        if (!isStunned) {
            // movement ---------------------------------
            Movement();

            // idle/run animation --------------------------------
            MovementAnimation();

            // run attack dash --------------------------------
            RunAttack();
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
        CheckDirection();
        if (!isAttacking) {
            if (isWalking && !isDashing)
                rb.MovePosition(rb.position + movement * walkSpeed * Time.fixedDeltaTime);
            else if (isRunning)
                rb.MovePosition(rb.position + movement * runSpeed * Time.fixedDeltaTime);
            else if (isDashing)
                rb.MovePosition(rb.position + movement * dashMoveSpeed * Time.fixedDeltaTime);
        }
    }
    private void MovementAnimation() {
        // attack animations override run/idle anims
        if (!isAttacking && !isDashing && !isFalling && !isLanding)
        {
            if (xAxis == 0 && yAxis == 0)
                PlayAnimation(PlayerAnimStates.PLAYER_IDLE);
            else if (isWalking)
                PlayAnimation(PlayerAnimStates.PLAYER_WALK);
            else if (isRunning)
                PlayAnimation(PlayerAnimStates.PLAYER_RUN);
        }
    }

    /// <summary>
    /// INPUTS AND CHECKS ///////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public void CheckForMovement(float xInput, float yInput) {
        // movement inputs
        if (isAttacking)
            ResetWalk();
        else { 
            xAxis = xInput;
            yAxis = yInput;
        }

        // Check to see if stopped moving after walking
        if (xAxis == 0 && yAxis == 0 && (isWalking || isRunning) && !isStopped) {
            isStopped = true;
            bufferTimer = 0f;
            StartCoroutine(StopTimer());
        } // Check to see if start moving after stopping
        else if ((xAxis != 0 || yAxis != 0) && !isRunning && !isWalking) {
            isWalking = true;
            isStopped = false;
        }
        else if ((xAxis != 0 || yAxis != 0) && (isWalking || isRunning)) 
            isStopped = false;
        else if (isRunning && isWalking)
            isWalking = false;
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

    public void BasicAttackInput() {
        if (isStunned || playerStats.GetCurrentStamina() <= 0)
            return; 

        if (canReceiveInput) {
            if (!isRunning)
                isAttackPressed = true;
            else 
                isRunAttackPressed = true;

            Attack();
        }
    }

    public void SuperAttackInput() {
        if (isStunned 
            || playerStats.GetCurrentStamina() <= 0 
            || playerStats.GetCurrentEnergy() <= 0)
            return;

        if (canReceiveInput) { 
            isSuperAttackPressed = true;
            Attack();
        }
    }

    public void DashInput() {
        if (isStunned || playerStats.GetCurrentEnergy() <= 0)
            return;

        if (canReceiveInput && dashReady && !isDashing && !isFalling) {
            PlayAnimation(PlayerAnimStates.PLAYER_DASH);
        }
    }

    public void StopDashInput() {
        /*if (isDashing && dashTimer >= dashMinTime) {
            dashTimer = 0f;
            isFalling = true;
            isDashing = false;
            PlayAnimation(PlayerAnimStates.PLAYER_FALL);
        }
        else if (isDashing && dashTimer < dashMinTime)
            stopDash = true;*/
    }

    ///
    ///  WALK/RUN CODE ////////////////////////////////////////////////////////////////////////
    ///  
    private void WalkingToRunning() {
        if (!shiftKeyHeld)
            return;

        if (isWalking && !isRunning) {
            if (walkTimer < walkToRunTime) { 
                walkTimer += Time.deltaTime;
                anim.SetFloat("speedMultiplier", 1.5f + (walkTimer / walkToRunTime));
            }
            else if (walkTimer >= walkToRunTime) {
                isWalking = false;
                isRunning = true;
                anim.SetFloat("speedMultiplier", 1f);
            }
        }
    }

    private void ResetWalk() {
        walkTimer = 0f;
        isWalking = false;
        isRunning = false;
        anim.SetFloat("speedMultiplier", 1f);
    }

    private void ResetRun() {
        if (isRunning && playerStats.GetCurrentStamina() <= 0) { 
            isRunning = false;
            shiftKeyHeld = false;
            anim.SetFloat("speedMultiplier", 1f);
        }
    }

    IEnumerator StopTimer() {
        while (bufferTimer < stopBuffer && isStopped) { 
            bufferTimer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (bufferTimer >= stopBuffer) { 
            ResetWalk();
            walkTimer = 0f;
        }
    }

    public void ShiftToRun(bool value) {
        if (playerStats.GetCurrentStamina() > 0 && value)
            shiftKeyHeld = value;
        else if (playerStats.GetCurrentStamina() > 0 && !value)
            shiftKeyHeld = value;

        if (!shiftKeyHeld) {
            walkTimer = 0f;
            anim.SetFloat("speedMultiplier", 1f);
            isRunning = false;
        }
    }

    /// <summary>
    /// DASH CODE /////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void DashStarting() {
        // called at begining of dash animation event
        ResetWalk();
        isDashing = true;
        dashReady = false;
        isInvincible = true;
        canReceiveInput = false;
        UI.SetEnergyRecoverable(false);
        StartCoroutine(Dashing());
    }
    private void DashStartFalling() {
        // called when dash animation begins falling
        dashTimer = 0f;
        isFalling = true;
        isDashing = false;
        StartCoroutine(DashFall());
    }

    IEnumerator Dashing() {
        if (dashReady) { 
            dashReady = false;
        }

        while (!isFalling)
        {
            transform.localPosition = new Vector2(transform.localPosition.x,
                                        Mathf.Lerp(transform.localPosition.y,
                                        dashHeight,
                                        3f * Time.deltaTime));
            yield return Time.deltaTime;
        }
        yield break;

/*        if (dashTimer < dashMinTime) {
            dashTimer += Time.deltaTime;
            transform.localPosition = new Vector2(transform.localPosition.x,
                                        Mathf.Lerp(transform.localPosition.y,
                                        dashHeight,
                                        4f * Time.deltaTime));
        } else if (dashTimer >= dashMinTime) {
            dashTimer = dashMinTime;
        }

        if (stopDash && dashTimer >= dashMinTime || playerStats.GetCurrentEnergy() <= 0) {
            dashTimer = 0f;
            isFalling = true;
            isDashing = false;
            PlayAnimation(PlayerAnimStates.PLAYER_FALL);
        } 
        if (playerStats.GetCurrentEnergy() > 0)*/
    }

    IEnumerator DashFall() {
        while (transform.localPosition.y > 0)
        {
            transform.Translate(0, -4f * Time.deltaTime, 0f, transform.parent);
            yield return Time.deltaTime;
        }
        Stunned();
        isFalling = false;
        isLanding = true;
        isInvincible = false;
        transform.localPosition = Vector2.zero;

        PlayAnimation(PlayerAnimStates.PLAYER_LAND);
        StartCoroutine(ResetLanding(GetAnimationLength(PlayerAnimStates.PLAYER_LAND)));
        yield break;

/*        if (transform.localPosition.y > 0) {
            transform.Translate(0, -4f * Time.deltaTime, 0f, transform.parent);
        }
        else if (transform.localPosition.y <= 0) {
            dashTimer = 0f;
            Stunned();
            stopDash = false;
            isLanding = true;
            isFalling = false;
            transform.localPosition = Vector2.zero;

            PlayAnimation(PlayerAnimStates.PLAYER_LAND);
            Invoke("ResetLanding", GetAnimationLength(PlayerAnimStates.PLAYER_LAND));
        }*/
    }

    IEnumerator ResetLanding(float delay) {
        yield return new WaitForSeconds(delay);

        isStunned = false;
        isLanding = false;
        if (!isAttacking)
            canReceiveInput = true;
        StartCoroutine(ResetDash());
        yield break;
    }

    IEnumerator ResetDash() {
        yield return new WaitForSeconds(dashCooldown);

        dashReady = true;
        yield break;
    }

    /// <summary>
    /// ATTACK CODE ////////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void RunAttack() {
        if (!runAttackDash)
            return;
        else {
            rb.MovePosition(rb.position 
                + runDirection 
                * (walkSpeed + ((runSpeed - walkSpeed) / 2))
                * Time.fixedDeltaTime);
        }
    }

    IEnumerator RunAttackDashTime() {
        yield return new WaitForSeconds(runDashMaxTime);
        runAttackDash = false;
    }

    private void Attack() {
        // case for basic attacks
        if (isAttackPressed 
                    && canReceiveInput 
                    && attackCombo < 3) {
            attackCombo += 1;
            AttackAnimation(attackCombo);
            UI.SetStaminaRecoverable(false);

            isAttackPressed = false;
        }
        else if (isSuperAttackPressed 
                    && canReceiveInput 
                    && attackCombo == 2
                    && UI.GetCurrentEnergy() >= superAttackEnergy1) {
            attackCombo += 2;
            AttackAnimation(attackCombo);
            UI.SetEnergyRecoverable(false);
            UI.SetStaminaRecoverable(false);

            isSuperAttackPressed = false;
        }
        // case for run attack
        else if (isRunAttackPressed) {
            attackCombo += 1;
            AttackAnimation(attackCombo);
            UI.SetStaminaRecoverable(false);

            shiftKeyHeld = false;
            isRunAttackPressed = false;
        }
        // case for super attack
        else if (isSuperAttackPressed 
                    && canReceiveInput 
                    && attackCombo == 0 
                    && UI.GetCurrentEnergy() >= superAttackEnergy2) {
            AttackAnimation(10);
            UI.SetEnergyRecoverable(false);
            UI.SetStaminaRecoverable(false);

            isSuperAttackPressed = false;
        }
    }


    private void AttackAnimation(int attackIndex) {
        // stop movement when attacking and not running
        if (!isRunning || isSuperAttackPressed) { 
            runDirection = movement = Vector2.zero;
            runAttackDash = false;
            runDashTimer = 0f;
        }
        else {
            // save runDirection and start runAttackDash
            runDirection = new Vector2(xAxis, yAxis);
            runAttackDash = true;
            StartCoroutine(RunAttackDashTime());
        }

        isStopped = true;
        isAttacking = true;
        canReceiveInput = false;
        GetAttackDelay(attackIndex);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
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
        else if (attackIndex == 4) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_ATTACK4);
            PlayAnimation(PlayerAnimStates.PLAYER_ATTACK4);
        }
        else if (attackIndex == 10) {
            attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_SUPERATTACK);
            PlayAnimation(PlayerAnimStates.PLAYER_SUPERATTACK);
        }
        else
            return;
    }

    private void AttackHitboxActivated(float attackRange) {
        // called thru animation event
        Collider2D[] hitEnemies;
        if (facingLeft)  
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, 
                                (Vector2)attackPoint.position + (Vector2.left * attackRange), 
                                enemyLayer);
        else
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, 
                                (Vector2)attackPoint.position + (Vector2.right * attackRange), 
                                enemyLayer);

        foreach (Collider2D enemy in hitEnemies) {
            if (enemy.tag == "Enemy" && enemy.GetComponent<BasicEnemy>() != null) {
                enemy.GetComponent<BasicEnemy>().EnemyHurt((int)damage, 
                                                            pushbackDistance, 
                                                            transform);
            }
        }
    }

    private void SpecialAttackHitboxActivated(float attackRange) {
        Collider2D[] hitEnemies;
        if (facingLeft)
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.left * attackRange), enemyLayer);
        else
            hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRange), enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.tag == "Enemy") { 
                enemy.GetComponent<BasicEnemy>().EnemyHurt((int)specialAttackDmg, pushbackDistance * specialPushbackMultiplier, transform);
            }

        }
    }

    private void AttackFollowThrough(float multiplier) {
        // called thru animation event
        Vector2 newPosition;
        if (facingLeft) {
            newPosition = new Vector2(transform.position.x - (attackFollowThruDistance * multiplier),
                                        transform.position.y);
            rb.position = newPosition;
            //transform.position = newPosition;
        } else {
            newPosition = new Vector2(transform.position.x + (attackFollowThruDistance * multiplier), 
                                        transform.position.y);
            rb.MovePosition(newPosition);
            //transform.position = newPosition;
        }
    }

    IEnumerator ResetAttack(float delay) {
        yield return new WaitForSeconds(delay);
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

    private void ConsumeEnergy(int index) {
        // called thru animation events
        if (index == 0)
            playerStats.SetCurrentEnergy(-dashEnergy);
        else if (index == 1)
            playerStats.SetCurrentEnergy(-superAttackEnergy1);
        else if (index == 2)
            playerStats.SetCurrentEnergy(-superAttackEnergy2);
    }

    private void ConsumeStamina(int index) {
        // called thru animation events
        if (index == -2)
            playerStats.SetCurrentStamina(-runStamina);
        else if (index == -1)
            playerStats.SetCurrentStamina(-runAttackStamina);
        else if (index == 0)
            playerStats.SetCurrentStamina(-basicAttackStamina);
        else if (index == 1)
            playerStats.SetCurrentStamina(-superAttackStamina1);
        else if (index == 2)
            playerStats.SetCurrentStamina(-superAttackStamina2);
    }


    /// <summary>
    /// DAMAGED CODE ////////////////////////////////////////////////////////////////////
    /// </summary>
    public void PlayerHurt(int damageNum) {
        if (isHurt || isInvincible)
            return; 

        Stunned();
        isHurt = true;
        isInvincible = true;
        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(0f));

        // take damage
        playSounds.PlayPlayerHit();
        playerStats.SetCurrentHealth(-damageNum);

        if (playerStats.GetCurrentHealth() <= 0) {
            isDead = true;
            PlayAnimation(PlayerAnimStates.PLAYER_DEATH);
        }
        else { 
            stunDuration = GetAnimationLength(PlayerAnimStates.PLAYER_HURT);
            PlayAnimation(PlayerAnimStates.PLAYER_HURT);

            if (resetStunRoutine != null)
                StopCoroutine(resetStunRoutine);
            resetStunRoutine = StartCoroutine(ResetStun());
        }
    }

    IEnumerator ResetStun() {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    private void Stunned() {
        ResetWalk();
        isStunned = true;
        runAttackDash = false;
    }

    private void ResetInvincible() {
        if (isDashing || isFalling)
            return;
        else
            isInvincible = false;
    }

    public bool GetInvincible() {
        return isInvincible;
    }

    private void DamageFlash() {
        if (!isHurt)
            return;

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
            ResetInvincible();
            sp.enabled = true;

            isHurtTimer = 0;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRangeVisualizer));
    }
}
