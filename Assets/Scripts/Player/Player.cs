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
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
            isAttackPressed = true;
        else if (Input.GetKeyDown(KeyCode.Mouse1) && !isAttacking)
            isSuperAttackPressed = true;
    }

    private void FixedUpdate()
    {
        //movement
        movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
        if (!isAttacking)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

            if (xAxis < 0)
            {
                transform.localScale = new Vector2(-1, 1);
            }
            else if (xAxis > 0)
            {
                transform.localScale = new Vector2(1, 1);
            }
        }

        //animation
        if (!isAttacking)
        {
            if (xAxis != 0 || yAxis != 0)
                playerAnim.ChangeAnimationState(AnimStates.PLAYER_RUN);
            else
                playerAnim.ChangeAnimationState(AnimStates.PLAYER_IDLE);
        }

        //attack
        if (isAttackPressed)
        {
            isAttackPressed = false;
            attackCombo += 1;
            Attack(attackCombo);
        }
        else if (isSuperAttackPressed)
        {
            isSuperAttackPressed = false;
            Attack(4);
        }

    }

    private void Attack(int attackIndex)
    {
        if (!isAttacking)
        {
            isAttacking = true;
            movement = Vector2.zero;

            GetAttackAnimation(attackIndex);
            
            Debug.Log(attackDelay);
            Invoke("ResetAttack", attackDelay);
        }
    }

    private void GetAttackAnimation(int attackIndex)
    { 
        if (attackIndex == 1)
        {
            attackDelay = playerAnim.GetAnimationClipLength(AnimStates.PLAYER_ATTACK1);
            playerAnim.ChangeAnimationState(AnimStates.PLAYER_ATTACK1);
        }
        else if (attackIndex == 2)
        {
            attackDelay = playerAnim.GetAnimationClipLength(AnimStates.PLAYER_ATTACK2);
            playerAnim.ChangeAnimationState(AnimStates.PLAYER_ATTACK2);
        }
        else if (attackIndex == 3)
        {
            attackDelay = playerAnim.GetAnimationClipLength(AnimStates.PLAYER_ATTACK3);
            playerAnim.ChangeAnimationState(AnimStates.PLAYER_ATTACK3);
        }
        else if (attackIndex == 4)
        {
            attackDelay = playerAnim.GetAnimationClipLength(AnimStates.PLAYER_SUPERATTACK);
            playerAnim.ChangeAnimationState(AnimStates.PLAYER_SUPERATTACK);
        }
    }

    private void ResetAttack()
    {
        attackCombo = 0;
        isAttacking = false;
    }
}
