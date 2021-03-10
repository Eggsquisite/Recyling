using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    enum PlayerWeapon
    {
        Sword = 1,
        Blaster = 2
    }

    [Header("Components")]
    // used for player sprite movement/animations
    [SerializeField]
    private Transform playerSprite;
    [SerializeField]
    private SpriteRenderer sp;

    private Animator anim;
    private Rigidbody2D rb;
    private Projectile projectile;
    private RuntimeAnimatorController ac;

    [Header("Player Stats")]
    private PlayerSounds playSounds;
    private PlayerUI UI;
    private bool isDead;
    private PlayerWeapon playerEquipment;

    [Header("Movement Properties")]
    [SerializeField]
    private float baseWalkSpeed;
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
    private float currentWalkSpeed;
    private bool shiftKeyHeld;
    private bool stopMovement;
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

    [Header("Healing Properties")]
    [SerializeField]
    private float healAmount;

    private bool isHealing;
    private Coroutine healRoutine, healthDelayRoutine;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Attack Properties")]
    [SerializeField]
    private int swordDamage;
    [SerializeField]
    private int specialAttackDmg;
    [SerializeField]
    private int blasterLightDmg;
    [SerializeField]
    private int blasterHeavyDmg;
    [SerializeField] 
    private float attackRangeVisualizer;
    [SerializeField]
    private float pushbackDistance;
    [SerializeField]
    private float specialPushbackMultiplier;
    [SerializeField]
    private float attackFollowThruDistance;

    private float attackDelay;
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
    private bool dashReady = true;

    [Header("Teleport Properties")]
    [SerializeField]
    private float teleportMoveSpeed;
    [SerializeField]
    private float teleportCooldown;

    private bool isTeleporting;
    private bool teleportReady = true;

    private Vector2 teleportDirection;

    [Header("Stamina Properties")]
    [SerializeField]
    private int dashStamina;
    [SerializeField]
    private int runStamina;
    [SerializeField]
    private int runAttackStamina;
    [SerializeField]
    private int basicAttackStamina;
    [SerializeField]
    private int superAttackStamina1;
    [SerializeField]
    private int superAttackStamina2;
    [SerializeField]
    private int blasterLightStamina;
    [SerializeField]
    private int blasterHeavyStamina;

    [Header("Energy Properties")]
    [SerializeField]
    private float energyRegenOnHit;
    [SerializeField]
    private int blasterLightEnergy;
    [SerializeField]
    private int blasterHeavyEnergy;
    [SerializeField]
    private int superAttackEnergy1;
    [SerializeField]
    private int superAttackEnergy2;

    // Start is called before the first frame update
    void Awake()
    {
        if (UI == null) UI = GetComponent<PlayerUI>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
        if (projectile == null) projectile = GetComponent<Projectile>();
        if (playSounds == null) playSounds = GetComponent<PlayerSounds>();

        currentWalkSpeed = baseWalkSpeed;
        projectile.SetDamage(blasterLightDmg);
        projectile.SetSpecialDamage(blasterHeavyDmg);
        ac = anim.runtimeAnimatorController;
        playerEquipment = PlayerWeapon.Sword;
        runDashMaxTime = GetAnimationLength(PlayerAnimStates.PLAYER_RUNATTACK);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        ResetRun();
        StopHeal();
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
    public void ReplayAnimation(string newAnim)
    {
        AnimHelper.ReplayAnimation(anim, ref currentState, newAnim);
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
        if (stopMovement) {
            ResetWalk();
            movement = Vector2.zero;
        }
        else if (isTeleporting)
            movement = teleportDirection;
        else
            movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);

        if (!isTeleporting)
            CheckDirection();
        if (!isAttacking) {
            if (isWalking && !isDashing && !isTeleporting)
                rb.MovePosition(rb.position + movement * currentWalkSpeed * Time.fixedDeltaTime);
            else if (isRunning)
                rb.MovePosition(rb.position + movement * runSpeed * Time.fixedDeltaTime);
            else if (isDashing)
                rb.MovePosition(rb.position + movement * dashMoveSpeed * Time.fixedDeltaTime);
            else if (isTeleporting)
                rb.MovePosition(rb.position + movement * teleportMoveSpeed * Time.fixedDeltaTime);
        }
    }
    private void MovementAnimation() {
        // attack animations override run/idle anims
        if (!isAttacking && !isDashing && !isFalling && !isLanding && !isTeleporting)
        {
            if (playerEquipment == PlayerWeapon.Sword) 
            { 
                if (xAxis == 0 && yAxis == 0)
                    PlayAnimation(PlayerAnimStates.PLAYER_IDLE);
                else if (isWalking)
                    PlayAnimation(PlayerAnimStates.PLAYER_WALK);
                else if (isRunning)
                    PlayAnimation(PlayerAnimStates.PLAYER_RUN);
            }
            else if (playerEquipment == PlayerWeapon.Blaster)
            {
                if (xAxis == 0 && yAxis == 0)
                    PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_IDLE);
                else if (isWalking)
                    PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_WALK);
                else if (isRunning)
                    PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_RUN);
            }
        }
    }

    public void SetStopMovement(bool flag) {
        stopMovement = flag;
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

    public void SwitchWeaponInput() {
        if (isStunned || isAttacking || isDashing || isFalling || isLanding || isHealing)
            return;

        if (playerEquipment == PlayerWeapon.Blaster)
            playerEquipment = PlayerWeapon.Sword;
        else if (playerEquipment == PlayerWeapon.Sword)
            playerEquipment = PlayerWeapon.Blaster;
    }

    public void BlasterAttackInput() {
        if (isStunned || UI.GetCurrentEnergy() <= 0)
            return;

        if (canReceiveInput && !isHealing)
            BlasterAttack();
    }

    public void SuperBlasterAttackInput() {
        if (isStunned || UI.GetCurrentEnergy() <= 0)
            return;

        if (canReceiveInput && !isHealing)
            SuperBlasterAttack();
    }

    public void BasicAttackInput() {
        if (isStunned || UI.GetCurrentStamina() <= 0)
            return; 

        if (canReceiveInput && !isHealing) {
            if (!isRunning)
                isAttackPressed = true;
            else 
                isRunAttackPressed = true;

            Attack();
        }
    }

    public void SuperAttackInput() {
        if (isStunned 
            || UI.GetCurrentStamina() <= 0 
            || UI.GetCurrentEnergy() <= 0)
            return;

        if (canReceiveInput && !isHealing) { 
            isSuperAttackPressed = true;
            Attack();
        }
    }

    public void DashInput() {
        if (isStunned || UI.GetCurrentStamina() <= 0)
            return;

        if (canReceiveInput && !isHealing)
        {
            if (dashReady && !isDashing && !isFalling && playerEquipment == PlayerWeapon.Sword)
                PlayAnimation(PlayerAnimStates.PLAYER_DASH);
            else if (teleportReady && !isTeleporting && playerEquipment == PlayerWeapon.Blaster)
                PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_TELEPORT);
        }
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
                anim.SetFloat("speedMultiplier", 1.25f + (walkTimer / walkToRunTime));
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
        if (isRunning && UI.GetCurrentStamina() <= 0) { 
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
        if (UI.GetCurrentStamina() > 0 && value && !isHealing)
            shiftKeyHeld = value;
        else if (UI.GetCurrentStamina() > 0 && !value)
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
        isAttacking = false;
        isInvincible = true;
        canReceiveInput = false;
        UI.SetStaminaRecoverable(false);
        StartCoroutine(Dashing());
    }
    private void DashStartFalling() {
        // called when dash animation begins falling
        //dashTimer = 0f;
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
            playerSprite.localPosition = new Vector2(playerSprite.localPosition.x,
                                        Mathf.Lerp(playerSprite.localPosition.y,
                                        dashHeight,
                                        3f * Time.deltaTime));
            yield return Time.deltaTime;
        }
        yield break;
    }

    IEnumerator DashFall() {
        while (playerSprite.localPosition.y > 0)
        {
            playerSprite.Translate(0, -4f * Time.deltaTime, 0f, transform.parent);
            yield return Time.deltaTime;
        }
        Stunned();
        isFalling = false;
        isLanding = true;
        isInvincible = false;
        playerSprite.localPosition = Vector2.zero;

        PlayAnimation(PlayerAnimStates.PLAYER_LAND);
        StartCoroutine(ResetLanding(GetAnimationLength(PlayerAnimStates.PLAYER_LAND)));
        yield break;
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

    /// TELEPORT CODE //////////////////////////////////////////////////////////////////////////
    /// 
    private void TeleportStarting() {
        // called at begining of teleport animation event
        ResetWalk();
        isInvincible = true;
        isAttacking = false;
        isTeleporting = true;
        teleportReady = false;
        canReceiveInput = false;
        UI.SetStaminaRecoverable(false);

        if (xAxis == 0 && facingLeft)
            teleportDirection = new Vector2(-1f, 0f);
        else if (xAxis == 0 && !facingLeft)
            teleportDirection = new Vector2(1f, 0f);
        else
            teleportDirection = new Vector2(xAxis, 0f);
    }

    private void TeleportEnding() {
        // called at end of teleport
        Stunned();
        isInvincible = false;
        isTeleporting = false;
        teleportDirection = Vector2.zero;
        StartCoroutine(TeleportLanding());
    }

    IEnumerator TeleportLanding() {
        yield return new WaitForSeconds(0.1f);

        isStunned = false;
        if (!isAttacking)
            canReceiveInput = true;
        StartCoroutine(ResetTeleport());
    }

    IEnumerator ResetTeleport() {
        yield return new WaitForSeconds(teleportCooldown);

        teleportReady = true;
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
                * (currentWalkSpeed + ((runSpeed - currentWalkSpeed) / 2))
                * Time.fixedDeltaTime);
        }
    }

    IEnumerator RunAttackDashTime() {
        yield return new WaitForSeconds(runDashMaxTime);
        runAttackDash = false;
    }

    private void Attack() {
        // case for basic attacks
        if (isAttackPressed && canReceiveInput) {
            if (attackCombo < 3)
                attackCombo += 1;
            else if (attackCombo == 3)
                attackCombo = 1;

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
            //runDashTimer = 0f;
        }
        else {
            // save runDirection and start runAttackDash
            runDirection = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
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
                var tmp = enemy.GetComponent<BasicEnemy>();
                tmp.EnemyHurt(swordDamage, pushbackDistance);
                StartCoroutine(UI.EnergyRegenOnHit(energyRegenOnHit, tmp.GetEnergyGainMultiplier()));
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
            if (enemy.tag == "Enemy" && enemy.GetComponent<BasicEnemy>() != null) { 
                enemy.GetComponent<BasicEnemy>().EnemyHurt(specialAttackDmg, pushbackDistance * specialPushbackMultiplier);
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
        if (!isDashing && !isFalling && !isHealing && !isTeleporting)
            canReceiveInput = true;
    }

    private void ComboInput() {
        // called during attack animation, allows input such as combos or dodging 
        if (isAttacking && attackCombo > 0) 
            canReceiveInput = true;
        else
            return;
    }

    /// BLASTER ATTACK CODE //////////////////////////////////////////////////////////////////////
    /// 

    private void BlasterAttack() {
        isStopped = true;
        isAttacking = true;
        canReceiveInput = false;
        attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_BLASTER_LIGHT);
        if (PlayerAnimStates.PLAYER_BLASTER_LIGHT == currentState)
            ReplayAnimation(PlayerAnimStates.PLAYER_BLASTER_LIGHT);
        else
            PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_LIGHT);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
    }

    private void SuperBlasterAttack() {
        isStopped = true;
        isAttacking = true;
        canReceiveInput = false;
        attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_BLASTER_HEAVY);
        PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_HEAVY);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
    }

    private void ConsumeEnergy(int index) {
        // called thru animation events
        if (index == -1)
            UI.SetCurrentEnergy(-blasterLightEnergy);
        else if (index == 0)
            UI.SetCurrentEnergy(-blasterHeavyEnergy);
        else if (index == 1)
            UI.SetCurrentEnergy(-superAttackEnergy1);
        else if (index == 2)
            UI.SetCurrentEnergy(-superAttackEnergy2);
    }

    private void ConsumeStamina(int index) {
        // called thru animation events
        if (index == -3)
            UI.SetCurrentStamina(-dashStamina);
        else if (index == -2)
            UI.StaminaWithoutDecay(-runStamina);
        else if (index == -1)
            UI.SetCurrentStamina(-runAttackStamina);
        else if (index == 0)
            UI.SetCurrentStamina(-basicAttackStamina);
        else if (index == 1)
            UI.SetCurrentStamina(-superAttackStamina1);
        else if (index == 2)
            UI.SetCurrentStamina(-superAttackStamina2);
        else if (index == 3)
            UI.SetCurrentStamina(-blasterLightStamina);
        else if (index == 4)
            UI.SetCurrentStamina(-blasterHeavyStamina);
    }

    public int GetPlayerWeapon() {
        return (int)playerEquipment;
    }

    /// <summary>
    /// DAMAGED CODE ////////////////////////////////////////////////////////////////////
    /// </summary>
    public void PlayerHurt(int damageNum) {
        if (isHurt || isInvincible)
            return;

        if (isDashing)
            ReplayAnimation(PlayerAnimStates.PLAYER_DASH);

        Stunned();
        StopRecoverInput();

        isHurt = true;
        isInvincible = true;
        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(0f));

        // take damage
        playSounds.PlayPlayerHit();
        UI.SetCurrentHealth(-damageNum);

        if (UI.GetCurrentHealth() <= 0) {
            isDead = true;
            PlayAnimation(PlayerAnimStates.PLAYER_DEATH);
        }
        else { 
            stunDuration = GetAnimationLength(PlayerAnimStates.PLAYER_HURT);
            PlayAnimation(PlayerAnimStates.PLAYER_HURT);

            if (resetStunRoutine != null)
                StopCoroutine(resetStunRoutine);
            resetStunRoutine = StartCoroutine(ResetStun(stunDuration));
        }
    }

    IEnumerator ResetStun(float delay) {
        yield return new WaitForSeconds(delay);
        isStunned = false;
    }

    public void Stunned() {
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

    public void SetInvincible(bool flag) {
        isInvincible = flag;
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

    /// <summary>
    /// HEAL CODE ///////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    public void RecoverInput() {
        if (isHealing || isStunned || isDashing || isFalling || isAttacking
                || UI.GetCurrentEnergy() <= 0
                || UI.GetCurrentHealth() >= UI.GetHealthMaxValue()) { 
            return;
        }

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        isHealing = true;
        canReceiveInput = false;
        healRoutine = StartCoroutine(HealthRecovery());
    }

    public void StopRecoverInput() {
        isHealing = false;
        currentWalkSpeed = baseWalkSpeed;
        Camera.main.GetComponent<CameraFollow>().SetIsFocused(false);

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        if (healthDelayRoutine != null)
            StopCoroutine(healthDelayRoutine);
        healthDelayRoutine = StartCoroutine(HealthRecoveryDelay());
    }

    private void StopHeal() {
        if (!isHealing)
            return;

        if (UI.GetCurrentHealth() >= UI.GetHealthMaxValue() || UI.GetCurrentEnergy() <= 0) {
            StopRecoverInput();
        }
    }

    IEnumerator HealthRecovery() {
        currentWalkSpeed = 0.25f;
        Camera.main.GetComponent<CameraFollow>().SetIsFocused(true);
        while (isHealing || UI.GetCurrentHealth() < UI.GetHealthMaxValue())
        {
            UI.SetCurrentHealth(healAmount * 1.25f);
            UI.EnergyWithoutDecay(-healAmount);
            yield return new WaitForSeconds(0.03f);
        }
        StopRecoverInput();
    }

    IEnumerator HealthRecoveryDelay() {
        yield return new WaitForSeconds(1f);
        if (!isAttacking && !isDashing && !isFalling && !isTeleporting)
            canReceiveInput = true;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRangeVisualizer));
    }
}
