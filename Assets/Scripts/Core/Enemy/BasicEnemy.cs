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
    private ArcherDroid archerArrow;
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

    // Start is called before the first frame update
    void Start() {
        SetupVariables();
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
        if (archerArrow == null) archerArrow = GetComponent<ArcherDroid>();
        if (archerArrow != null)
            archerArrow.SetDamage(attackDamage1);

        enemyMovement.FindPlayerRepeating();
        ac = anim.runtimeAnimatorController;
        ChooseAttack(attackChosen);

        attackReady = true;
        attackDelay = Random.Range(minAttackDelay, maxAttackDelay);
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
    private void FollowPlayer() {
        if (!isStunned && !isAttacking)
            enemyMovement.FollowPlayer(attackFromLeft, attackReady);
    }

    private void CheckPlayerInRange() {
        if (!isAttacking)
            enemyMovement.CheckPlayerPos();

        enemyMovement.CheckPlayerInRange(ref playerDetected);

        if (!attackReady)
            inRange = false;
        else if (playerDetected.collider != null && !inRange) {
            inRange = true;
            //Debug.Log(name + " is IN range!");
        }
        else if (playerDetected.collider == null && inRange) {
            inRange = false;
            //Debug.Log(name + " is OUT OF range!");
        } 
    }

    ////////////////// Attack Code ////////////////////////////////////////////////////////////
    private void ResetStun() {
        // called thru invoke event in EnemyHurt()
        isStunned = false;
        StartCoroutine(enemyMovement.ResetStunFollow());
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", stunDelay);
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

    IEnumerator AttackAnimation() {
        isAttacking = true;
        attackReady = false;
        enemyMovement.SetFollow(false);
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
    }

    private void FinishAttack() {
        FindNextAttack();
        AttackFollowDeactivated();
        StartCoroutine(enemyMovement.ResetAttackFollow());

        isAttacking = false;
        if (IsInvoking("ResetAttack"))
            CancelInvoke("ResetAttack");

        Invoke("ResetAttack", attackDelay);

        int tmp = Random.Range(0, 10);
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
        }
    }

    private void CheckHitBox() {
        //Collider2D[] playerDetectedPlayer = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);
        if (enemyMovement.GetLeftOfPlayer()) 
            hitBox = Physics2D.Raycast(attackPoint.position, Vector2.right, currentAttackRange, playerLayer);
        else 
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
        else
            hitBox2 = Physics2D.Raycast(attackPoint2.position, Vector2.left, currentAttackRange2, playerLayer);

        if (hitBox2.collider != null) {
            hitBox2.collider.GetComponentInChildren<Player>().PlayerHurt(currentAttackDamage);
        }
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
        // called thru animation event
        attackFollowThru = true;
        attackFollowThruSpeed = value;
    }

    private void AttackFollowDeactivated() {
        // called thru animation event
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

    public bool GetIsAttacking() {
        return isAttacking;
    }

    public int GetDamage() {
        return currentAttackDamage;
    }

    /////////////// Enemy Is Hit //////////////////
    public void EnemyHurt(int damageNum, float distance, Transform playerRef) {
        if (isDead || isInvincible)
            return;

        isStunned = true;
        isInvincible = true;
        attackReady = false;
        enemyMovement.SetFollow(false);
        PushBack(distance, playerRef);


        health -= damageNum;
        AttackDeactivated();
        playSound.PlayEnemyHit();

        if (health <= 0)
            Death();
        else { 
            ReplayAnimation(EnemyAnimStates.ENEMY_HURT);
            stunDuration = GetAnimationLength(EnemyAnimStates.ENEMY_HURT);
            if (IsInvoking("ResetStun"))
                CancelInvoke("ResetStun");

            Invoke("ResetStun", stunDuration + 0.25f);
            Invoke("ResetInvincible", stunDuration);
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
    }
}
