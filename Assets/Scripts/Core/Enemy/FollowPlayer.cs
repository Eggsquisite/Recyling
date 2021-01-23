using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Vector2 rightOffset;

    [SerializeField]
    private Vector2 leftOffset;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sp;
    private Vector2 playerChar;

    public bool canFollow;
    private float xScaleVal;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
        xScaleVal = transform.localScale.x;

        InvokeRepeating("FindPlayer", 1f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canFollow)
        {
            // player is to the right of enemy
            if (playerChar.x > transform.position.x)
            {
                transform.localScale = new Vector2(xScaleVal, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, moveSpeed * Time.deltaTime);
            } 
            // player is to the left of enemy
            else if (playerChar.x <= transform.position.x)
            {
                transform.localScale = new Vector2(-xScaleVal, transform.localScale.y);
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, moveSpeed * Time.deltaTime);
            }


        }
    }

    private void FindPlayer()
    {
        if (canFollow)
            playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
    }


}
