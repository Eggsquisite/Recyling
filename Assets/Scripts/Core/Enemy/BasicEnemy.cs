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
    private Animator anim = null;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;
    private EnemyMovement enemyMovement;
    private EnemySounds playSound;
    private EnemyType enemyType;
    private ArcherDroid archerArrow;
    private string currentState;


    [Header("Enemy Stats")]
    [SerializeField]
    private float spawnDelay;

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
    private float emptyStaminaDelayMult;

    /*[SerializeField]
    private float deathFadeTime;*/

    private bool isDead;
    private bool isInvincible;
    private bool staminaRecovery;
    private bool outOfStamina;
    private bool isSpawning;

    private int currentHealth;
    private int currentStamina;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform visualizePoint;
    [SerializeField]
    private List<Transform> attackPoints;

    [SerializeField]
    private LayerMask playerLayer;

    private RaycastHit2D playerDetected;
    private RaycastHit2D hitBox;
    private RaycastHit2D hitBox2;

    [Header("Attack Properties")]
    [SerializeField]
    private float stunDelay;

    [SerializeField] 
    private float visualizeRange;

    [SerializeField]
    private List<string> attackIndexes;
    [SerializeField]
    private List<int> attackPriority;
    [SerializeField]
    private List<float> attackRanges;
    [SerializeField]
    private List<int> attackDamages;
    [SerializeField]
    private List<float> attackFollowDistances;

    [SerializeField]
    private float minAttackDelay;
    [SerializeField]
    private float maxAttackDelay;

    private List<AttackPriority> priorityListOne;
    private List<AttackPriority> priorityListTwo;
    private List<AttackPriority> priorityListThree;
    private List<List<AttackPriority>> priorityLists;

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

    private string attackChosen;
    private int attackIndex;
    private float attackFollowThruSpeed;
    private float attackDelay;
    private float stunDuration;
    private float playerDistance;
    private Vector2 newPosition;

    // Start is called before the first frame update
    void Awake() {
        SetupVariables();
    }

    private void Update() {
        if (isDead || isSpawning)
            return;

        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHitbox)
            CheckHitBox();

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerInRange();
        MovementAnimation();
        FollowPlayer();

        /////////////////////////// Attack Animation Activated //////////////////
        //CheckStamina();
        if (inRange && !isStunned && !isAttacking && !isPicking && attackReady && currentStamina > 0)
            PickAttack();

        if (attackFollowThruBoth) {
            AttackFollowThroughHorizontal();
            AttackFollowThroughVertical();
        } else if (attackFollowThruVertical) {
            AttackFollowThroughVertical(); 
        } else if (attackFollowThruHorizontal) {
            AttackFollowThroughHorizontal();
        }
    }

    private void SetupVariables()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (enemyType == null) enemyType = GetComponent<EnemyType>();
        if (playSound == null) playSound = GetComponent<EnemySounds>();
        if (enemyMovement == null) enemyMovement = GetComponent<EnemyMovement>();

        if (archerArrow == null) archerArrow = GetComponent<ArcherDroid>();
        if (archerArrow != null)
            archerArrow.SetDamage(attackDamages[0]);

        //enemyMovement.FindPlayerRepeating();
        ac = anim.runtimeAnimatorController;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        isSpawning = true;
        attackReady = true;
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);

        StartCoroutine(enemyMovement.SpawningDelay(spawnDelay));
        StartCoroutine(SpawningEnding());

        priorityListOne = new List<AttackPriority>();
        priorityListTwo = new List<AttackPriority>();
        priorityListThree = new List<AttackPriority>();
        priorityLists = new List<List<AttackPriority>>();

        for (int i = 0; i < attackIndexes.Count; i++)
        {
            var tmp = attackPriority[i];
            if (tmp == 1)
                priorityListOne.Add(new AttackPriority(i, attackIndexes[i]));
            else if (tmp == 2)
                priorityListTwo.Add(new AttackPriority(i, attackIndexes[i]));
            else if (tmp == 3)
                priorityListThree.Add(new AttackPriority(i, attackIndexes[i]));
        }

        priorityLists.Add(priorityListOne);
        priorityLists.Add(priorityListTwo);
        priorityLists.Add(priorityListThree);
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
    IEnumerator SpawningEnding() {
        yield return new WaitForSeconds(spawnDelay + 0.5f);
        isSpawning = false;
    }
    
    private void FollowPlayer() {
        if (!isStunned && !isAttacking)
            enemyMovement.FollowPlayer(attackFromLeft, attackReady);
    }

    private void CheckPlayerInRange() {
        playerDistance = enemyMovement.GetPlayerDistance();

        if (!isAttacking)
            enemyMovement.CheckPlayerPos();

        enemyMovement.CheckPlayerInRange(ref playerDetected);

        if (!attackReady)
            inRange = false;
        else if (playerDetected.collider != null && !inRange) 
            inRange = true;
        else if (playerDetected.collider == null && inRange) 
            inRange = false;
    }

    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    private void ResetStun() {
        // called thru invoke event in EnemyHurt()
        isStunned = false;

        StartCoroutine(enemyMovement.ResetStunFollow());
        if (!outOfStamina) { 
            if (IsInvoking("ResetAttack"))
                CancelInvoke("ResetAttack");
            
            Invoke("ResetAttack", attackDelay);
        }
    }

    private void ResetInvincible() {
        // called thru invoke event in EnemyHurt()
        isInvincible = false;
    }

    private void ResetAttack() {
        // called thru invoke in ResetStun()
        attackReady = true;
    }

    private void AttackActivated() {
        //called thru animation event
        enemyMovement.StopFindPlayer();
        enemyMovement.FindPlayer();
        attackHitbox = true;
    }

    private void AttackDeactivated() {
        //called thru animation event
        attackHitbox = false;
        //enemyMovement.FindPlayerRepeating();
    }

    private void PickAttack() {
        isPicking = true;
        float tmp;
        for (int j = 0; j < priorityLists.Count; j++)
        {
            // priorityLists[j] is the list that contains all attacks of that specific priority
            if (priorityLists[j].Count > 0)
            {
                for (int i = 0; i < priorityLists[j].Count; i++)
                {
                    tmp = attackRanges[priorityLists[j][i].index];
                    //Debug.Log(priorityLists[j][i].attackName + " Range: " + tmp + " Index: " + priorityLists[j][i].index + " PlayerDistance: " + playerDistance);
                    if (playerDistance < tmp)
                    {
                        attackIndex = priorityLists[j][i].index;

                        //Debug.Log("Found attack, stopping pick attack: " + j + " " + i);
                        isPicking = false;
                        StartCoroutine(AttackAnimation());
                        return;
                    }
                    // keep iterating
                }
                //Debug.Log("iterated through priority list " + j + ", moving onto " + (j + 1));
            }
        }
        isPicking = false;

        //Debug.Log("restarting pick ");
        /*while (playerDistance >= attackRanges[attackIndex])
        {
            Debug.Log("Picking new attack");
            attackIndex = Random.Range(0, attackIndexes.Count);
            yield return null;
        }
        StartCoroutine(AttackAnimation());*/
    }

    IEnumerator AttackAnimation() {
        isPicking = false;
        isAttacking = true;
        attackReady = false;
        staminaRecovery = false;
        enemyMovement.SetFollow(false);
        StopCoroutine(StaminaRecovery());
        float tmpLength;

        /*Debug.Log(attackIndex + "attack chosen: " + attackIndexes[attackIndex]);
        Debug.Log("ATTACKINGGGGGGGGGGGGGGGG");*/
        attackChosen = attackIndexes[attackIndex];
        PlayAnimation(attackChosen);
        tmpLength = GetAnimationLength(attackChosen);

        yield return new WaitForSeconds(tmpLength);
        FinishAttack();
    }


    private void FinishAttack() {
        isAttacking = false;
        staminaRecovery = true;
        AttackDeactivated();
        AttackFollowDeactivated();
        StartCoroutine(StaminaRecovery());
        StartCoroutine(enemyMovement.ResetAttackFollow());

        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");
        if (!outOfStamina)
            Invoke("ResetAttack", attackDelay);

/*        int tmp = Random.Range(0, 10);
        if (attackFromLeft) { 
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = true;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = false;
        } else {
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = false;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = true;
        }*/
    }

    private void CheckHitBox() {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (enemyMovement.GetLeftOfPlayer()) 
            hitBox = Physics2D.Raycast(attackPoints[attackIndex].position, Vector2.right, attackRanges[attackIndex], playerLayer);
        else 
            hitBox = Physics2D.Raycast(attackPoints[attackIndex].position, Vector2.left, attackRanges[attackIndex], playerLayer);

        if (hitBox.collider != null) {
            hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(attackDamages[attackIndex]);
        }
    }

/*    private void CheckHitBox2()
    {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (enemyMovement.GetLeftOfPlayer())
            hitBox2 = Physics2D.Raycast(attackPoint2.position, Vector2.right, attackRanges[attackIndex], playerLayer);
        else
            hitBox2 = Physics2D.Raycast(attackPoint2.position, Vector2.left, attackRanges[attackIndex], playerLayer);

        if (hitBox2.collider != null) {
            hitBox2.collider.GetComponentInChildren<Player>().PlayerHurt(attackDamages[attackIndex]);
        }
    }*/

    private void AttackFollowBothActivated(int value) {
        // called thru animation events
        attackFollowThruBoth = true;
        attackFollowThruSpeed = value;
    }

    private void AttackFollowVerticalActivated(int value){
        // called thru animation events
        attackFollowThruVertical = true;
        attackFollowThruSpeed = value;
    }

    private void AttackFollowHorizontalActivated(int value) {
        // called thru animation events
        attackFollowThruHorizontal = true;
        attackFollowThruSpeed = value;
    }

    private void AttackFollowDeactivated() {
        // called thru animation events
        attackFollowThruBoth = false;
        attackFollowThruVertical = false;
        attackFollowThruHorizontal = false;
    }

    private void AttackFollowThroughVertical() {
        // up down movement only
        if (enemyMovement.GetPlayerPosition().y > transform.position.y) {
            newPosition = new Vector2(0f, attackFollowDistances[attackIndex]) + (Vector2)transform.position;
        }
        else if (enemyMovement.GetPlayerPosition().y <= transform.position.y) {
            newPosition = new Vector2(0f, -attackFollowDistances[attackIndex]) + (Vector2)transform.position;
        } else if (enemyMovement.GetPlayerPosition().y == transform.position.y) 
            return;

        transform.position = Vector2.MoveTowards(transform.position, newPosition, attackFollowThruSpeed * Time.deltaTime);
    }

    private void AttackFollowThroughHorizontal() {
        // Left right movement only
        var tmp = enemyMovement.GetLeftOfPlayer();
        if (enemyMovement.GetPlayerPosition().x >= transform.position.x && tmp) {
            newPosition = new Vector2(attackFollowDistances[attackIndex], 0f) + (Vector2)transform.position;
        }
        else if (enemyMovement.GetPlayerPosition().x <= transform.position.x && !tmp) {
            newPosition = new Vector2(-attackFollowDistances[attackIndex], 0f) + (Vector2)transform.position;
        }
        
        transform.position = Vector2.MoveTowards(transform.position, newPosition, attackFollowThruSpeed * Time.deltaTime);
    }

    public bool GetIsAttacking() {
        return isAttacking;
    }

    // STAMINA CODE ////////////////////////////////////////////////////////

    IEnumerator StaminaRecovery() {
        yield return new WaitForSeconds(staminaRecoveryDelay);
        while (currentStamina < maxStamina && staminaRecovery && !isAttacking)
        {
            currentStamina += staminaRecoveryValue;
            CheckStamina();
            yield return new WaitForSeconds(staminaRecoverySpeed);
        }
    }

    private void CheckStamina() {
        if (currentStamina > maxStamina) { 
            currentStamina = maxStamina;
        }

        if (!outOfStamina && currentStamina <= 0) {
            currentStamina = 0;
            outOfStamina = true;
        }
        else if (outOfStamina && currentStamina >= maxStamina) {
            if (!isStunned || !isAttacking)
                ResetAttack();  
            outOfStamina = false;
        } 
    }

    private void ConsumeStamina(int value) {
        // called thru animation event
        currentStamina -= value;
        CheckStamina();
    }

    /////////////// Enemy Is Hit //////////////////
    public void EnemyHurt(int damageNum, float distance, Transform playerRef) {
        if (isDead || isInvincible)
            return;

        isStunned = true;
        isInvincible = true;
        enemyMovement.SetFollow(false);
        if (isAttacking) {
            StopCoroutine("AttackAnimation");
            FinishAttack();
        }

        AttackDeactivated();
        AttackFollowDeactivated();
        currentHealth -= damageNum;
        PushBack(distance, playerRef);

        // Play sounds
        playSound.PlayEnemyHit();

        if (currentHealth <= 0)
            Death();
        else {
            ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
            stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

            Invoke("ResetInvincible", stunDuration + 0.1f);
            if (IsInvoking("ResetStun"))
                return;
            Invoke("ResetStun", stunDuration + 0.1f);
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
        enemyMovement.StopFindPlayer();
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

    // ENEMY SPECIFIC ATTACKS /////////////////////////////////////////////////////////////////////////////


    // GIZMOS ////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(visualizePoint.position, (Vector2)visualizePoint.position + (Vector2.right * visualizeRange));
    }
}
