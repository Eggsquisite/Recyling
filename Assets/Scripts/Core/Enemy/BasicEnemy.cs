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
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;
    //private EnemyAttack enemyAttack;
    private EnemyMovement enemyMovement;
    private EnemySounds playSound;
    private Projectile projectile;
    private string currentState;

    [Header("Enemy Stats")]
    [SerializeField]
    private float tetherFollowRange;
    [SerializeField]
    private float tetherUnfollowRange;

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

    private bool isDead;
    private bool isInvincible;
    private bool staminaRecovery;
    private bool outOfStamina;
    private bool outOfTetherRange;

    private int currentHealth;
    private int currentStamina;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private Transform visualizePoint;
    [SerializeField] 
    private float visualizeRange;

    private RaycastHit2D hitBox, playerDetected, attackFollowHit;

    [Header("Attack Properties")]
    [SerializeField]
    private List<Transform> attackDetectPoints;
    [SerializeField]
    private List<float> attackDetectRanges;
    [SerializeField]
    private List<Transform> attackPoints;
    [SerializeField]
    private List<float> attackRanges;
    [SerializeField]
    private List<string> attackAnimations;
    [SerializeField]
    private List<int> attackPriority;
    [SerializeField]
    private List<int> attackDamages;
    [SerializeField]
    private List<float> attackFollowDistances;
    [SerializeField]
    private List<bool> attackFollowFacePlayer;

    [SerializeField]
    private float minAttackDelay;
    [SerializeField]
    private float maxAttackDelay;

    private List<AttackPriority> priorityListOne;
    private List<AttackPriority> priorityListTwo;
    private List<AttackPriority> priorityListThree;
    private List<List<AttackPriority>> priorityLists;

    private string attackChosen;

    private int attackIndex, attackPointIndex;
    private int abovePlayer;

    private bool leftOfPlayer;
    private bool inRange;
    private bool isStunned;
    private bool isPicking;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private bool attackFromLeft;
    private bool attackFollowThruBoth;
    private bool attackFollowThruVertical;
    private bool attackFollowThruHorizontal;

    private float attackFollowThruSpeed;
    private float attackDelay;
    private float stunDuration;
    private float playerDistance;
    private Vector2 newPosition;

    private Coroutine attackAnimationRoutine, staminaRecoveryRoutine, attackFollowRoutine;
    private Coroutine invincibleRoutine, stunRoutine, resetAttackRoutine;

    // Start is called before the first frame update
    void Awake() {
        if (anim == null) anim = GetComponent<Animator>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        ac = anim.runtimeAnimatorController;

        if (playSound == null) playSound = GetComponent<EnemySounds>();
        if (enemyMovement == null) enemyMovement = GetComponent<EnemyMovement>();

        if (projectile == null) projectile = GetComponent<Projectile>();
        if (projectile != null)
            projectile.SetDamage(attackDamages[0]);


        currentHealth = maxHealth;
        currentStamina = maxStamina;

        outOfTetherRange = true;
        InvokeRepeating("CheckPlayerDistance", 0f, 0.25f);

        /*enemyAttack.SetMaxStamina(maxStamina);
        enemyAttack.SetStaminaRecoveryValue(staminaRecoveryValue);
        enemyAttack.SetStaminaRecoveryDelay(staminaRecoveryDelay);
        enemyAttack.SetStaminaRecoverySpeed(staminaRecoverySpeed);*/

        resetAttackRoutine = StartCoroutine(ResetAttack(0f));

        priorityListOne = new List<AttackPriority>();
        priorityListTwo = new List<AttackPriority>();
        priorityListThree = new List<AttackPriority>();
        priorityLists = new List<List<AttackPriority>>();

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
        if (isDead || outOfTetherRange)
            return;

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerInRange();
        MovementAnimation();

        /////////////////////////// Attack Animation Activated //////////////////
        //CheckStamina();
        if (inRange && !isStunned && !isAttacking && !isPicking && attackReady && currentStamina > 0)
            PickAttack();
    }

    private void FixedUpdate() {
        if (isDead || outOfTetherRange)
            return;

        // Follow Player ////////////////////////////////////////////////////////////////
        FollowPlayer();
    }

    /////////////////// Animation Helper Functions ////////
    private void PlayAnimation(string newAnim) {
        AnimHelper.ChangeAnimationState(anim, ref currentState, newAnim);
    }
    private void ReplayAnimation(string newAnim) {
        AnimHelper.ReplayAnimation(anim, ref currentState, newAnim);
    }
    private float GetAnimationLength(string newAnim) {
        return AnimHelper.GetAnimClipLength(ac, newAnim);
    }

    private void MovementAnimation() {
        if (!isStunned && !isAttacking) { 
            if (!enemyMovement.GetIsMoving())
                PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            else if (enemyMovement.GetIsMoving())
                PlayAnimation(EnemyAnimStates.ENEMY_RUN);
        }
    }

    ////////////////// Find Player AI ////////////////////
    private void EnemyInTetherRange() {
        outOfTetherRange = false;
        enemyMovement.SetFollow(true);
        enemyMovement.FindPlayerRepeating();
        PlayAnimation(EnemyAnimStates.ENEMY_RUN);

        CancelInvoke("CheckPlayerDistance");
    }

    private void EnemyOutsideTetherRange() {
        outOfTetherRange = true;
        enemyMovement.SetFollow(false);
        PlayAnimation(EnemyAnimStates.ENEMY_IDLE);

        InvokeRepeating("CheckPlayerDistance", 0f, 0.25f);
    }
    
    private void FollowPlayer() {
        if (!isStunned && !isAttacking)
            enemyMovement.FollowPlayer(attackReady);
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
        CheckPlayerDistance();

        if (!isAttacking)
            enemyMovement.CheckPlayerPos();

        //enemyMovement.CheckPlayerInRange(ref playerDetected);
        playerDetected = enemyMovement.CalculateDirectionToPlayer();

        if (!attackReady)
            inRange = false;
        else if (playerDetected.collider != null && !inRange) 
            inRange = true;
        else if (playerDetected.collider == null && inRange) 
            inRange = false;
    }

    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    IEnumerator ResetAttack(float value)
    {
        // called thru invoke in ResetStun()
        yield return new WaitForSeconds(value);
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);
        attackReady = true;
    }

    private void AttackActivated(int attackPoint)
    {
        //called thru animation event
        if (attackPoint != 0)
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
        isPicking = false;
        isAttacking = true;
        attackReady = false;
        staminaRecovery = false;
        enemyMovement.SetFollow(false);
        attackFollowHit = new RaycastHit2D();
        attackChosen = attackAnimations[attackIndex];
        if (staminaRecoveryRoutine != null)
            StopCoroutine(StaminaRecovery());

        SavePlayerPosition(3);
        PlayAnimation(attackChosen);
        tmpLength = GetAnimationLength(attackChosen);

        yield return new WaitForSeconds(tmpLength);
        if (!isAttacking)
            yield break;

        FinishAttack();
    }


    private void FinishAttack()
    {
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

/*        int tmp = Random.Range(0, 10);
        if (attackFromLeft)
        {
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = true;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = false;
        }
        else
        {
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = false;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = true;
        }*/
    }

    private void SavePlayerPosition(int flag) {
        if (flag == 1)
            abovePlayer = enemyMovement.GetAbovePlayer();
        else if (flag == 2)
            leftOfPlayer = enemyMovement.GetLeftOfPlayer();
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
                hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(attackDamages[attackIndex]);
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
        while (attackFollowThruVertical)
        {
            if (attackFollowFacePlayer[attackIndex]) {
                enemyMovement.CheckPlayerPos();
                SavePlayerPosition(1);
            }

            if (abovePlayer == 1)
            {
                newPosition = new Vector2(0f, attackFollowDistances[attackIndex])
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }
            else if (abovePlayer == -1)
            {
                newPosition = new Vector2(0f, -attackFollowDistances[attackIndex])
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }
            else if (abovePlayer == 0)
            {
                newPosition = Vector2.zero;
            }

            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) {
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
        attackFollowHit = new RaycastHit2D();
        yield break;
    }

    IEnumerator AttackFollowThroughHorizontal()
    {
        // Left right movement only
        while (attackFollowThruHorizontal)
        {
            if (attackFollowFacePlayer[attackIndex]) { 
                enemyMovement.CheckPlayerPos();
                SavePlayerPosition(2);
            }

            if (leftOfPlayer)
            {
                newPosition = new Vector2(attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
            }
            else
            {
                newPosition = new Vector2(-attackFollowDistances[attackIndex], 0f)
                                        * attackFollowThruSpeed
                                        * Time.fixedDeltaTime;
            }

            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) { 
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
        attackFollowHit = new RaycastHit2D();
        yield break;
    }

    IEnumerator AttackFollowThroughBoth()
    {
        while (attackFollowThruBoth)
        {
            if (attackFollowFacePlayer[attackIndex]) {
                // for changing the direction of enemy to continually face the player
                enemyMovement.CheckPlayerPos();
                // to check if enemy should move up or down/left or right
                SavePlayerPosition(3);
            }

            if (leftOfPlayer)
            {
                // if left of player, always move left
                if (abovePlayer == 1)
                {
                    newPosition = new Vector2(attackFollowDistances[attackIndex],
                                            attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == -1)
                {
                    newPosition = new Vector2(attackFollowDistances[attackIndex],
                                            -attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == 0)
                {
                    newPosition = new Vector2(attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
            }
            else if (!leftOfPlayer)
            {
                // if to the right of player, always move right
                if (abovePlayer == 1)
                {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex],
                                            attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == -1)
                {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex],
                                            -attackFollowDistances[attackIndex])
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
                else if (abovePlayer == 0)
                {
                    newPosition = new Vector2(-attackFollowDistances[attackIndex], 0f)
                                            * attackFollowThruSpeed
                                            * Time.fixedDeltaTime;
                }
            }

            attackFollowHit = enemyMovement.CalculateDirectionToMove(rb.position + newPosition);
            if (attackFollowHit.collider != null) {
                if (Vector2.Distance(rb.position, attackFollowHit.point) > 0.25f)
                    rb.MovePosition(rb.position + newPosition);
            } else
                rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
        attackFollowHit = new RaycastHit2D();
        yield break;
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
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }

        if (!outOfStamina && currentStamina <= 0)
        {
            currentStamina = 0;
            outOfStamina = true;
        }
        else if (outOfStamina && currentStamina >= maxStamina)
        {
            if (!isStunned || !isAttacking)
            {
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
        currentStamina -= value;
        CheckStamina();
    }

    /////////////// Enemy Is Hit ///////////////////////////////////////////////////////////
    IEnumerator ResetStun(float value) {
        yield return new WaitForSeconds(value);
        isStunned = false;

        StartCoroutine(enemyMovement.ResetStunFollow());
        if (!outOfStamina) {
            if (resetAttackRoutine != null)
                StopCoroutine(resetAttackRoutine);
            resetAttackRoutine = StartCoroutine(ResetAttack(attackDelay));
        }
    }

    IEnumerator ResetInvincible(float value) {
        yield return new WaitForSeconds(value);
        isInvincible = false;
    }

    public void EnemyHurt(int damageNum, float distance, Transform playerRef) {
        if (isDead || isInvincible)
            return;

        isStunned = true;
        isInvincible = true;
        enemyMovement.SetFollow(false);
        if (isAttacking) {
            if (attackAnimationRoutine != null)
                StopCoroutine(attackAnimationRoutine);
            FinishAttack();
        }

        AttackDeactivated();
        AttackFollowDeactivated();
        currentHealth -= damageNum;
        PushBack(distance, playerRef);

        // Play sounds
        playSound.PlayEnemyHit();

        if (currentHealth <= 0)
            StartCoroutine(Death());
        else {
            ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
            stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

            if (invincibleRoutine != null)
                StopCoroutine(invincibleRoutine);
            invincibleRoutine = StartCoroutine(ResetInvincible(stunDuration + 0.1f));

            if (stunRoutine != null)
                StopCoroutine(stunRoutine);
            stunRoutine = StartCoroutine(ResetStun(stunDuration + 0.1f));
        }
    }

    private void PushBack(float distance, Transform reference) {
        Vector2 newPosition;
        if (reference.transform.position.x > rb.position.x) {
            newPosition = new Vector2(-distance, 0f) + rb.position;
            rb.position = newPosition;
        }
        else if (reference.transform.position.x <= transform.position.x) {
            newPosition = new Vector2(distance, 0f) + rb.position;
            rb.position = newPosition;
        }
    }

    public bool GetIsDead() {
        return isDead;
    }

    IEnumerator Death() {
        isDead = true;
        enemyMovement.StopFindPlayer();
        GetComponent<Collider2D>().enabled = false;
        PlayAnimation(EnemyAnimStates.ENEMY_DEATH);
        var tmp = GetAnimationLength(EnemyAnimStates.ENEMY_DEATH);

        yield return new WaitForSeconds(tmp);
        Destroy(enemyMovement);
        Destroy(this);
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

    // ENEMY SPECIFIC ATTACKS /////////////////////////////////////////////////////////////////////////////


    // GIZMOS ////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(visualizePoint.position, (Vector2)visualizePoint.position + (Vector2.right * visualizeRange));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(visualizePoint.position, (Vector2)visualizePoint.position + (Vector2.left * visualizeRange));
        Gizmos.color = Color.green;
        if (rb != null)
            Gizmos.DrawLine(rb.position, rb.position + newPosition);
    }
}
