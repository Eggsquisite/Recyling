using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnim;

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
        playerAnim = GetComponent<PlayerAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
            isAttackPressed1 = true;
    }

    private void FixedUpdate()
    {
        //movement
        Vector2 vel = new Vector2(0, rb.velocity.y);
        if (!isAttacking)
        {
            if (xAxis < 0)
            {
                vel.x = -moveSpeed;
                transform.localScale = new Vector2(-1, 1);
            }
            else if (xAxis > 0)
            {
                vel.x = moveSpeed;
                transform.localScale = new Vector2(1, 1);
            }
            else
            {
                vel.x = 0;
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

                playerAnim.ChangeAnimationState(AnimStates.PLAYER_ATTACK1);

                attackDelay = anim.GetCurrentAnimatorStateInfo(0).length;
                Invoke("ResetAttack", attackDelay);
            }
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }
}
