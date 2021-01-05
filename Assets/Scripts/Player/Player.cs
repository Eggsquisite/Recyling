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

    private float xAxis;
    private float yAxis;
    private float attackDelay;
    private bool isAttackPressed1, isAttackPressed2, isAttackPressed3, isSuperAttackPressed;
    private bool isAttacking;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        playerAnim = GetComponent<PlayerAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
            isAttackPressed1 = true;
    }

    private void FixedUpdate()
    {
        //movement
        Vector2 movement = new Vector2(xAxis * horizontalSpeedMult, yAxis * verticalSpeedMult);
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
        if (isAttackPressed1)
        {
            isAttackPressed1 = false;

            if (!isAttacking)
            {
                isAttacking = true;
                movement = Vector2.zero;

                playerAnim.ChangeAnimationState(AnimStates.PLAYER_ATTACK1);

                attackDelay = anim.GetCurrentAnimatorStateInfo(0).length;
                Debug.Log(attackDelay);
                Invoke("ResetAttack", attackDelay);
            }
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }
}
