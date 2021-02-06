using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private enum EnemyAttacks
    {
        BasicAttack1,
        BasicAttack2,
        BasicAttack3
    };

    private Animator anim;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;
    private EnemySounds playSound;
    private string currentState;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private LayerMask playerLayer;

    private RaycastHit2D playerDetected;
    private RaycastHit2D hitBox;

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

    [SerializeField] [Range(0, 5f)]
    private float attackVisualizer;
    [SerializeField] [Range(0, 5f)]
    private float attackRange1;
    [SerializeField] [Range(0, 5f)]
    private float attackRange2;
    [SerializeField] [Range(0, 5f)]
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

    private bool inRange;
    private bool isStunned;
    private bool isAttacking;
    private bool attackReady;
    private bool attackHitbox;
    private int attackCounter;
    private int currentAttackDamage;
    private float currentAttackRange;
    private float attackDelay;
    private float stunDuration;

    [Header("Follow Player")]
    [SerializeField] [Range(0, 5f)]
    private float minMoveSpeed;
    [SerializeField] [Range(0, 5f)]
    private float maxMoveSpeed;

    [SerializeField] [Range(0.5f, 5f)]
    private float minOffset;
    [SerializeField] [Range(0.5f, 5f)]
    private float maxOffset;

    private bool canFollow;
    private bool leftOfPlayer;
    private float baseMoveSpeed;
    private float xScaleValue;
    private Vector2 leftOffset, rightOffset, playerChar;

    // Start is called before the first frame update
    void Start() {
        SetupVariables();
        InvokeRepeating("FindPlayer", 1f, 0.25f);
    }

    private void Update() {
        if (isDead)
            return;

        ///////////////////// Attack Hitbox Activated ///////////////////////////
        if (attackHitbox)
            CheckHitBox();

        ///////////////////// Follow Player /////////////////////////////////////
        CheckPlayerInRange();
        FollowPlayer();

        /////////////////////////// Attack Animation Activated //////////////////
        if (inRange && !isStunned && !isAttacking && attackReady) {
            AttackAnimation();
        }
    }

    private void SetupVariables()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (playSound == null) playSound = GetComponent<EnemySounds>();
        ac = anim.runtimeAnimatorController;
        ChooseAttack(attackChosen);

        canFollow = true;
        attackReady = true;
        xScaleValue = transform.localScale.x;

        if (playerChar.x > transform.position.x) {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x) {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

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
    private void FindPlayer() {
        if (canFollow) {
            playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
        }
    }

    private void FollowPlayer() {
        /*if (inRange && !canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            Debug.Log("1");
        }
        else if (!canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
            Debug.Log("2");
        }*/
        if (inRange && !attackReady && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_IDLE);
        }
        // ENEMY MOVEMENT ///////////////////////////////////////////////
        else if (canFollow && !isStunned && !isAttacking) {
            PlayAnimation(EnemyAnimStates.ENEMY_RUN);

            if (leftOfPlayer) {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, baseMoveSpeed * Time.deltaTime);
            }
            else {
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, baseMoveSpeed * Time.deltaTime);
            }
        }
    }

    private void CheckPlayerPos()
    {
        // Set variable of leftOfPlayer
        if (playerChar.x > transform.position.x && !leftOfPlayer) {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer) {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }
    }

    private void CheckPlayerInRange()
    {
        CheckPlayerPos();

        if (leftOfPlayer)
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.right, currentAttackRange, playerLayer);
        else
            playerDetected = Physics2D.Raycast(attackPoint.position, Vector2.left, currentAttackRange, playerLayer);

        if (playerDetected.collider != null && !inRange) {
            inRange = true;
/*            Debug.Log(name + " is IN range!");*/
        }
        else if (playerDetected.collider == null && inRange) {
            inRange = false;
/*            Debug.Log(name + " is OUT OF range!");*/
        }
    }

    private void ResetFollow() {
        canFollow = true;
        FindPlayer();
    }
    private void ResetStun() {
        ResetFollow();
        isStunned = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", stunDelay);
    }
    private void ResetInvincible() {
        isInvincible = false;
    }
    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    private void ResetAttack() {
        attackReady = true;
    }
    private void FinishAttack() {
        FindAttack();
        ResetFollow();
        isAttacking = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", attackDelay);
    }
    private void AttackActivated() {
        //called thru animation event
        attackHitbox = true;
    }
    private void AttackDeactivated() {
        //called thru animation event
        attackHitbox = false;
    }

    private void CheckHitBox() {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (leftOfPlayer)
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.right, currentAttackRange, playerLayer);
        else
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.left, currentAttackRange, playerLayer);

        if (hitBox.collider != null) {
            hitBox.collider.GetComponentInChildren<Player>().PlayerHurt(currentAttackDamage);
        }
    }

    private void AttackAnimation() {
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

        Invoke("FinishAttack", tmp);
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
    private void FindAttack() {
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
        if (!leftOfPlayer)
            Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.left * attackVisualizer));
        else
            Gizmos.DrawLine(attackPoint.position, (Vector2)attackPoint.position + (Vector2.right * attackVisualizer));
    }   
}
