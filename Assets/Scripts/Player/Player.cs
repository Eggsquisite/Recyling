using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;
    [SerializeField]
    private float verticalSpeedMult = 0.75f;
    [SerializeField]
    private float horizontalSpeedMult = 1.25f;

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnim;
    private SpriteRenderer sp;

    private Vector2 movement;
    private float xAxis;
    private float yAxis;
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
        playerAnim = GetComponent<PlayerAnimation>();

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
    }

    private void FixedUpdate()
    {
        // movement ---------------------------------
        Movement();

        // idle/run animation --------------------------------
        MovementAnimation();

        //attack ------------------------------------
        Attack();
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
                playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_RUN);
            else
                playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_IDLE);
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
            attackDelay = playerAnim.GetAnimationClipLength(PlayerAnimStates.PLAYER_ATTACK1);
            playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_ATTACK1);
        }
        else if (attackIndex == 2)
        {
            attackDelay = playerAnim.GetAnimationClipLength(PlayerAnimStates.PLAYER_ATTACK2);
            playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_ATTACK2);
        }
        else if (attackIndex == 3)
        {
            attackDelay = playerAnim.GetAnimationClipLength(PlayerAnimStates.PLAYER_ATTACK3);
            playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_ATTACK3);
        }
        else if (attackIndex == 10)
        {
            attackDelay = playerAnim.GetAnimationClipLength(PlayerAnimStates.PLAYER_SUPERATTACK);
            playerAnim.ChangeAnimationState(PlayerAnimStates.PLAYER_SUPERATTACK);
        }
        else
            return;
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
}
