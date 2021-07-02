using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPriority
{
    public int index;
    public string attackName;

    public AttackPriority(int newIndex, string newAttackName) {
        attackName = newAttackName;
        index = newIndex;
    }
}

public class BasicEnemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject healthBar;

    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;
    //private EnemyAttack enemyAttack;
    private EnemyHealthbar healthFill;
    private EnemyMovement enemyMovement;
    private EnemyAnimation enemyAnimation;
    private EnemySounds playSound;
    private Projectile projectile;
    private Vector2 resetSpawnSpoint;

    private Coroutine healthBarRoutine;

    [Header("Enemy Stats")]
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int maxStamina;

    [SerializeField]
    private int staminaRecoveryValue;
    [SerializeField]
    private float staminaRecoveryDelay;
    [SerializeField]
    private float staminaRecoverySpeed;

    [SerializeField]
    private int currencyOnDeath;
    [SerializeField]
    private float energyGainMultiplier;
    [SerializeField]
    private float baseDamageThresholdPercent;
    [SerializeField]
    private float tetherFollowRange;
    [SerializeField]
    private float tetherUnfollowRange;

    [SerializeField]
    private bool deleteOnDeath;

    private bool isDead;
    private bool facingLeft;
    private bool isInactive;
    private bool isInvincible;
    private bool staminaRecovery;
    private bool outOfStamina;
    private bool outOfTetherRange;

    private int currentHealth;
    private int currentStamina;
    private int damageThreshold;
    private int currentDamageTaken;

    private float baseTetherRange;
    private float currentDamageThresholdPercent;

    private Coroutine deathRoutine;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private Transform visualizePoint;
    [SerializeField] 
    private float visualizeRange;

    private RaycastHit2D hitBox, playerDetected, attackFollowHit;

    [Header("Attack Properties")]
    [Header("Attack Stats (Tied to Animation Played)")]
    [SerializeField]
    private float hurtPushBackSpeed;
    [SerializeField]
    private List<Transform> attackPoints;
    [SerializeField]
    private List<float> attackRanges;
    [SerializeField]
    private List<int> attackDamages;
    [SerializeField]
    private List<float> attackPushDistances;

    [Header("Corresponding Attack Properties")]
    // Must have same amount 
    [SerializeField]
    private List<Transform> attackDetectPoints;
    [SerializeField]
    private List<float> attackDetectRanges;
    [SerializeField]
    private List<string> attackAnimations;
    [SerializeField]
    private List<int> attackPriority;
    [SerializeField]
    private List<bool> attackFollowFacePlayer;
    [SerializeField]
    private List<float> attackFollowDistances;
    [SerializeField]
    private List<bool> attackIsRay;

    [SerializeField]
    private float minStunAttackDelay;
    [SerializeField]
    private float maxStunAttackDelay;
    [SerializeField]
    private float minAttackDelay;
    [SerializeField]
    private float maxAttackDelay;

    private List<AttackPriority> priorityListOne;
    private List<AttackPriority> priorityListTwo;
    private List<AttackPriority> priorityListThree;
    private List<List<AttackPriority>> priorityLists;

    private string attackChosen;
    private RaycastHit2D attackDetected;

    private int attackIndex, attackPointIndex;
    private int abovePlayer;

    private bool inRange;
    private bool isPushed;
    private bool isStunned;
    private bool isPicking;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private bool leftOfPlayer;
    private bool attackFollowThruBoth;
    private bool attackFollowThruVertical;
    private bool attackFollowThruHorizontal;

    private float stunDelay;
    private float attackDelay;
    private float stunDuration;
    private float playerDistance;
    private float attackFollowThruSpeed;
    private Vector2 newPosition;

    private Coroutine attackAnimationRoutine, staminaRecoveryRoutine, attackFollowRoutine, teleportRoutine;
    private Coroutine invincibleRoutine, stunRoutine, resetAttackRoutine, resetHurtSpriteRoutine;
    private Coroutine pushBackDurationRoutine, pushBackMovementRoutine;

    // Start is called before the first frame update
    void Awake() {
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (shaderGUItext == null) shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = sp.material.shader;

        if (healthFill == null) healthFill = GetComponent<EnemyHealthbar>();
        if (playSound == null) playSound = GetComponent<EnemySounds>();
        if (enemyMovement == null) enemyMovement = GetComponent<EnemyMovement>();
        if (enemyAnimation == null) enemyAnimation = GetComponent<EnemyAnimation>();

        if (projectile == null) projectile = GetComponent<Projectile>();
        if (projectile != null) { 
            projectile.SetDamage(attackDamages[0]);
        }

        resetSpawnSpoint = transform.position;

        outOfTetherRange = true;
        baseTetherRange = tetherUnfollowRange;
        //enemyMovement.BeginPatrol();
        currentDamageThresholdPercent = baseDamageThresholdPercent;
        damageThreshold = Mathf.RoundToInt(currentDamageThresholdPercent * maxHealth);

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        if (healthFill != null) healthFill.SetMaxHealth(maxHealth);
        if (healthBar != null) healthBar.SetActive(false);

        /*enemyAttack.SetMaxStamina(maxStamina);
        enemyAttack.SetStaminaRecoveryValue(staminaRecoveryValue);
        enemyAttack.SetStaminaRecoveryDelay(staminaRecoveryDelay);
        enemyAttack.SetStaminaRecoverySpeed(staminaRecoverySpeed);*/

        resetAttackRoutine = StartCoroutine(ResetAttack(0f));

        priorityLists = new List<List<AttackPriority>>();
        priorityListOne = new List<AttackPriority>();
        priorityListTwo = new List<AttackPriority>();
        priorityListThree = new List<AttackPriority>();

        for (int i = 0; i < attackAnimations.Count; i++)
        {
            var tmp = attackPriority[i];
            if (tmp == 1)
                priorityListOne.Add(new AttackPriority(i, attackAnimations[i]));
            else if (tmp == 2)
                priorityListTwo.Add(new AttackPriority(i, attackAnimations[i]));
            else if (tmp == 3)
                priorityListThree.Add(new AttackPriority(i, attackAnimations[i]));
        }

        priorityLists.Add(priorityListOne);
        priorityLists.Add(priorityListTwo);
        priorityLists.Add(priorityListThree);
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerDistance();
        CheckPlayerInRange();
        MovementAnimation();

        /////////////////////////// Attack Animation Activated //////////////////
        //CheckStamina();
        if (inRange 
                && attackReady 
                && !isStunned 
                && !isAttacking 
                && !isPicking 
                && !isInactive
                && !enemyMovement.GetIsTeleporting()
                && currentStamina > 0)
            PickAttack();
    }

    private void FixedUpdate() {
        if (isDead || isInactive || outOfTetherRange)
            return;

        // Follow Player ////////////////////////////////////////////////////////////////
        FollowPlayer();
    }

    /// <summary>
    ///  ANIMATION /////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void MovementAnimation() {
        if (!isStunned && !isAttacking && !isInactive) {
            if (!enemyMovement.GetIsMoving()) {
                if (enemyMovement.GetCanTeleport() 
                        && enemyMovement.GetTeleportReady() 
                        && !attackReady 
                        && !outOfTetherRange) {
                    if (teleportRoutine != null)
                        StopCoroutine(teleportRoutine);

                    enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_TELE1);
                    teleportRoutine = StartCoroutine(Teleporting());
                }
                else if (!enemyMovement.GetCanTeleport()) {
                    enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
                }
            }
            else {
                if (!enemyMovement.GetIsTeleporting())
                    enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_RUN);
            }
        }
    }

    /// <summary>
    ///  FIND PLAYER AI //////////////////////////////////////////////////////////////////////
    /// </summary>
    IEnumerator Teleporting() {
        enemyMovement.SetIsTeleporting(true);
        enemyMovement.SetTeleportReady(false);
        //enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_TELE1);

        yield return new WaitForSeconds(enemyMovement.GetTeleportDuration()
            + enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_TELE1));

        if (isInactive)
            yield break;

        enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_TELE2);
        enemyMovement.TeleportToPlayer();
        yield return new WaitForSeconds(enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_TELE2));

        enemyMovement.SetIsTeleporting(false);
    }

    private void EnemyInTetherRange() {
        outOfTetherRange = false;
        //enemyMovement.StopPatrol();
        enemyMovement.SetFollow(true);
        enemyMovement.FindPlayerRepeating();

        if (enemyMovement.GetCanTeleport()) { 
            if (teleportRoutine != null)
                    StopCoroutine(teleportRoutine);
                teleportRoutine = StartCoroutine(Teleporting());
        }
        else if (!enemyMovement.GetCanTeleport() && !enemyMovement.GetIsTeleporting() && !isInactive)
                enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_RUN);
    }

    private void EnemyOutsideTetherRange() {
        outOfTetherRange = true;
        enemyMovement.SetFollow(false);
        enemyMovement.StopFindPlayer();
        enemyMovement.IsMoving();
        //SetIsInactive(false);
        //enemyMovement.BeginPatrol();

/*        // If enemy unfollow range is shorter than follow range, update unfollow range to the 
        // follow range - this allows for teleporting enemies to reteleport if enemy gets too far
        if (tetherUnfollowRange < tetherFollowRange)
            tetherUnfollowRange = tetherFollowRange;*/

        if (!enemyMovement.GetCanTeleport())
            enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
        else 
            if (!enemyMovement.GetIsTeleporting()) {
                enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_TELE1);
                /*if (teleportRoutine != null)
                    StopCoroutine(teleportRoutine);
                teleportRoutine = StartCoroutine(Teleporting());*/
            }
    }
    
    private void CheckPlayerDistance() {
        if (outOfTetherRange) 
            enemyMovement.FindPlayer();
        playerDistance = enemyMovement.GetPlayerDistance();

        if (playerDistance <= tetherFollowRange && outOfTetherRange)
            EnemyInTetherRange();
        else if (playerDistance > tetherUnfollowRange && !outOfTetherRange)
            EnemyOutsideTetherRange();
    }

    private void CheckPlayerInRange() {
        if (outOfTetherRange)
            return;

        if (!isAttacking)
            enemyMovement.CheckPlayerPos();

        playerDetected = enemyMovement.DetectPlayer();

        if (!attackReady)
            inRange = false;
        else if (playerDetected.collider != null && !inRange)
            inRange = true;
        else if (playerDetected.collider == null && inRange) 
            inRange = false;
    }

    private void FollowPlayer() {
        if (!isStunned && !isAttacking && !isPushed)
            enemyMovement.FollowPlayer(attackReady);
    }

    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    IEnumerator ResetAttack(float value)
    {
        yield return new WaitForSeconds(value);
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);
        attackReady = true;
    }

    // CHOOSE WHICH ATTACK ANIMATION TO PLAY
    private void AttackActivated(int attackPoint)
    {
        //called thru animation event
        // set attackPoint to specific index if using multiple attackPoints for a single index
        // otherwise keep at -1
        if (attackPoint != -1)
            attackPointIndex = attackPoint;

        enemyMovement.StopFindPlayer();
        enemyMovement.FindPlayer();
        attackHitbox = true;
        StartCoroutine(CheckHitBox());
    }

    private void AttackDeactivated()
    {
        //called thru animation event
        attackHitbox = false;
        //enemyMovement.FindPlayerRepeating();
    }

    private void PickAttack()
    {
        isPicking = true;
        float tmpRange;
        Vector2 tmpAttackVector;
        Transform tmpAttackPoint;
        for (int j = 0; j < priorityLists.Count; j++)
        {
            // priorityLists[j] is the list that contains all attacks of that specific priority
            if (priorityLists[j].Count > 0)
            {
                priorityLists[j].Shuffle();
                for (int i = 0; i < priorityLists[j].Count; i++)
                {
                    // split the attackRange / 2 and add to attackPoint to split the difference 
                    // when finding the distance between playerPosition and attackPoint, otherwise
                    // it will check the distance from both sides of attackPoint
                    tmpRange = attackDetectRanges[priorityLists[j][i].index] / 2;
                    tmpAttackPoint = attackDetectPoints[priorityLists[j][i].index];

                    if (enemyMovement.GetLeftOfPlayer())
                        tmpAttackVector = new Vector2(
                                            tmpAttackPoint.position.x + tmpRange,
                                            tmpAttackPoint.position.y);
                    else
                        tmpAttackVector = new Vector2(
                                            tmpAttackPoint.position.x - tmpRange,
                                            tmpAttackPoint.position.y);
                    // iterates through each list in priorityLists to find an attack that is currently in range
                    if (attackIsRay[priorityLists[j][i].index]) {
                        if (enemyMovement.GetLeftOfPlayer())
                            attackDetected = Physics2D.Raycast(tmpAttackPoint.position,
                                                                    Vector2.right, 
                                                                    tmpRange, 
                                                                    playerLayer);
                        else
                            attackDetected = Physics2D.Raycast(tmpAttackPoint.position,
                                                                    Vector2.left,
                                                                    tmpRange,
                                                                    playerLayer);

                        // if attackIsRay is true, use a raycast to check if player is in 
                        // attack detect range of the attack detect point, if true continue
                        if (attackDetected.collider != null) {
                            attackPointIndex = attackIndex = priorityLists[j][i].index;

                            //Debug.Log("Found attack, stopping pick attack: " + j + " " + i);
                            isPicking = false;
                            attackAnimationRoutine = StartCoroutine(AttackAnimation());
                            return;
                        }
                    }
                    else { 
                        if (Vector2.Distance(enemyMovement.GetPlayerPosition(), tmpAttackVector)
                                < tmpRange)
                        {
                            attackPointIndex = attackIndex = priorityLists[j][i].index;

                            //Debug.Log("Found attack, stopping pick attack: " + j + " " + i);
                            isPicking = false;
                            attackAnimationRoutine = StartCoroutine(AttackAnimation());
                            return;
                        }
                        // keep iterating
                    }
                }
            }
        }
        isPicking = false;

        /*//Debug.Log("restarting pick ");
        while (playerDistance >= attackRanges[attackIndex])
        {
            Debug.Log("Picking new attack");
            attackIndex = Random.Range(0, attackAnimations.Count);
            yield return null;
        }
        StartCoroutine(AttackAnimation());*/
    }

    IEnumerator AttackAnimation()
    {
        float tmpLength;
        isPushed = false;
        isPicking = false;
        isAttacking = true;
        attackReady = false;
        staminaRecovery = false;
        enemyMovement.SetFollow(false);
        attackFollowHit = new RaycastHit2D();
        attackChosen = attackAnimations[attackIndex];
        if (staminaRecoveryRoutine != null)
            StopCoroutine(StaminaRecovery());

        enemyAnimation.PlayAnimation(attackChosen);
        tmpLength = enemyAnimation.GetAnimationLength(attackChosen);

        yield return new WaitForSeconds(tmpLength);
        if (!isAttacking)
            yield break;

        FinishAttack();
    }


    private void FinishAttack() {
        isAttacking = false;
        staminaRecovery = true;
        AttackDeactivated();
        AttackFollowDeactivated();

        staminaRecoveryRoutine = StartCoroutine(StaminaRecovery());
        StartCoroutine(enemyMovement.ResetAttackFollow());

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        if (!outOfStamina)
            resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
    }

    private void SavePlayerPosition(int flag) {
        // vertical position
        if (flag == 1)
            abovePlayer = enemyMovement.GetAbovePlayer();
        // horizontal position
        else if (flag == 2)
            leftOfPlayer = enemyMovement.GetLeftOfPlayer();
        // both positions
        else if (flag == 3) {
            abovePlayer = enemyMovement.GetAbovePlayer();
            leftOfPlayer = enemyMovement.GetLeftOfPlayer();
        }
    }

    IEnumerator CheckHitBox()
    {
        while (attackHitbox)
        {
            if (enemyMovement.GetLeftOfPlayer())
                hitBox = Physics2D.Raycast(attackPoints[attackPointIndex].position, Vector2.right, attackRanges[attackPointIndex], playerLayer);
            else
                hitBox = Physics2D.Raycast(attackPoints[attackPointIndex].position, Vector2.left, attackRanges[attackPointIndex], playerLayer);

            if (hitBox.collider != null) {
                hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(attackDamages[attackPointIndex],
                                                            attackPushDistances[attackPointIndex],
                                                            rb.position);
            }

            yield return null;
        }
        yield break;
    }

    private void AttackFollowBothActivated(float value)
    {
        // called thru animation events
        attackFollowThruBoth = true;
        attackFollowThruSpeed = value;
        attackFollowRoutine = StartCoroutine(AttackFollowThroughBoth());
    }

    private void AttackFollowVerticalActivated(float value)
    {
        // called thru animation events
        attackFollowThruVertical = true;
        attackFollowThruSpeed = value;
        attackFollowRoutine = StartCoroutine(AttackFollowThroughVertical());
    }

    private void AttackFollowHorizontalActivated(float value)
    {
        // called thru animation events
        attackFollowThruHorizontal = true;
        attackFollowThruSpeed = value;
        attackFollowRoutine = StartCoroutine(AttackFollowThroughHorizontal());
    }

    private void AttackFollowDeactivated()
    {
        // called thru animation events
        newPosition = Vector2.zero;
        attackFollowThruBoth = false;
        attackFollowThruVertical = false;
        attackFollowThruHorizontal = false;

        if (attackFollowRoutine != null)
            StopCoroutine(attackFollowRoutine);
    }

    IEnumerator AttackFollowThroughVertical()
    {
        // up down movement only
        if (attackFollowFacePlayer[attackIndex]) 
            enemyMovement.CheckPlayerPos();

        SavePlayerPosition(1);
        while (attackFollowThruVertical)
        {

            if (abovePlayer == 1) {
                newPosition = new Vector2(0f, attackFollowDistances[attackIndex])
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }
            else if (abovePlayer == -1) {
                newPosition = new Vector2(0f, -attackFollowDistances[attackIndex])
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }
            else if (abovePlayer == 0) {
                newPosition = Vector2.zero;
            }

            // raycast to check if enemy can move in that direction
            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) {
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }

        // reset border raycast check
        attackFollowHit = new RaycastHit2D();
        yield break;
    }

    IEnumerator AttackFollowThroughHorizontal()
    {
        // Left right movement only
        if (attackFollowFacePlayer[attackIndex]) 
            enemyMovement.CheckPlayerPos();

        SavePlayerPosition(2);
        while (attackFollowThruHorizontal)
        {

            if (leftOfPlayer) {
                newPosition = new Vector2(attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
            }
            else {
                newPosition = new Vector2(-attackFollowDistances[attackIndex], 0f)
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }

            // raycast to check if enemy can move in that direction
            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) { 
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }

        // reset border raycast check
        attackFollowHit = new RaycastHit2D();
        yield break;
    }

    IEnumerator AttackFollowThroughBoth()
    {
        // for changing the direction of enemy to continually face the player
        if (attackFollowFacePlayer[attackIndex]) 
            enemyMovement.CheckPlayerPos();

        SavePlayerPosition(3);
        while (attackFollowThruBoth)
        {

            if (leftOfPlayer) {
                // if left of player, always move right
                if (abovePlayer == 1) {
                    newPosition = new Vector2(attackFollowDistances[attackIndex],
                                            attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == -1) {
                    newPosition = new Vector2(attackFollowDistances[attackIndex],
                                            -attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == 0) {
                    newPosition = new Vector2(attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
            }
            else if (!leftOfPlayer) {
                // if to the right of player, always move left
                if (abovePlayer == 1) {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex],
                                            attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == -1) {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex],
                                            -attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == 0) {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
            }

            // raycast to check if enemy can move in that direction
            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) {
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }

        // reset border raycast check
        attackFollowHit = new RaycastHit2D();
        yield break;
    }

    private void AttackStartSavePosition() {
        // animation event called thru enemy attacks, useful for follow attacks that dont
        // chase player movement but just where they were at the start of the attack
        if (attackFollowFacePlayer[attackIndex])
            enemyMovement.CheckPlayerPos();

        enemyMovement.CheckPlayerVertical();
        SavePlayerPosition(3);
    }

    // STAMINA CODE ////////////////////////////////////////////////////////

    IEnumerator StaminaRecovery()
    {
        yield return new WaitForSeconds(staminaRecoveryDelay);
        while (currentStamina < maxStamina && staminaRecovery && !isAttacking)
        {
            currentStamina += staminaRecoveryValue;
            CheckStamina();
            yield return new WaitForSeconds(staminaRecoverySpeed);
        }
    }

    private void CheckStamina()
    {
        if (currentStamina > maxStamina) {
            currentStamina = maxStamina;
        }

        if (!outOfStamina && currentStamina <= 0)
            outOfStamina = true;
        else if (outOfStamina && currentStamina >= maxStamina) {
            if (!isStunned || !isAttacking) {
                if (resetAttackRoutine != null)
                    StopCoroutine(resetAttackRoutine);
                resetAttackRoutine = StartCoroutine(ResetAttack(0f));
            }
            outOfStamina = false;
        }
    }

    private void ConsumeStamina(int value)
    {
        // called thru animation event
        if (currentStamina - value < 0)
            currentStamina = 0;
        else
            currentStamina -= value;
        CheckStamina();
    }

    /// <summary>
    ///  ENEMY IS HIT /////////////////////////////////////////////////////////////////////////////
    /// </summary>
    private void SetIsInvincible(int flag) {
        // also called thru animation events
        if (flag == 0)
            isInvincible = false;
        else if (flag == 1)
            isInvincible = true;
    }
    
    /// <summary>
    /// If enemy was stunned, will not follow/attack. Stun will turn off after a delay
    /// </summary>
    IEnumerator ResetStun(float value) {
        yield return new WaitForSeconds(value);
        isStunned = false;
        stunDelay = Random.Range(minStunAttackDelay, maxStunAttackDelay);

        StartCoroutine(enemyMovement.ResetStunFollow());
        if (!outOfStamina) {
            if (resetAttackRoutine != null)
                StopCoroutine(resetAttackRoutine);
            resetAttackRoutine = StartCoroutine(ResetAttack(stunDelay));
        }
    }

    IEnumerator ResetInvincible(float value) {
        yield return new WaitForSeconds(value);
        SetIsInvincible(0);
    }

    /// <summary>
    /// Called when player hits enemy
    /// </summary>
    public void EnemyHurt(int damageNum, float distance, int soundIndex) {
        if (isDead || isInvincible)
            return;

        SetIsInvincible(1);
        Player.instance.RegainEnergy(GetEnergyGainMultiplier());
        stunDuration = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

        PushBack(distance);
        currentHealth -= damageNum;
        if (healthBar != null && healthFill != null) {
            healthBar.SetActive(true);
            healthFill.SetCurrentHealth(damageNum);

            if (healthBarRoutine != null)
                StopCoroutine(healthBarRoutine);
            healthBarRoutine = StartCoroutine(HealthBarVisibility(5f));
        }
        StartCoroutine(BeginDamageThreshold(damageNum));

        // Play sounds
        playSound.PlayEnemyHit(soundIndex);

        if (currentHealth <= 0) {
            if (deathRoutine != null)
                StopCoroutine(deathRoutine);
            deathRoutine = StartCoroutine(Death());
        }
        else {
            // if damage threshold reached, stagger enemy
            // else don't interrupt enemy 
            if (currentDamageTaken >= damageThreshold) { 
                enemyMovement.SetFollow(false);
                enemyAnimation.ReplayAnimation(EnemyAnimStates.ENEMY_HURT);

                isStunned = true;
                if (stunRoutine != null)
                    StopCoroutine(stunRoutine);
                stunRoutine = StartCoroutine(ResetStun(stunDuration + 0.05f));

                AttackDeactivated();
                AttackFollowDeactivated();
                if (isAttacking) {
                    if (attackAnimationRoutine != null)
                        StopCoroutine(attackAnimationRoutine);
                    FinishAttack();
                }

                if (resetHurtSpriteRoutine != null)
                    StopCoroutine(resetHurtSpriteRoutine);
                resetHurtSpriteRoutine = StartCoroutine(ResetHurtSprite(0f));
            }
            else { 
                SetHurtSprite();

            }

            if (invincibleRoutine != null)
                StopCoroutine(invincibleRoutine);
            invincibleRoutine = StartCoroutine(ResetInvincible(stunDuration + 0.05f));
        }
    }

    IEnumerator HealthBarVisibility(float delay) {
        yield return new WaitForSeconds(delay);
        healthBar.SetActive(false);
    }

    /// <summary>
    /// Counter that adds to currentDamageTaken to see if player did enough damage to stagger enemy
    /// </summary>
    IEnumerator BeginDamageThreshold(int damageNum) {
        currentDamageTaken += damageNum;
        yield return new WaitForSeconds(.5f);
        currentDamageTaken -= damageNum;
        if (currentDamageTaken < 0)
            currentDamageTaken = 0;
    }

    /// <summary>
    /// Will call during animation events, where some attacks require a higher damage threshold 
    /// to interrupt the enemy.
    /// Must call the Reset at the end of that specific animation
    /// </summary>
    /// <param name="newValue"></param>
    private void SetDamageThresholdPercent(float newValue) {
        // most likely called thru animation events
        currentDamageThresholdPercent = newValue;
        damageThreshold = Mathf.RoundToInt(currentDamageThresholdPercent * maxHealth);
    }

    /// <summary>
    /// Reset for damage threshold percent that should be called during an animation if 
    /// SetDamageThresholdPercent() was called
    /// </summary>
    private void ResetDamageThresholdPercent() {
        currentDamageThresholdPercent = baseDamageThresholdPercent;
        damageThreshold = Mathf.RoundToInt(currentDamageThresholdPercent * maxHealth);
    }

    /// <summary>
    /// To show enemy is hurt without stunning/stopping its animation/movement, which utilizes
    /// a GUI/Text shader
    /// </summary>
    private void SetHurtSprite() {
        sp.material.shader = shaderGUItext;
        sp.color = Color.white;

        if (resetHurtSpriteRoutine != null)
            StopCoroutine(resetHurtSpriteRoutine);
        resetHurtSpriteRoutine = StartCoroutine(ResetHurtSprite(stunDuration));
    }

    /// <summary>
    /// Called to reset the GUI/Text shader that makes the enemy white
    /// </summary>
    IEnumerator ResetHurtSprite(float delay) {
        yield return new WaitForSeconds(delay);
        sp.material.shader = shaderSpritesDefault;
        sp.color = Color.white;
    }

    /// <summary>
    /// Pushes back enemy a set distance when hurt
    /// </summary>
    private void PushBack(float distance) {
        isPushed = true;
        Vector2 newPosition;
        if (Player.instance.transform.position.x > rb.position.x) {
            newPosition = new Vector2(-distance, 0f);

            if (pushBackDurationRoutine != null)
                StopCoroutine(pushBackDurationRoutine);
            pushBackDurationRoutine = StartCoroutine(PushBackDuration(newPosition, stunDuration));
        }
        else if (/*enemyMovement.GetPlayerPosition()*/ Player.instance.transform.position.x <= transform.position.x) {
            newPosition = new Vector2(distance, 0f);

            if (pushBackDurationRoutine != null)
                StopCoroutine(pushBackDurationRoutine);
            pushBackDurationRoutine = StartCoroutine(PushBackDuration(newPosition, stunDuration));
        }
    }

    IEnumerator PushBackDuration(Vector2 position, float duration) {
        if (pushBackMovementRoutine != null)
            StopCoroutine(pushBackMovementRoutine);
        pushBackMovementRoutine = StartCoroutine(PushBackMovement(position));

        yield return new WaitForSeconds(duration);

        isPushed = false;
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

    public float GetEnergyGainMultiplier() {
        return energyGainMultiplier;
    }

    public bool GetIsInvincible() {
        return isInvincible;
    }

    public bool GetIsDead() {
        return isDead;
    }

    public void IsDead(bool facing)
    {
        facingLeft = facing;
        deathRoutine = StartCoroutine(LoadDead());
    }

    public bool GetFacing() {
        return enemyMovement.GetFacingDirection();
    }

    IEnumerator LoadDead() {
        isDead = true;
        enemyMovement.StopFindPlayer();
        enemyMovement.SetDirection(facingLeft);
        enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_DEAD);
        var tmp = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_DEAD);

        GetComponent<Collider2D>().enabled = false;
        transform.position = EnemyManager.Instance.GetDeathPosition(transform, name);

        yield return new WaitForSeconds(tmp);
        if (deleteOnDeath)
            sp.enabled = false;
    }

    /// <summary>
    /// Death will disable this and enemyMovement script after playing the death animation.
    /// Will connect to a script that allows player to continue past an area
    /// </summary>
    IEnumerator Death() {
        isDead = true;
        enemyMovement.StopFindPlayer();
        GetComponent<Collider2D>().enabled = false;
        enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_DEATH);
        var tmp = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_DEATH);
        EnemyManager.Instance.EnemyDead(name);

        yield return new WaitForSeconds(tmp);
        GiveCurrency();
        if (deleteOnDeath)
            sp.enabled = false;

        //Destroy(enemyMovement);
        //Destroy(this);
    }

    // Called through death animation event
    private void GiveCurrency() {
        //enemyMovement.GetPlayer().GetComponent<PlayerUI>().SetCurrency(currencyOnDeath);
        Player.instance.GetComponent<PlayerUI>().SetCurrency(currencyOnDeath);
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

    // ENEMY RESET /////////////////////////////////////////////////////////////////////////////
            public void ResetToSpawn()
    {
        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        SetIsInactive(true);
        enemyMovement.ResetDirection();
        transform.position = new Vector2(resetSpawnSpoint.x, resetSpawnSpoint.y);

        if (healthFill != null)
            healthFill.ResetHealth();
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        isInvincible = false;
        outOfStamina = false;
        attackReady = true;
        if (isAttacking)
            FinishAttack();

        if (isDead) { 
            isDead = false;
            sp.enabled = true;
            enemyMovement.FindPlayerRepeating();
            GetComponent<Collider2D>().enabled = true;
            enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
        }
    }

    public void SetIsInactive(bool flag) {
        if (isAttacking && flag)
            FinishAttack();
        isInactive = flag;
    }

    // FOR LOADING ///////////////////////////////////////////////////////////////////////////
    public Vector2 GetSpawnPoint() {
        return resetSpawnSpoint;
    }

    // GIZMOS ////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmosSelected()
    {
        if (visualizePoint != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(visualizePoint.position, (Vector2)visualizePoint.position + (Vector2.right * visualizeRange));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(visualizePoint.position, (Vector2)visualizePoint.position + (Vector2.left * visualizeRange));
        }
        Gizmos.color = Color.green;
        if (rb != null)
            Gizmos.DrawLine(rb.position, rb.position + newPosition);
    }
}
