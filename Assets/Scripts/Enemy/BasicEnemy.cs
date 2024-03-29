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
    [SerializeField] [Tooltip("For Non Bosses")] 
    private GameObject healthBarParent;

    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;
    //private EnemyAttack enemyAttack;
    private EnemyHealthbar healthFill;
    private EnemyMovement enemyMovement;
    private EnemyAnimation enemyAnimation;
    private EnemySounds playSound;
    private BossHealthbar bossHealthbar;
    private BossPhases bossPhases;
    private Projectile projectile;

    private Vector2 resetSpawnSpoint;
    private Coroutine healthBarRoutine;

    [Header("Boss Variables")]
    private float phaseThresholdPercent = 0f;
    private float animSpeedMult = 1f;
    private float moveSpeedMult = 1f;
    private float reducedAttackDelay = 0f;

    private bool isNextPhase;
    private bool isBossHealthbarActive;

    [Header("Enemy Stats")]
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int maxStamina;
    [SerializeField]
    private float healthBarVisiblityDuration;

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

    [SerializeField] [Tooltip("Delete sprite on death")]
    private bool deleteOnDeath;
    [SerializeField]
    private bool isBoss;

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

    private Coroutine deathRoutine, damageThresholdRoutine;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private Transform visualizePoint;
    [SerializeField] 
    private float visualizeRange;

    private RaycastHit2D hitBox, playerDetected, attackFollowHit;

    [Header("Attack Properties")]
    [Header("Attack Stats (Tied to AttackActivated in Animation Played)")]
    [SerializeField] [Tooltip ("Speed enemy is pushed back")]
    private float hurtPushBackSpeed;
    [SerializeField] [Tooltip ("Attack point (Transform) used for each attack")]
    private List<Transform> attackPoints;
    [SerializeField] [Tooltip ("Range for each attack")]
    private List<float> attackRanges;
    [SerializeField] [Tooltip ("Damage for each attack")]
    private List<int> attackDamages;
    [SerializeField]
    private List<float> attackPushDistances;

    [Tooltip("Used for each type of attack")]
    private int attackPointIndex;

    [Header("Corresponding Attack Properties (Tied to Each Attack Animation")]
    // Must have same amount 
    [SerializeField]
    private List<Transform> attackDetectPoints;
    [SerializeField] [Tooltip ("Range that enemy will begin picking attacks")]
    private List<float> attackDetectRanges;
    [SerializeField] [Tooltip ("Name of animation to be played")]
    private List<string> attackAnimations;
    [SerializeField] [Tooltip ("Enemy attacks work off priority. 1 > greater priority, 3 > least priority")]
    private List<int> attackPriority;
    [SerializeField] [Tooltip ("Does enemy face player before attack")]
    private List<bool> attackFollowFacePlayer;
    [SerializeField] [Tooltip ("Distance enemy moves during attack follow through")]
    private List<float> attackFollowDistances;
    [SerializeField] [Tooltip ("Does attack use raycast (for projectiles or straight attacks")]
    private List<bool> attackIsRay;
    [SerializeField] [Tooltip ("Does this attack only get used during a bosses second phase?")]
    private List<bool> boss2ndPhaseAttack;

    [SerializeField] [Tooltip("If enemy is stunned, will not follow/attack after delay")]
    private float minStunAttackDelay;
    [SerializeField] [Tooltip("If enemy is stunned, will not follow/attack after delay")]
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

    [Tooltip("Used for each attack animation")]
    private int attackIndex;
    private int abovePlayer;

    private bool inRange;
    private bool isPushed;
    private bool isStunned;
    private bool isPicking;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private bool leftOfPlayer;
    private bool attackFacePlayer;
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
        if (isBoss) { 
            if (bossHealthbar == null) bossHealthbar = GetComponent<BossHealthbar>();
            if (bossPhases == null) bossPhases = GetComponent<BossPhases>();
        }


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

        if (bossHealthbar != null && isBoss) { 
            bossHealthbar.SetHealthbar(false);
        } 
        else if (healthFill != null && healthBarParent != null) {
            // turn off healthbar initially
            healthBarParent.SetActive(false);
            healthFill.SetMaxHealth(maxHealth);
        }

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
            // Get the priority of each attackAnimation, then set that into its corresponding priorityList
            // 1 > higher priority / 3 > lowest priority
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

    private void Start()
    {
        // Initialize variables by passing into bossPhases.cs thru reference
        if (isBoss)
            bossPhases.InitializePhaseVariables(out phaseThresholdPercent, 
                                                    out animSpeedMult,
                                                    out moveSpeedMult,
                                                    out reducedAttackDelay);
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerDistance();
        CheckPlayerInRange();
        MovementAnimation();
        CheckBossHealthBar();

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

        // if enemy is not attack, face player at its regular intervals
        // if enemy is attacking AND can follow the player AND is in the time frame of the animation to look at player, do so
        if (!isAttacking || (isAttacking && attackFacePlayer && attackFollowFacePlayer[attackIndex]))
            facingLeft = enemyMovement.CheckPlayerPos();

/*        if (healthBarParent != null) { 
            if (transform.localScale.x < 0 && healthBarParent.transform.localScale.x < 0) { 
                healthBarParent.transform.localScale = new Vector3(-healthBarParent.transform.localScale.x, healthBarParent.transform.localScale.y);
                Debug.Log("facing left health bar");
            }
            else if (transform.localScale.x > 0 && healthBarParent.transform.localScale.x > 0) { 
                healthBarParent.transform.localScale = new Vector3(healthBarParent.transform.localScale.x, healthBarParent.transform.localScale.y);
                Debug.Log("facing right health bar");
            }
        }*/

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

    private void CheckBossHealthBar() {
        // If boss is in tether range and set to active, set healthbar true
        if (isBoss && !outOfTetherRange && bossHealthbar != null && !bossHealthbar.GetHealthBar()) {
            if (!isInactive && !isBossHealthbarActive) { 
                bossHealthbar.SetHealthbar(true);
                bossHealthbar.SetMaxHealth(maxHealth);
                bossHealthbar.SetBossName(name);

                isBossHealthbarActive = true;
                //Debug.Log("Boss is active and is boss " + name);
            } else if (isInactive && isBossHealthbarActive) {
                bossHealthbar.SetHealthbar(false);
                isBossHealthbarActive = false;
            }
        }
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

        // If enemy follows player movement, enable sprite direction change
        if (attackFollowFacePlayer[attackIndex])
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
        //float tmpRange;
        //Vector2 tmpAttackVector;
        //Transform tmpAttackPoint;
        for (int j = 0; j < priorityLists.Count; j++) // j < 3 because there are always 3 priorityLists
        {
            // priorityLists[j] is the list that contains all attacks of that specific priority
            if (priorityLists[j].Count > 0)
            {
                // Shuffle the attackAnimations and their corresponding indexes in the list
                priorityLists[j].Shuffle();
                for (int i = 0; i < priorityLists[j].Count; i++) // priorityLists[j][i] gets the AttackPriority
                {
                    if (isBoss)
                    {
                        // if the selected attack is a 2ndPhaseAttack and boss IS in 2ndPhase, choose attack
                        if (boss2ndPhaseAttack[priorityLists[j][i].index] && isNextPhase)
                        {
                            ConfirmAttack(j, i);
                        }   
                        // if the selected attack is NOT a 2ndPhaseAttack, choose attack
                        else if (!boss2ndPhaseAttack[priorityLists[j][i].index])
                        {
                            ConfirmAttack(j, i);
                        }
                        // else - keep iterating through the list
                    }
                    else if (!isBoss)
                    {
                        // if enemy is NOT a boss and selected attack is NOT a 2ndPhaseAttack, choose attack
                        if (!boss2ndPhaseAttack[priorityLists[j][i].index])
                        {
                            ConfirmAttack(j, i);
                        }
                        // else - keep iterating through the list
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

    private void ConfirmAttack(int j, int i)
    {
        float tmpRange;
        Vector2 tmpAttackVector;
        Transform tmpAttackPoint;

        // split the attackRange / 2 and add to attackPoint to split the difference 
        // when finding the distance between playerPosition and attackPoint, otherwise
        // it would check the distance from both sides of attackPoint
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
        if (attackIsRay[priorityLists[j][i].index])
        { // utilizes a ray attack
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
            if (attackDetected.collider != null)
            {
                attackPointIndex = attackIndex = priorityLists[j][i].index;

                //Debug.Log("Found attack, stopping pick attack: " + j + " " + i);
                isPicking = false;
                if (attackAnimationRoutine != null)
                    StopCoroutine(attackAnimationRoutine);
                attackAnimationRoutine = StartCoroutine(AttackAnimation());
                return;
            }
        }
        else
        {
            if (Vector2.Distance(enemyMovement.GetPlayerPosition(), tmpAttackVector)
                    < tmpRange)
            { // utilizes distance from attackPoint  
                attackPointIndex = attackIndex = priorityLists[j][i].index;

                //Debug.Log("Found attack, stopping pick attack: " + j + " " + i);
                isPicking = false;
                if (attackAnimationRoutine != null)
                    StopCoroutine(attackAnimationRoutine);
                attackAnimationRoutine = StartCoroutine(AttackAnimation());
                return;
            }
        }
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
            StopCoroutine(staminaRecoveryRoutine);

        enemyAnimation.PlayAnimation(attackChosen);
        if (isBoss && isNextPhase)
            // divide animation length by increasedAnimSpeed if Boss to account for increasedAnimSpeed
            tmpLength = enemyAnimation.GetAnimationLength(attackChosen) / animSpeedMult;
        else
            tmpLength = enemyAnimation.GetAnimationLength(attackChosen);

        yield return new WaitForSeconds(tmpLength);
        if (!isAttacking)
            yield break;

        FinishAttack();
    }


    private void FinishAttack() {
        isAttacking = false;
        staminaRecovery = false;
        //staminaRecovery = true;
        AttackDeactivated();
        AttackFollowDeactivated();

        if (attackAnimationRoutine != null)
            StopCoroutine(attackAnimationRoutine);

        if (staminaRecoveryRoutine != null)
            StopCoroutine(staminaRecoveryRoutine);
        staminaRecoveryRoutine = StartCoroutine(StaminaRecovery());

        StartCoroutine(enemyMovement.ResetAttackFollow());

        if (resetAttackRoutine != null)
            StopCoroutine(resetAttackRoutine);
        if (!outOfStamina)
            resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
    }

    /// <summary>
    /// Used to save vertical, horizontal, or both for attack follow thru's
    /// </summary>
    /// <param name="flag"></param>
    private void SavePlayerPosition(int flag) {
        // do nothing
        if (flag == 0)
            return;
        // vertical position
        else if (flag == 1)
            abovePlayer = enemyMovement.GetAbovePlayer();
        // horizontal position
        else if (flag == 2)
            leftOfPlayer = enemyMovement.GetLeftOfPlayer();
        // both positions
        else if (flag == 3)
        {
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
            else if (!enemyMovement.GetLeftOfPlayer())
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
            facingLeft = enemyMovement.CheckPlayerPos();

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
            facingLeft = enemyMovement.CheckPlayerPos();

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
            facingLeft = enemyMovement.CheckPlayerPos();

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
            facingLeft = enemyMovement.CheckPlayerPos();

        enemyMovement.CheckPlayerVertical();
        SavePlayerPosition(3);
    }

    private void CameraShakeEvent(float magnitude) {
        // called thru animation events for certain attack animations
        GameManager.instance.BeginCameraShake(0.2f, magnitude);
    }

    // STAMINA CODE ////////////////////////////////////////////////////////

    IEnumerator StaminaRecovery()
    {
        yield return new WaitForSeconds(staminaRecoveryDelay);
        staminaRecovery = true;
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

        if (!outOfStamina && currentStamina <= 0) { 
            currentStamina = 0;
            outOfStamina = true;
        }
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
        if (isDead)
            yield return null;

        isStunned = false;
        stunDelay = Random.Range(minStunAttackDelay, maxStunAttackDelay);

        // extra case just in case enemy hitbox stays active
        AttackDeactivated();

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
        PushBack(distance);
        currentHealth -= damageNum;
        stunDuration = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

        // Player regains energy on hitting enemy with a normal attack
        Player.instance.RegainEnergy(GetEnergyGainMultiplier());

        if (!isBoss) { 
            // if not a boss, set health bar active/inactive when hit
            if (healthBarParent != null && healthFill != null) {
                healthBarParent.SetActive(true);
                // updates health text and visuals
                healthFill.UpdateHealthbarDirection();
                healthFill.SetCurrentHealth(damageNum);

                if (healthBarRoutine != null)
                    StopCoroutine(healthBarRoutine);
                healthBarRoutine = StartCoroutine(HealthBarVisibility(healthBarVisiblityDuration));
            } 
        }
        else if (isBoss) {
            // if boss, update enemy health through BossHealthBar.cs
            bossHealthbar.UpdateHealth(damageNum);

            // if health threshold reached, begin next phase using variables from BossPhases.cs
            if (currentHealth / (float)maxHealth <= phaseThresholdPercent && !isNextPhase) { 
                BeginNextPhase();
            }
        }

        // Play sounds
        playSound.PlayEnemyHit(soundIndex);

        // Begin damage threshold timer, and reset whenever enemy is hit
        if (damageThresholdRoutine != null)
            StopCoroutine(damageThresholdRoutine);
        damageThresholdRoutine = StartCoroutine(BeginDamageThreshold(damageNum));

        if (currentHealth <= 0) {
            if (deathRoutine != null)
                StopCoroutine(deathRoutine);
            deathRoutine = StartCoroutine(Death());
        }
        else {
            // if damage threshold reached, stagger enemy and reset damage threshold
            // else don't interrupt enemy 
            if (currentDamageTaken >= damageThreshold) { 
                currentDamageTaken = 0;
                enemyMovement.SetFollow(false);
                enemyAnimation.ReplayAnimation(EnemyAnimStates.ENEMY_HURT);

                isStunned = true;
                if (stunRoutine != null)
                    StopCoroutine(stunRoutine);
                stunRoutine = StartCoroutine(ResetStun(stunDuration + 0.07f));

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

    /// <summary>
    /// Called thru animation event: will hide health bar when enemies disappear such as when teleporting
    /// </summary>
    private void HideHealthBar() {
        if (healthBarParent != null && healthFill != null) {
            if (healthBarRoutine != null)
                StopCoroutine(healthBarRoutine);

            healthBarParent.SetActive(false);
        }
    }

    IEnumerator HealthBarVisibility(float delay) {
        if (isBoss)
            yield return null;

        healthBarParent.SetActive(true);
        yield return new WaitForSeconds(delay);
        healthBarParent.SetActive(false);
    }

    /// <summary>
    /// Counter that adds to currentDamageTaken to see if player did enough damage to stagger enemy
    /// </summary>
    IEnumerator BeginDamageThreshold(int damageNum) {
        currentDamageTaken += damageNum;
        yield return new WaitForSeconds(.5f);
        //currentDamageTaken -= damageNum;
        currentDamageTaken = 0;
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
        if (!isBoss)
            sp.color = Color.white;
        else
        {
            if (!isNextPhase)
                sp.color = Color.white;
            else
                sp.color = Color.yellow;
        }
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

    public bool GetIsBoss() {
        return isBoss;
    }

    public int GetMaxHealth() {
        return maxHealth;
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
        // if player is not dead, enemy does not need to be revived in the circumstance: player exited game before enemies revived
        if (!SaveManager.instance.activeSave.playerIsDead) { 
            isDead = true;
            enemyMovement.StopFindPlayer();
            enemyMovement.SetDirection(facingLeft);
            enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_DEAD);
            var tmp = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_DEAD);

            GetComponent<Collider2D>().enabled = false;
            transform.position = EnemyManager.Instance.GetDeathPosition(transform, name);
            if (healthFill != null)
                healthFill.UpdateHealthbarDirection();

            yield return new WaitForSeconds(tmp);
            if (deleteOnDeath /*|| isBoss*/)
                sp.enabled = false;
        }
    }

    /// <summary>
    /// Death will disable this and enemyMovement script after playing the death animation.
    /// Will connect to a script that allows player to continue past an area
    /// </summary>
    IEnumerator Death() {
        isDead = true;
        enemyMovement.EnemyDeath();
        GetComponent<Collider2D>().enabled = false;
        enemyAnimation.PlayAnimation(EnemyAnimStates.ENEMY_DEATH);
        var tmp = enemyAnimation.GetAnimationLength(EnemyAnimStates.ENEMY_DEATH);
        EnemyManager.Instance.EnemyDead(name);

        // if enemy is a boss, set boss healthbar inactive and set bossDefeated at current bossArenaIndex 
        if (isBoss) { 
            bossHealthbar.SetHealthbar(false);
            GameManager.instance.BossDefeated();
        }

        yield return new WaitForSeconds(tmp);
        GiveCurrency();

        if (healthBarRoutine != null)
            StopCoroutine(healthBarRoutine);
        healthBarRoutine = StartCoroutine(HealthBarVisibility(1f));

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
        if (isBoss) {
            ResetPhaseVariables();
            bossHealthbar.SetHealthbar(false);
        }

        if (deathRoutine != null)
            StopCoroutine(deathRoutine);

        ResetHealth();
        SetIsInactive(true);
        enemyMovement.ResetDirection();
        
        if (healthFill != null)
            healthFill.UpdateHealthbarDirection();
        transform.position = new Vector2(resetSpawnSpoint.x, resetSpawnSpoint.y);

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

    public void ResetHealth()
    {
        if (healthFill != null)
            healthFill.ResetHealth();
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    public void SetIsInactive(bool flag) {
        if (isAttacking && flag) { 
            FinishAttack();
            if (isBoss) 
                GameManager.instance.SetBossHealthbar(false);
        }
        isInactive = flag;
    }

    // FOR LOADING ///////////////////////////////////////////////////////////////////////////
    public Vector2 GetSpawnPoint() {
        return resetSpawnSpoint;
    }

    // FOR BOSS PHASE STUFF //////////////////////////////////////////////////////////////////
    private void BeginNextPhase() {
        Debug.Log(name + " is starting next phase!");
        isNextPhase = true;

        // play animation
        
        // change sprite color
        sp.material.shader = shaderGUItext;
        sp.color = Color.red;

        // increase enemy animation speed
        enemyAnimation.SetAnimSpeed(animSpeedMult);

        // increase enemy move speed
        enemyMovement.SetMoveSpeed(moveSpeedMult);

        // reduce attack delay / increase attack frequency
        minAttackDelay -= reducedAttackDelay;
        maxAttackDelay -= reducedAttackDelay;
    }

    private void ResetPhaseVariables() {
        isNextPhase = false;

        // Reset sprite color
        sp.material.shader = shaderSpritesDefault;
        sp.color = Color.white;

        // Reset animation/move speeds
        enemyAnimation.SetAnimSpeed(1f);
        enemyMovement.ResetMoveSpeed();

        // Reset attack delays
        minAttackDelay += reducedAttackDelay;
        maxAttackDelay += reducedAttackDelay;
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
