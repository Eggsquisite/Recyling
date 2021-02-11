using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
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
    private bool leftOfPlayer;
    private float currentSpeed;
    private float baseMoveSpeed;
    private float xScaleValue;

    private Vector2 lastUpdatePos = Vector2.zero;
    private Vector2 leftOffset, rightOffset, playerChar, dist;

    // Start is called before the first frame update
    void Start()
    {
        SetupVariables();
        InvokeRepeating("FindPlayer", 1f, followDelay);
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
    public void FindPlayer() {
        playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    public void StopFindPlayer() {
        CancelInvoke("FindPlayer");
    }
    public void FindPlayerRepeating() {
        InvokeRepeating("FindPlayer", 0f, followDelay);
    }

    public void FollowPlayer(bool attackFromLeft) {
        if (canFollow)  {
            IsMoving();

            if (attackFromLeft)
                transform.position = Vector2.MoveTowards(transform.position, playerChar + leftOffset, baseMoveSpeed * Time.deltaTime);
            else
                transform.position = Vector2.MoveTowards(transform.position, playerChar + rightOffset, baseMoveSpeed * Time.deltaTime);
        }
    }

    private void IsMoving()
    {
        dist = (Vector2)transform.position - lastUpdatePos;
        currentSpeed = dist.magnitude / Time.deltaTime;
        lastUpdatePos = transform.position;

        if (currentSpeed > 0 && !isMoving)
            isMoving = true;
        else if (currentSpeed <= 0 && isMoving)
            isMoving = false;
    }

    public void CheckPlayerPos()
    {
        if (playerChar.x > transform.position.x && !leftOfPlayer)
        {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer)
        {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }
    }


    public void ResetFollow()
    {
        canFollow = true;
        FindPlayer();
    }
    public void SetFollow(bool flag) {
        canFollow = flag;
    }
    public float GetFollowDelay() {
        return followDelay;
    }
    public bool GetLeftOfPlayer() {
        return leftOfPlayer;
    }
    public bool GetIsMoving() {
        return isMoving;
    }
    public Vector2 GetPlayerPosition() {
        return playerChar;
    }
}
