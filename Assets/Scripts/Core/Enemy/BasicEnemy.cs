using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyAttacks
{
    BasicAttack1,
    BasicAttack2,
    BasicAttack3
};

public class BasicEnemy : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;
    private EnemyMovement enemyMovement;
    private EnemySounds playSound;
    private EnemyType enemyType;
    private string currentState;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private Transform attackPoint2;

    [SerializeField]
    private LayerMask playerLayer;

    private RaycastHit2D playerDetected;
    private RaycastHit2D hitBox;
    private RaycastHit2D hitBox2;

    [Header("Enemy Stats")]
    [SerializeField]
    private int health;

    /*[SerializeField]
    private float deathFadeTime;*/

    private bool isDead;
    private bool isInvincible;

    [Header("Attack Properties")]
    [SerializeField]
    private EnemyAttacks attackChosen;
    [SerializeField]
    private float stunDelay;

    [SerializeField] 
    private float visualizeRange;
    [SerializeField]
    private float detectRange;
    [SerializeField] 
    private float attackRange1;
    [SerializeField] 
    private float attackRange2;
    [SerializeField] 
    private float attackRange3;

    [SerializeField]
    private int attackDamage1;
    [SerializeField]
    private int attackDamage2;
    [SerializeField]
    private int attackDamage3;

    [SerializeField]
    private float minAttackDelay;
    [SerializeField]
    private float maxAttackDelay;
    [SerializeField]
    private float attackFollowDistance;

    private bool inRange;
    private bool isStunned;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private bool attackHitbox2;
    private bool attackFromLeft;
    private bool attackFollowThru;

    private int attackCounter;
    private int currentAttackDamage;
    private float currentAttackRange;
    private float currentAttackRange2;
    private float attackFollowThruSpeed;
    private float attackDelay;
    private float stunDuration;
    private Vector2 newPosition;

    [Header("Follow Player")]
    [SerializeField]
    private float followDelay;

    [SerializeField] 
    private float minMoveSpeed;
    [SerializeField] 
    private float maxMoveSpeed;

    [SerializeField] 
    private float minOffset;
    [SerializeField] 
    private float maxOffset;

    private bool isMoving;
    private bool canFollow;
    private float xScaleValue;
    private float currentSpeed;
    private float baseMoveSpeed;
    private Vector2 leftOffset, rightOffset, dist;
    private Vector2 lastUpdatePos = Vector2.zero;

    // Start is called before the first frame update
    void Start() {
        SetupVariables();
        /*InvokeRepeating("FindPlayer", 1f, followDelay);*/
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHitbox)
            CheckHitBox();
        if (attackHitbox2)
            CheckHitBox2();

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerInRange();
        MovementAnimation();
        FollowPlayer();

        /////////////////////////// Attack Animation Activated //////////////////
        if (inRange && !isStunned && !isAttacking && attackReady) {
            StartCoroutine(AttackAnimation());
        }

        if (attackFollowThru) {
            AttackFollowThroughHorizontal();
            AttackFollowThroughVertical();
        }
    }

    private void SetupVariables()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (playSound == null) playSound = GetComponent<EnemySounds>();
        if (enemyType == null) enemyType = GetComponent<EnemyType>();
        if (enemyMovement == null) enemyMovement = GetComponent<EnemyMovement>();

        enemyMovement.FindPlayer();
        ac = anim.runtimeAnimatorController;
        ChooseAttack(attackChosen);

        canFollow = true;
        attackReady = true;
        xScaleValue = transform.localScale.x;

        baseMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);

        leftOffset = new Vector2(Random.Range(-minOffset, -maxOffset), 0f);
        rightOffset = new Vector2(Random.Range(minOffset, maxOffset), 0f);
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

    ////////////////// Find Player AI ////////////////////
/*    private void FindPlayer() {
       playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
    }*/

    IEnumerator FindPlayer() {
        yield return new WaitForSeconds(enemyMovement.GetFollowDelay());
        enemyMovement.FindPlayerRepeating();
    }

    private void MovementAnimation() {
        if (!isStunned && !isAttacking) { 
            if (!enemyMovement.GetIsMoving())
                PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            else if (enemyMovement.GetIsMoving())
                PlayAnimation(EnemyAnimStates.ENEMY_RUN);
        }
    }

    private void FollowPlayer() {
        /*if (canFollow && !isStunned && !isAttacking) {
            //PlayAnimation(EnemyAnimStates.ENEMY_RUN);
            IsMoving();

            if (attackFromLeft) {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, baseMoveSpeed * Time.deltaTime);
                //rb.velocity = new Vector2(transform.position.x - (playerChar.x + leftOffset.x), transform.position.y - (playerChar.y + leftOffset.y));
            }
            else {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, baseMoveSpeed * Time.deltaTime);
                //rb.velocity = new Vector2(transform.position.x - (playerChar.x + rightOffset.x), transform.position.y - (playerChar.y + rightOffset.y));
            }
        }*/

        if (!isStunned && !isAttacking)
            enemyMovement.FollowPlayer(attackFromLeft);
    }

/*    private void IsMoving() {
        dist = (Vector2)transform.position - lastUpdatePos;
        currentSpeed = dist.magnitude / Time.deltaTime;
        lastUpdatePos = transform.position;

        if (currentSpeed > 0 && !isMoving)
            isMoving = true;
        else if (currentSpeed <= 0 && isMoving)
            isMoving = false;
    }*/

    private void CheckPlayerPos()
    {
        if (isAttacking)
            return;

        enemyMovement.CheckPlayerPos();

        /*// Set variable of leftOfPlayer
        if (playerChar.x > transform.position.x && !leftOfPlayer) {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer) {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }*/
    }

    private void CheckPlayerInRange()
    {
        CheckPlayerPos();

        if (enemyMovement.GetLeftOfPlayer())
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.right, detectRange, playerLayer);
        else if (!enemyMovement.GetLeftOfPlayer())
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.left, detectRange, playerLayer);

        if (!attackReady)
            inRange = false;
        else if (playerDetected.collider != null && !inRange) {
            inRange = true;
/*            Debug.Log(name + " is IN range!");*/
        }
        else if (playerDetected.collider == null && inRange) {
            inRange = false;
/*            Debug.Log(name + " is OUT OF range!");*/
        } 
    }

/*    private void ResetFollow() {
        canFollow = true;
        FindPlayer();
    }*/
    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    private void ResetStun() {
        enemyMovement.ResetFollow();
        isStunned = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", stunDelay);
    }
    private void ResetInvincible() {
        isInvincible = false;
    }
    private void ResetAttack() {
        attackReady = true;
    }
    private void FinishAttack() {
        enemyMovement.ResetFollow();
        FindNextAttack();
        AttackFollowDeactivated();

        isAttacking = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", attackDelay);

        int tmp = Random.Range(0, 10);
        if (tmp >= 0 && tmp < 5)
            attackFromLeft = true;
        else if (tmp >= 5 && tmp < 10)
            attackFromLeft = false;
    }
    private void AttackActivated() {
        //called thru animation event
        /*CancelInvoke("FindPlayer");*/
        enemyMovement.StopFindPlayer();
        enemyMovement.FindPlayer();
        attackHitbox = true;

        /*if (flag == 0)
            attackFollowThruHorizontal = true;
        else if (flag == 1)
            attackFollowThruVertical = true;
        else if (flag == 2)
            attackFollowThruBoth = true;*/
    }
    private void AttackDeactivated() {
        //called thru animation event
        attackHitbox = false;
        StartCoroutine("FindPlayer");
        //InvokeRepeating("FindPlayer", 0f, followDelay);

        /*attackFollowThruBoth = false;
        attackFollowThruVertical = false;
        attackFollowThruHorizontal = false;*/
    }

    private void CheckHitBox() {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (enemyMovement.GetLeftOfPlayer()) 
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.right, currentAttackRange, playerLayer);
        else if (!enemyMovement.GetLeftOfPlayer())
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.left, currentAttackRange, playerLayer);

        if (hitBox.collider != null) {
            hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(currentAttackDamage);
        }
    }

    private void CheckHitBox2()
    {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (enemyMovement.GetLeftOfPlayer())
            hitBox2 = Physics2D.Raycast(attackPoint2.position, Vector2.right, currentAttackRange2, playerLayer);
        else if (!enemyMovement.GetLeftOfPlayer())
            hitBox2 = Physics2D.Raycast(attackPoint2.position, Vector2.left, currentAttackRange2, playerLayer);

        if (hitBox2.collider != null) {
            hitBox2.collider.GetComponentInChildren<Player>().PlayerHurt(currentAttackDamage);
        }
    }

    IEnumerator AttackAnimation() {
        canFollow = false;
        isAttacking = true;
        attackReady = false;
        var tmp = 0f;

        if (attackChosen == EnemyAttacks.BasicAttack1) { 
            PlayAnimation(EnemyAnimStates.ENEMY_ATTACK1);
            tmp = GetAnimationLength(EnemyAnimStates.ENEMY_ATTACK1);
        }
        else if (attackChosen == EnemyAttacks.BasicAttack2) { 
            PlayAnimation(EnemyAnimStates.ENEMY_ATTACK2);
            tmp = GetAnimationLength(EnemyAnimStates.ENEMY_ATTACK2);
        }
        else if (attackChosen == EnemyAttacks.BasicAttack3) { 
            PlayAnimation(EnemyAnimStates.ENEMY_ATTACK3);
            tmp = GetAnimationLength(EnemyAnimStates.ENEMY_ATTACK3);
        }

        yield return new WaitForSeconds(tmp);
        FinishAttack();
        //Invoke("FinishAttack", tmp);
    }

   private void ChooseAttack(int index) {
        if (index == 1)
        {
            attackChosen = EnemyAttacks.BasicAttack1;
            currentAttackRange = attackRange1;
        }
        else if (index == 2)
        {
            attackChosen = EnemyAttacks.BasicAttack2;
            currentAttackRange = attackRange2;
        }
        else if (index == 3)
        {
            attackChosen = EnemyAttacks.BasicAttack3;
            currentAttackRange = attackRange3;
        }
    }
    
    private void ChooseAttack(EnemyAttacks attackChoice)
    {
        if (attackChoice == EnemyAttacks.BasicAttack1)
        {
            attackChosen = EnemyAttacks.BasicAttack1;
            currentAttackRange = attackRange1;
            currentAttackDamage = attackDamage1;
        }
        else if (attackChoice == EnemyAttacks.BasicAttack2)
        {
            attackChosen = EnemyAttacks.BasicAttack2;
            currentAttackRange = attackRange2;
            currentAttackDamage = attackDamage2;
        }
        else if (attackChoice == EnemyAttacks.BasicAttack3)
        {
            attackChosen = EnemyAttacks.BasicAttack3;
            currentAttackRange = attackRange3;
            currentAttackDamage = attackDamage3;
        }
    }

    private void AttackFollowActivated(int value) {
        attackFollowThru = true;
        attackFollowThruSpeed = value;
    }

    private void AttackFollowDeactivated() {
        attackFollowThru = false;
    }

    private void AttackFollowThroughVertical() {
        // up down movement only
        if (enemyMovement.GetPlayerPosition().y > transform.position.y) {
            newPosition = new Vector2(0f, 1) + (Vector2)transform.position;
        }
        else if (enemyMovement.GetPlayerPosition().y <= transform.position.y) {
            newPosition = new Vector2(0f, -1) + (Vector2)transform.position;
        } else if (enemyMovement.GetPlayerPosition().y == transform.position.y) 
            return;

        transform.position = Vector2.MoveTowards(transform.position, newPosition, attackFollowThruSpeed * Time.deltaTime);
    }

    private void AttackFollowThroughHorizontal() {
        // Left right movement only
        if (enemyMovement.GetPlayerPosition().x >= transform.position.x) {
            newPosition = new Vector2(attackFollowDistance, 0f) + (Vector2)transform.position;
        }
        else if (enemyMovement.GetPlayerPosition().x <= transform.position.x) {
            newPosition = new Vector2(-attackFollowDistance, 0f) + (Vector2)transform.position;
        }
        
        transform.position = Vector2.MoveTowards(transform.position, newPosition, attackFollowThruSpeed * Time.deltaTime);
    }

/*    private void AttackFollowThroughBoth(float distance) {
        FindPlayer();
        Vector2 newPosition;
        Debug.Log(playerChar.y + " - " + transform.position.y + " = " + (playerChar.y - transform.position.y));
        if (playerChar.x >= transform.position.x) {
            newPosition = new Vector2(distance, playerChar.y - transform.position.y * (1 / distance)) + (Vector2)transform.position;
            transform.position = newPosition;
        } else if (playerChar.x < transform.position.x) {
            newPosition = new Vector2(-distance, playerChar.y - transform.position.y * (1 / distance)) + (Vector2)transform.position;
            transform.position = newPosition;
        }
    }*/

    /////////////// Enemy Is Hit //////////////////
    public void EnemyHurt(int damageNum, float distance, Transform playerRef) {
        if (isDead || isInvincible)
            return;

        isStunned = true;
        canFollow = false;
        attackReady = false;
        isInvincible = true;
        PushBack(distance, playerRef);

        if (IsInvoking("ResetStun"))
            CancelInvoke("ResetStun");

        health -= damageNum;
        AttackDeactivated();
        playSound.PlayEnemyHit();

        if (health <= 0)
            Death();
        else { 
            ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
            stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);

            Invoke("ResetInvincible", 0.2f);
            Invoke("ResetStun", stunDuration + 0.25f);
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
        CancelInvoke("FindPlayer");
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
    private void FindNextAttack() {
        if (name.Contains("MudGuard"))
            MudguardAttack();
    }

    private void MudguardAttack() {
        attackCounter++;
        if (attackCounter % 3 == 0)
            ChooseAttack(EnemyAttacks.BasicAttack1);
        else
            ChooseAttack(EnemyAttacks.BasicAttack2);
    }

    // GIZMOS ////////////////////////////////////////////////////////////////////////////////

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * visualizeRange));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(attackPoint2.position, (Vector2)attackPoint2.position + (Vector2.right * visualizeRange));
    }
}
