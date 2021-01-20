using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float verticalSpeedMult = 0.75f;
    [SerializeField]
    private float horizontalSpeedMult = 1.25f;

    private Vector2 movement;
    private float xAxis;
    private float yAxis;

    [Header("Damaged Properties")]
    [SerializeField]
    private float hurtMaxTime;
    private float hurtTimer;
    private float flashInterval = 0.1f;
    private float flashTimer;
    private bool hurt;
    private bool invincible;
    private bool isStunned;
    private float stunDuration;

    private string currentState;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private RuntimeAnimatorController ac;

    [Header("Attack Collider Properties")]
    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private float attackLengthMultiplier;

    [SerializeField]
    private float attackWidthMultiplier;

    [Header("Attack Properties")]
    [SerializeField]
    private float damage;
    
    private float attackDelay;
    private int attackCombo;
    private bool isAttackPressed;
    private bool isSuperAttackPressed;
    private bool isAttacking;
    private bool canReceiveInput = true;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        ac = anim.runtimeAnimatorController;

        ResetAttack();
    }

    // Update is called once per frame
    void Update()
    {
        // movement inputs
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        // attack inputs
        if (Input.GetKeyDown(KeyCode.Mouse0) && canReceiveInput)
            isAttackPressed = true;
        else if (Input.GetKeyDown(KeyCode.Mouse1) && canReceiveInput)
            isSuperAttackPressed = true;

        if (hurt)
            DamageFlash();
    }

    private void FixedUpdate()
    {
        if (!isStunned)
        {
            // movement ---------------------------------
            Movement();

            // idle/run animation --------------------------------
            MovementAnimation();

            //attack ------------------------------------
            Attack();
        }
    }

    private void Movement()
    {
        movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

            if (xAxis < 0)
                transform.localScale = new Vector2(-1, 1);
            else if (xAxis > 0)
                transform.localScale = new Vector2(1, 1);
        }
    }

    private void MovementAnimation()
    {
        // attack animations override run/idle anims
        if (!isAttacking)
        {
            if (xAxis != 0 || yAxis != 0)
                AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_RUN);
            else
                AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_IDLE);
        }
    }

    private void Attack()
    {
        // case for first attack
        if (isAttackPressed && canReceiveInput && !isAttacking && attackCombo == 0)
        {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isAttackPressed = false;
            canReceiveInput = false;
        }
        // case for combo attacks
        else if (isAttackPressed && canReceiveInput && isAttacking && attackCombo < 3)
        {
            attackCombo += 1;
            AttackAnimation(attackCombo);

            isAttackPressed = false;
            canReceiveInput = false;
        }
        // case for super attack
        else if (isSuperAttackPressed)
        {
            AttackAnimation(10);

            isSuperAttackPressed = false;
            canReceiveInput = false;
        }
    }

    private void AttackAnimation(int attackIndex)
    {
        // stop movement when attacking
        movement = Vector2.zero;

        // case for first attack in combo/super attack
        if (!isAttacking)
        {
            isAttacking = true;

            GetAttackDelay(attackIndex);

            Invoke("ResetAttack", attackDelay);
        }
        // case for combos
        else if (isAttacking)
        {
            CancelInvoke("ResetAttack");

            GetAttackDelay(attackIndex);

            Invoke("ResetAttack", attackDelay);
        }
    }

    private void GetAttackDelay(int attackIndex)
    {
        if (attackIndex == 1)
        {
            attackDelay = AnimHelper.GetAnimClipLength(ac, PlayerAnimStates.PLAYER_ATTACK1);
            AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_ATTACK1);
        }
        else if (attackIndex == 2)
        {
            attackDelay = AnimHelper.GetAnimClipLength(ac, PlayerAnimStates.PLAYER_ATTACK2);
            AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_ATTACK2);
        }
        else if (attackIndex == 3)
        {
            attackDelay = AnimHelper.GetAnimClipLength(ac, PlayerAnimStates.PLAYER_ATTACK3);
            AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_ATTACK3);
        }
        else if (attackIndex == 10)
        {
            attackDelay = AnimHelper.GetAnimClipLength(ac, PlayerAnimStates.PLAYER_SUPERATTACK);
            AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_SUPERATTACK);
        }
        else
            return;
    }

    private void AttackHit(float attackMultiplier)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCapsuleAll(attackPoint.position, new Vector2(1 * attackLengthMultiplier, 0.3f * attackWidthMultiplier), CapsuleDirection2D.Horizontal, 0f);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.tag == "Enemy")
                enemy.GetComponent<BasicEnemy>().Hurt(Mathf.RoundToInt(damage * attackMultiplier));
        }
    }

    private void ResetAttack()
    {
        attackCombo = 0;
        isAttacking = false;
        canReceiveInput = true;
    }

    private void ComboInput()
    {
        // called during attack animation, allows input such as combos or dodging 
        if (isAttacking && attackCombo > 0)
            canReceiveInput = true;
        else
            return;
    }

    public void Hurt(int damageNum)
    {
        if (!hurt && !invincible)
        {
            hurt = true;
            invincible = true;

            isStunned = true;
            stunDuration = AnimHelper.GetAnimClipLength(ac, PlayerAnimStates.PLAYER_HURT);
            AnimHelper.ChangeAnimationState(anim, ref currentState, PlayerAnimStates.PLAYER_HURT);
            Invoke("ResetStun", stunDuration);

            // take damage
        }
    }

    private void ResetStun()
    {
        isStunned = false;
    }

    private void DamageFlash()
    { 
        if (hurtTimer < hurtMaxTime)
            hurtTimer += Time.deltaTime;
        else if (hurtTimer >= hurtMaxTime)
        {
            hurt = false;
            invincible = false;
            sp.enabled = true;

            hurtTimer = 0;
        }

        if (flashTimer < flashInterval)
            flashTimer += Time.deltaTime;
        else if (flashTimer >= flashInterval)
        {
            sp.enabled = !sp.enabled;
            flashTimer = 0;
        }
    }
}
