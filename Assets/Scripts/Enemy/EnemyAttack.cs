using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyAttack : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;
    private Rigidbody2D rb;
    private RuntimeAnimatorController ac;
    private EnemyMovement enemyMovement;

    private string currentState;

    [Header("Stamina Properties")]
    private int staminaRecoveryValue;
    private float staminaRecoveryDelay;
    private float staminaRecoverySpeed;

    private bool outOfStamina;
    private bool staminaRecovery;

    private int maxStamina;
    private int currentStamina;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private Transform visualizePoint;
    [SerializeField]
    private float visualizeRange;

    private RaycastHit2D hitBox;

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
    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        ac = anim.runtimeAnimatorController;
        StartCoroutine(ResetAttack(0f));

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

    private void PlayAnimation(string newAnim)
    {
        AnimHelper.ChangeAnimationState(anim, ref currentState, newAnim);
    }
    private void ReplayAnimation(string newAnim)
    {
        AnimHelper.ReplayAnimation(anim, ref currentState, newAnim);
    }
    private float GetAnimationLength(string newAnim)
    {
        return AnimHelper.GetAnimClipLength(ac, newAnim);
    }

    public IEnumerator ResetAttack(float value)
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

    public void AttackDeactivated()
    {
        //called thru animation event
        attackHitbox = false;
        //enemyMovement.FindPlayerRepeating();
    }

    public void PickAttack()
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

        //Debug.Log("restarting pick ");
        /*while (playerDistance >= attackRanges[attackIndex])
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
        if (staminaRecoveryRoutine != null)
            StopCoroutine(StaminaRecovery());

        //Debug.Log("ATTACKINGGGGGGGGGGGGGGGG");
        attackChosen = attackAnimations[attackIndex];
        abovePlayer = enemyMovement.GetAbovePlayer();
        leftOfPlayer = enemyMovement.GetLeftOfPlayer();

        PlayAnimation(attackChosen);
        tmpLength = GetAnimationLength(attackChosen);

        yield return new WaitForSeconds(tmpLength);
        if (!isAttacking)
            yield break;

        FinishAttack();
    }


    public void FinishAttack()
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

    public IEnumerator CheckHitBox()
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

    public void AttackFollowDeactivated()
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

            rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
        yield break;
    }

    IEnumerator AttackFollowThroughHorizontal()
    {
        // Left right movement only
        while (attackFollowThruHorizontal)
        {
            if (attackFollowFacePlayer[attackIndex])
                enemyMovement.CheckPlayerPos();

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

            rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
        yield break;
    }

    IEnumerator AttackFollowThroughBoth()
    {
        while (attackFollowThruBoth)
        {
            if (attackFollowFacePlayer[attackIndex]) {
                enemyMovement.CheckPlayerPos();
                abovePlayer = enemyMovement.GetAbovePlayer();
            }

            if (leftOfPlayer) {
                // if left of player, always move left
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
                // if to the right of player, always move right
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

            rb.MovePosition(rb.position + newPosition);
            yield return null;
        }
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
        currentStamina -= value;
        CheckStamina();
    }

    // GET/SET METHODS ///////////////////////////////////////////////////////////////////////
    public void SetMaxStamina(int value) {
        maxStamina = currentStamina = value;
    }
    public void SetStaminaRecoveryValue(int value) {
        staminaRecoveryValue = value;
    }
    public void SetStaminaRecoveryDelay(float value) {
        staminaRecoveryDelay = value;
    }

    public void SetStaminaRecoverySpeed(float value) {
        staminaRecoverySpeed = value;
    }
}
