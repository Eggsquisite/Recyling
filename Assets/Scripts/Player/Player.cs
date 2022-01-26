using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    public static Player instance;

    enum PlayerWeapon
    {
        Sword = 1,
        Blaster = 2,
    }

    [Header("Components")]
    // used for player sprite movement/animations
    [SerializeField]
    private Transform playerSprite;
    [SerializeField]
    private SpriteRenderer sp;
    [SerializeField]
    private ParticleSystem ps;

    private Animator anim;
    private Rigidbody2D rb;
    private Projectile projectile;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;
    private RuntimeAnimatorController ac;

    [Header("Player Stats")]
    private PlayerUI UI;
    private PlayerStats playerStats;
    private PlayerSounds playSounds;
    private PlayerUpgrades playerUpgrades;

    private bool isDead;
    private PlayerWeapon playerEquipment;

    [Header("Player Upgrade Levels")]
    private int strengthUpgradeLevel;
    private int specialUpgradeLevel;
    private int focusUpgradeLevel;
    private int vitalityUpgradeLevel;
    private int staminaUpgradeLevel;

    private bool jetpackDamageFlag;

    [Header("Movement Properties")]
    [SerializeField]
    private float baseWalkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float healWalkSpeedMultiplier;
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
    [SerializeField]
    private float hurtPushBackSpeed;

    private bool isHurt;
    private bool isStunned;
    private bool isInvincible;
    private float stunDuration;

    private Coroutine pushBackMovementRoutine, pushBackDurationRoutine, isHurtRoutine;

    [Header ("Upgrade Properties")]
    private bool focusUpgrade;

    // flag to check if vitality boost was granted to avoid duplicate boosts
    private bool vitalityUpgradeGranted;

    // flag to check if deathResist upgrade is enabled and if it is currently available
    private bool deathResistFlag;
    private bool deathResistEnabled;

    [Header("Healing Properties")]
    private bool isHealing;
    private Coroutine healRoutine, healthDelayRoutine;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Attack Properties")]
    [SerializeField] 
    private float attackRangeVisualizer;
    [SerializeField] [Tooltip("Begins when an input is executed; that value will stay true until the buffer runs out")]
    private float attackBuffer;
    [SerializeField]
    private float pushbackDistance;
    [SerializeField]
    private float attackFollowDistance;
    [SerializeField]
    private float attackFollowSpeed;

    private float attackDelay;
    private float runDashMaxTime;

    private int attackCombo;
    private int swordDamage;
    private int specialAttackDmg;
    private int blasterLightDmg;
    private int blasterHeavyDmg;

    private bool isAttacking;
    private bool runAttackDash;
    private bool isAttackPressed;
    private bool isRunAttackPressed;
    private bool isSuperAttackPressed;
    private bool isBlasterAttackPressed;
    private bool isBlasterSuperAttackPressed;
    private bool canReceiveInput = true;
    private float specialPushbackMultiplier = 5f;
    private Vector2 runDirection;

    private Coroutine attackBufferRoutine, dodgeBufferRoutine;
    private Coroutine resetStunRoutine, resetAttackRoutine, attackFollowRoutine;

    [Header("Dash Properties")]
    [SerializeField]
    private float dodgeBuffer;
    [SerializeField]
    private float dashMoveSpeed;
    [SerializeField]
    private float dashCooldown;
    [SerializeField]
    private float dashHeight;
    [SerializeField]
    private float dashFallSpeed;

    private bool isDashing;
    private bool isFalling;
    private bool isLanding;
    private bool dashReady = true;
    private bool dodgeInputPressed;

    [Header("Teleport Properties")]
    [SerializeField]
    private float teleportMoveSpeed;
    [SerializeField]
    private float teleportCooldown;

    private bool isTeleporting;
    private bool teleportReady = true;

    private Vector2 teleportDirection;

    [Header("Stamina Consumption Properties")]
    [SerializeField]
    private int dodgeStamina;
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

    [Header("Energy Consumption Properties")]
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
        // low key singleton
        instance = this;

        if (UI == null) UI = GetComponent<PlayerUI>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
        if (projectile == null) projectile = GetComponent<Projectile>();
        if (playSounds == null) playSounds = GetComponent<PlayerSounds>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
        if (playerUpgrades == null) playerUpgrades = GetComponent<PlayerUpgrades>();
        //if (ps != null) ps.gameObject.SetActive(false);   code for particle system, will probably not use does not fit art style
        if (shaderGUItext == null) shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = sp.material.shader;

        sp.enabled = true;
        currentWalkSpeed = baseWalkSpeed;
        ac = anim.runtimeAnimatorController;

        playerEquipment = PlayerWeapon.Sword;
        projectile.SetDamage(blasterLightDmg);
        projectile.SetSpecialDamage(blasterHeavyDmg);

        stunDuration = GetAnimationLength(PlayerAnimStates.PLAYER_HURT);
        runDashMaxTime = GetAnimationLength(PlayerAnimStates.PLAYER_RUNATTACK);
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        Dodge();
        Attack();
        ResetRun();
        StopHeal();
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

        if (!isTeleporting && !isStunned)
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
        if (isStunned || isHealing || UI.GetCurrentEnergy() <= blasterLightEnergy)
            return;

        isBlasterAttackPressed = true;

        if (attackBufferRoutine != null)
            StopCoroutine(attackBufferRoutine);
        attackBufferRoutine = StartCoroutine(AttackBuffer(3));
        //BlasterAttack();
    }

    public void SuperBlasterAttackInput() {
        if (isStunned || isHealing || UI.GetCurrentEnergy() <= blasterHeavyEnergy)
            return;

        isBlasterSuperAttackPressed = true;

        if (attackBufferRoutine != null)
            StopCoroutine(attackBufferRoutine);
        attackBufferRoutine = StartCoroutine(AttackBuffer(4));
        //SuperBlasterAttack();
    }

    public void BasicAttackInput() {
        if (isStunned || isHealing || UI.GetCurrentStamina() <= 0)
            return; 

        if (!isRunning)
            isAttackPressed = true;
        else 
            isRunAttackPressed = true;

        if (attackBufferRoutine != null)
            StopCoroutine(attackBufferRoutine);
        attackBufferRoutine = StartCoroutine(AttackBuffer(1));
        //Attack();
    }

    public void SuperAttackInput() {
        if (isStunned
            || isHealing
            || UI.GetCurrentStamina() <= 0 
            || UI.GetCurrentEnergy() <= 0)
            return;

        isSuperAttackPressed = true;

        if (attackBufferRoutine != null)
            StopCoroutine(attackBufferRoutine);
        attackBufferRoutine = StartCoroutine(AttackBuffer(2));
        //Attack();
    }

    public void DodgeInput() {
        if (isHurt || UI.GetCurrentStamina() <= 0)
            return;

        if (!isHealing) {
            dodgeInputPressed = true;

            if (dodgeBufferRoutine != null)
                StopCoroutine(dodgeBufferRoutine);
            dodgeBufferRoutine = StartCoroutine(DodgeBuffer());
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

    /// DODGE CODE //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// DASH CODE /////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void Dodge() {
        if (dodgeInputPressed && canReceiveInput) { 
            SetInvincible(true);
            if (dashReady && !isDashing && !isFalling && playerEquipment == PlayerWeapon.Sword)
                PlayAnimation(PlayerAnimStates.PLAYER_DASH);
            else if (teleportReady && !isTeleporting && playerEquipment == PlayerWeapon.Blaster)
                PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_TELEPORT);
        }
    }

     private IEnumerator DodgeBuffer() {
        yield return new WaitForSeconds(dodgeBuffer);

        if (dodgeInputPressed)
            dodgeInputPressed = false;
    }

    private void DashStarting() {
        // called at begining of dash animation event
        ResetWalk();
        isDashing = true;
        dashReady = false;
        isAttacking = false;
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
            playerSprite.Translate(0, -dashFallSpeed * Time.deltaTime, 0f, transform.parent);
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
    /// ATTACK CODE ///////////////////////////////////////////////////////////////////////////////////////////
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
        if (!canReceiveInput)
            return;

        // case for basic attacks
        if (isAttackPressed) {
            if (attackCombo < 3)
                attackCombo += 1;
            else if (attackCombo == 3)
                attackCombo = 1;

            AttackAnimation(attackCombo);
            UI.SetStaminaRecoverable(false);

            isAttackPressed = false;
        }
        // case for smaller super attack
        else if (isSuperAttackPressed
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
        // case for BIG super attack
        else if (isSuperAttackPressed
                    && attackCombo == 0
                    && UI.GetCurrentEnergy() >= superAttackEnergy2) {
            AttackAnimation(10);
            UI.SetEnergyRecoverable(false);
            UI.SetStaminaRecoverable(false);

            isSuperAttackPressed = false;
        } 
        // case for blaster light attack
        else if (isBlasterAttackPressed)
        {
            BlasterAttack();
        }
        // case for blaster super attack
        else if (isBlasterSuperAttackPressed)
        {
            SuperBlasterAttack();
        }
    }

    private IEnumerator AttackBuffer(int index) {
        // depending on which attack is pressed, set the other attack false
        if (index == 1 && isSuperAttackPressed)
            isSuperAttackPressed = false;
        else if (index == 2 && isAttackPressed)
            isAttackPressed = false;
        // cases for BLASTER
        else if (index == 3 && isBlasterSuperAttackPressed)
            isBlasterSuperAttackPressed = false;
        else if (index == 4 && isBlasterAttackPressed)
            isBlasterAttackPressed = false;

        // isAttackPressed or isSuperAttackPressed stays true until the buffer runs out
        yield return new WaitForSeconds(attackBuffer);

        if (index == 1 && isAttackPressed)
            isAttackPressed = false;
        else if (index == 2 && isSuperAttackPressed)
            isSuperAttackPressed = false;
        else if (index == 3 && isBlasterAttackPressed)
            isBlasterAttackPressed = false;
        else if (index == 4 && isBlasterSuperAttackPressed)
            isBlasterSuperAttackPressed = false;
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


    /// <summary>
    /// Regain energy on hitting an enemy
    /// If focusUpgrade is true (Focus upgrade 2 is unlocked), then heal health on hit as well
    /// </summary>
    /// <param name="gainMultiplier"></param>
    public void RegainEnergy(float gainMultiplier) {
        if (focusUpgrade) { 
            StartCoroutine(UI.EnergyRegenOnHit(gainMultiplier));
            StartCoroutine(UI.HealthRegenOnHit(gainMultiplier));
        }
        else
            StartCoroutine(UI.EnergyRegenOnHit(gainMultiplier));
    }

    /// <summary>
    /// Attack for when jetpack attack is set to true
    /// </summary>
    private void JetpackHitboxActivated() {
        if (!jetpackDamageFlag)
            return;

        Debug.Log("Activating jetpack damage");
        Collider2D[] hitEnemies;
        hitEnemies = Physics2D.OverlapCircleAll(transform.position, 1f);

        foreach (Collider2D enemy in hitEnemies) {
            if (enemy.tag == "Enemy" && enemy.GetComponent<BasicEnemy>() != null) {
                var tmp = enemy.GetComponent<BasicEnemy>();
                tmp.EnemyHurt(swordDamage, pushbackDistance, 0);
            }
        }
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
                tmp.EnemyHurt(swordDamage, pushbackDistance, 0);
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
                enemy.GetComponent<BasicEnemy>().EnemyHurt(specialAttackDmg, pushbackDistance * specialPushbackMultiplier, 0);
            }
        }
    }

    private void AttackFollowThrough(float multiplier) {
        // called thru animation event
        Vector2 newPosition;
        newPosition = new Vector2(attackFollowDistance * multiplier, 0f);

        if (attackFollowRoutine != null)
            StopCoroutine(attackFollowRoutine);
        attackFollowRoutine = StartCoroutine(AttackFollowMovement(newPosition));
    }

    IEnumerator AttackFollowMovement(Vector2 position) { 
        while (true != false)
        {
            if (facingLeft)
                rb.MovePosition(rb.position - position * attackFollowSpeed * Time.fixedDeltaTime);
            else
                rb.MovePosition(rb.position + position * attackFollowSpeed * Time.fixedDeltaTime);

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }

    private void StopAttackFollowThrough() {
        // called thru animation event
        if (attackFollowRoutine != null)
            StopCoroutine(attackFollowRoutine);
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
    }

    public void SetSwordDamage(float newValue) {
        swordDamage = Mathf.RoundToInt(newValue);
    }
    public void SetSpecialAttackDmg(float newValue) {
        specialAttackDmg = Mathf.RoundToInt(newValue);
    }
    public void SetBlasterLightDmg(float newValue) {
        blasterLightDmg = Mathf.RoundToInt(newValue);
        projectile.SetDamage(blasterLightDmg);
    }
    public void SetBlasterHeavyDmg(float newValue) {
        blasterHeavyDmg = Mathf.RoundToInt(newValue);
        projectile.SetSpecialDamage(blasterHeavyDmg);
    }

    ///
    /// BLASTER ATTACK CODE //////////////////////////////////////////////////////////////////////
    /// 

    private void BlasterAttack() {
        isStopped = true;
        isAttacking = true;
        canReceiveInput = false;
        isBlasterAttackPressed = false;
        attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_BLASTER_LIGHT);
        if (PlayerAnimStates.PLAYER_BLASTER_LIGHT == currentState)
            ReplayAnimation(PlayerAnimStates.PLAYER_BLASTER_LIGHT);
        else
            PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_LIGHT);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        // set attack delay slightly longer than animation to prevent spam shooting and make each shot 
        // more powerful
        resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay + 0.15f));
    }

    private void SuperBlasterAttack() {
        isStopped = true;
        isAttacking = true;
        canReceiveInput = false;
        isBlasterSuperAttackPressed = false;
        attackDelay = GetAnimationLength(PlayerAnimStates.PLAYER_BLASTER_HEAVY);
        if (PlayerAnimStates.PLAYER_BLASTER_HEAVY == currentState)
            ReplayAnimation(PlayerAnimStates.PLAYER_BLASTER_HEAVY);
        else
            PlayAnimation(PlayerAnimStates.PLAYER_BLASTER_HEAVY);

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay - 0.02f));
    }


    // CONSUMPTION CODE ////////////////////////////////////////////////////////////////////////////////
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
            UI.SetCurrentStamina(-dodgeStamina);
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
    public void PlayerHurt(int damageNum, float pushDistance, Vector2 reference) {
        if (isHurt || isInvincible || isDead)
            return;

        if (isDashing)
            ReplayAnimation(PlayerAnimStates.PLAYER_DASH);

        Stunned();
        StopAttackFollowThrough();
        GameManager.instance.BeginCameraShake(stunDuration, 0.05f);
        PushBack(pushDistance, reference);

        // if hurt and attacking, do something
        if (isAttacking)
            UI.SetCurrentStamina(-0.1f);

        isHurt = true;
        isInvincible = true;
        isAttackPressed = false;
        dodgeInputPressed = false;
        isSuperAttackPressed = false;

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        resetAttackRoutine = StartCoroutine(ResetAttack(0f));

        if (isHurtRoutine != null)
            StopCoroutine(isHurtRoutine);
        isHurtRoutine = StartCoroutine(BeginIsHurtTimer());

        // take damage
        playSounds.PlayPlayerHit();
        UI.StopFutureHealthRecovery();
        //UI.SetCurrentHealth(-damageNum);

        if (UI.GetCurrentHealth() - damageNum <= 0 && deathResistEnabled && deathResistFlag) {
            // play some guardian angel FX breaking
            Debug.Log("Death Resist used up");

            UI.SetCurrentHealth(UI.GetHealthMaxValue() * 0.01f);
            deathResistFlag = false;
        } else {
            UI.SetCurrentHealth(-damageNum);
        }

        if (UI.GetCurrentHealth() <= 0) {
            Death();
        }
        else { 
            PlayAnimation(PlayerAnimStates.PLAYER_HURT);
            StartResetStunRoutine(stunDuration);
        }
    }

    private void Death() {
        isDead = true;
        PlayAnimation(PlayerAnimStates.PLAYER_DEATH);

        // find a way to place dead body
    }

    private void BeginRespawn() {
        // called at the end of the Player_Death Animation
        GameManager.instance.BeginRespawn(GetCurrency());
        UI.SetCurrency(-GetCurrency());
    }

    public void Respawn() {
        isDead = false;
        ChangeSpriteColor(0);
        PlayAnimation(PlayerAnimStates.PLAYER_RESPAWN);

        StartResetStunRoutine(1f);
    }

    public void RefreshResources() {
        playerStats.RefreshResources();

        if (deathResistEnabled)
            deathResistFlag = true;
    }

    public bool GetIsDead() {
        return isDead;
    }

    /// <summary>
    /// Pushes back player a set distance when hurt
    /// </summary>
    private void PushBack(float distance, Vector2 reference) {
        Vector2 newPosition;
        if (reference.x > rb.position.x)
        {
            newPosition = new Vector2(-distance, 0f);
            //facingLeft = false;
            //transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);

            if (pushBackDurationRoutine != null)
                StopCoroutine(pushBackDurationRoutine);
            pushBackDurationRoutine = StartCoroutine(PushBackDuration(newPosition, 0.1f));
        }
        else if (reference.x <= transform.position.x)
        {
            newPosition = new Vector2(distance, 0f);
            //facingLeft = true;
            //transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);

            if (pushBackDurationRoutine != null)
                StopCoroutine(pushBackDurationRoutine);
            pushBackDurationRoutine = StartCoroutine(PushBackDuration(newPosition, 0.1f));
        }
    }

    IEnumerator PushBackDuration(Vector2 position, float duration)
    {
        if (pushBackMovementRoutine != null)
            StopCoroutine(pushBackMovementRoutine);
        pushBackMovementRoutine = StartCoroutine(PushBackMovement(position));

        yield return new WaitForSeconds(duration);

        if (pushBackMovementRoutine != null)
            StopCoroutine(pushBackMovementRoutine);
        yield break;
    }

    IEnumerator PushBackMovement(Vector2 position) {
        while (true != false)
        {
            rb.MovePosition(rb.position + position * hurtPushBackSpeed * Time.fixedDeltaTime);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
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

    public void StartResetStunRoutine(float delay) {
        if (resetStunRoutine != null)
            StopCoroutine(resetStunRoutine);
        resetStunRoutine = StartCoroutine(ResetStun(delay));
    }

    private void ResetInvincible() {
        if (isDashing || isFalling || LoadArea.isLoading)
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

    private IEnumerator BeginIsHurtTimer() {
        yield return new WaitForSeconds(isHurtMaxTime);
        
        isHurt = false;
        sp.enabled = true;
        ResetInvincible();
    }

    /// <summary>
    /// HEAL CODE ///////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    public void RecoverInput() {
        if (isHealing || isStunned || isDashing || isFalling || isAttacking 
                || UI.GetCurrentEnergy() <= 0
                || UI.GetCurrentHealth() >= UI.GetHealthMaxValue()
                || UI.GetFutureHealth() >= UI.GetHealthMaxValue()) { 
            return;
        }

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        isHealing = true;
        canReceiveInput = false;
        healRoutine = StartCoroutine(HealthRecovery());
    }

    public void StopRecoverInput() {
        if (!isHealing)
            return;

        isHealing = false;
        ChangeSpriteColor(0);
        currentWalkSpeed = baseWalkSpeed;
        anim.SetFloat("speedMultiplier", 1f);
        Camera.main.GetComponent<CameraFollow>().SetIsFocused(false);

        if (!isHurt)
            UI.BeginFutureHealthRecovery();

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        if (healthDelayRoutine != null)
            StopCoroutine(healthDelayRoutine);
        healthDelayRoutine = StartCoroutine(HealthRecoveryDelay());
    }

    private void StopHeal() {
        if (!isHealing)
            return;

        if (isHurt
            || UI.GetHealthDecaying()
            || UI.GetCurrentHealth() >= UI.GetHealthMaxValue()
            || UI.GetFutureHealth() >= UI.GetHealthMaxValue()
            || UI.GetCurrentEnergy() <= 0) {
            StopRecoverInput();
        }
    }

    IEnumerator HealthRecovery() {
        var time = 0f;
        //if (ps != null) ps.gameObject.SetActive(true);

        ChangeSpriteColor(1);
        currentWalkSpeed *= healWalkSpeedMultiplier;
        anim.SetFloat("speedMultiplier", 0.5f);

        Camera.main.GetComponent<CameraFollow>().SetIsFocused(true);
        while (isHealing || UI.GetCurrentHealth() < UI.GetHealthMaxValue() || isHurt)
        {
            // consume energy to heal an equal amount (multiplied by energyToHealthMultiplier)
            UI.EnergyWithoutDecay(-playerStats.GetHealthRecoveryValue());
            // increase heal amount the longer recovery is held
            if (time < 0.15f)
                UI.SetFutureHealth(1f);
            else if (time >= 0.15f && time < 1.25f)
                UI.SetFutureHealth(1.25f);
            else if (time >= 1.25f && time < 2f) 
                UI.SetFutureHealth(1.5f);
            else if (time >= 2f)
                UI.SetFutureHealth(2.5f);

            time += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        StopRecoverInput();
    }

    IEnumerator HealthRecoveryDelay() {
        yield return new WaitForSeconds(0.5f);
        if (ps != null) ps.gameObject.SetActive(false);
        if (!isAttacking && !isDashing && !isFalling && !isTeleporting)
            canReceiveInput = true;
    }

    private void ChangeSpriteColor(int index)
    {
        if (index == 0) {
            sp.material.shader = shaderSpritesDefault;
            sp.color = Color.white;
        }
        // healing case
        else if (index == 1) {
            sp.material.shader = shaderGUItext;
            sp.color = Color.red;
        }
    }

    public void SetCollider(bool flag) {
        GetComponent<Collider2D>().enabled = flag;
    }


    /// PLAYER UPGRADING/UPGRADES STUFF ////////////////////////////////////////////////////////////////////////////

    public void UpdatePlayerUpgrades() {
        playerStats.CheckUpgradeLevels(out strengthUpgradeLevel, 
            out specialUpgradeLevel, 
            out focusUpgradeLevel,
            out vitalityUpgradeLevel,
            out staminaUpgradeLevel);

        UpdateStrengthUpgrades();
        UpdateSpecialUpgrades();
        UpdateFocusUpgrades();
        UpdateVitalityUpgrades();
        UpdateStaminaUpgrades();
    }

    private void UpdateStrengthUpgrades() {
        if (strengthUpgradeLevel == 0)
            return;
        else if (strengthUpgradeLevel == 1) {
            anim.SetFloat("attackMultiplier", playerUpgrades.GetStrengthUpgradeValues(1));
        } 
        else if (strengthUpgradeLevel == 2) {
            anim.SetFloat("attackMultiplier", playerUpgrades.GetStrengthUpgradeValues(1));

            jetpackDamageFlag = true;
            dashFallSpeed = playerUpgrades.GetStrengthUpgradeValues(2);
        }
    }

    private void UpdateSpecialUpgrades() {
        if (specialUpgradeLevel == 0)
            return;
        else if (specialUpgradeLevel == 1) {
            projectile.SetPierceUpgrade(true, playerUpgrades.GetSpecialUpgradeValues(1));
        } 
        else if (specialUpgradeLevel == 2) {
            projectile.SetPierceUpgrade(true, playerUpgrades.GetSpecialUpgradeValues(1));

            var tmp = playerUpgrades.GetSpecialUpgradeValues(2);
            blasterLightEnergy -= (int)(blasterLightEnergy * tmp);
            blasterHeavyEnergy -= (int)(blasterHeavyEnergy * tmp);
            superAttackEnergy1 -= (int)(superAttackEnergy1 * tmp);
            superAttackEnergy2 -= (int)(superAttackEnergy2 * tmp);
        }
    }

    private void UpdateFocusUpgrades() { 
        if (focusUpgradeLevel == 0)
            return;
        else if (focusUpgradeLevel == 1) {
            // increase player walk speed while healing
            healWalkSpeedMultiplier = playerUpgrades.GetFocusUpgradeValues(1);
        } 
        else if (focusUpgradeLevel == 2) {
            healWalkSpeedMultiplier = playerUpgrades.GetFocusUpgradeValues(1);

            // allow player to heal health on hit in addition to more energy
            focusUpgrade = true;   
        }
    }

    private void UpdateVitalityUpgrades() { 
        if (vitalityUpgradeLevel == 0)
            return;
        else if (vitalityUpgradeLevel == 1) {
            // grant a significant boost to health
            if (!vitalityUpgradeGranted) { 
                Debug.Log("Granting health boost");
                vitalityUpgradeGranted = true;
                playerStats.SetMaxHealth(UI.GetHealthMaxValue() + playerUpgrades.GetVitalityUpgradeValues(1));
            }
        } 
        else if (vitalityUpgradeLevel == 2) {
            // check to avoid granted health boost twice on leveling up to the second upgrade tier in a single playthrough
            if (!vitalityUpgradeGranted) {
                vitalityUpgradeGranted = true;
                playerStats.SetMaxHealth(UI.GetHealthMaxValue() + playerUpgrades.GetVitalityUpgradeValues(1));
            }

            // Enable single death resist per altar visit
            deathResistFlag = true;
            deathResistEnabled = true;
        }
    }

    private void UpdateStaminaUpgrades() { 
        if (staminaUpgradeLevel == 0)
            return;
        else if (staminaUpgradeLevel == 1) {

        } 
        else if (staminaUpgradeLevel == 2) {

        }
    }

    /// <summary>
    /// LOAD STUFF ////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// For loading player levels
    public void LoadPlayerLevels() {
        playerStats.IncreaseStat(-1, 0);
        UpdatePlayerUpgrades();
    }

    public void LoadCurrency(int loadedCurrency) {
        UI.LoadCurrency(loadedCurrency);
    }

    public void LoadHealth(float loadedHealth) {
        UI.LoadCurrentHealth(loadedHealth);
    }

    public void LoadEnergy(float loadedEnergy) {
        UI.LoadCurrentEnergy(loadedEnergy);
    }

    /// GET PLAYER STATS FOR SAVING /////////////////////////////////////////////////////////////////////
    public int GetHealth() {
        return UI.GetCurrentHealth();
    }

    public int GetEnergy() {
        return UI.GetCurrentEnergy();
    }

    public int GetCurrency() {
        return UI.GetCurrency();
    }

    public int GetVitalityLevel() {
        return playerStats.GetVitalityLevel();
    }

    public int GetFocusLevel() {
        return playerStats.GetFocusLevel();
    }

    public int GetStrengthLevel() {
        return playerStats.GetStrengthLevel();
    }

    public int GetStaminaLevel() {
        return playerStats.GetStaminaLevel();
    }

    public int GetSpecialLevel() {
        return playerStats.GetSpecialLevel();
    }

    private void OnApplicationQuit()
    {
        SaveManager.instance.SavePlayerValues();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackRangeVisualizer));
    }
}
