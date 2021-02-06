using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Vector2 leftOffset, rightOffset, playerChar;

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

    // Start is called before the first frame update
    void Start()
    {
        SetupVariables();
        InvokeRepeating("FindPlayer", 1f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        ///////////////////// Follow Player /////////////////////////////////////
        //CheckPlayerInRange();
        //FollowPlayer();
    }

    private void SetupVariables() {
        canFollow = true;
        xScaleValue = transform.localScale.x;

        if (playerChar.x > transform.position.x)
        {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x)
        {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

        baseMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        leftOffset = new Vector2(Random.Range(-minOffset, -maxOffset), 0f);
        rightOffset = new Vector2(Random.Range(minOffset, maxOffset), 0f);
    }

    ////////////////// Find Player AI ////////////////////
    private void FindPlayer() {
        if (canFollow) {
            playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
        }
    }

    public void FollowPlayer(bool attackFromLeft) {
        if (attackFromLeft)
            transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, baseMoveSpeed * Time.deltaTime);
        else
            transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, baseMoveSpeed * Time.deltaTime);
    }

    public void ResetFollow()
    {
        canFollow = true;
        FindPlayer();
    }
    public void SetFollow(bool flag) {
        canFollow = flag;
    }
}
