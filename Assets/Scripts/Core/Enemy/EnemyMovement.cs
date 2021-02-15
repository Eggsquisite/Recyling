using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField]
    private Transform detectPos;
    [SerializeField]
    private float detectRange;
    [SerializeField]
    private LayerMask playerLayer;

    [Header("Follow Properties")]
    [SerializeField]
    private float repeatFollowDelay;
    [SerializeField]
    private float attackFollowDelay;
    [SerializeField]
    private float stunFollowDelay;

    [Header("Movement Properties")]
    [SerializeField] 
    private float minMoveSpeed;
    [SerializeField] 
    private float maxMoveSpeed;
    [SerializeField]
    private float idleSpeedMult;

    [SerializeField] 
    private float minOffset;
    [SerializeField] 
    private float maxOffset;
    [SerializeField]
    private Vector2 offsetAttackStandbyRange;

    private bool isMoving;
    private bool canFollow;
    private bool leftOfPlayer;
    private float currentSpeed;
    private float baseMoveSpeed;
    private float xScaleValue;

    private Vector2 offsetAttackStandby;
    private Vector2 lastUpdatePos = Vector2.zero;
    private Vector2 leftOffset, rightOffset, playerChar, dist;

    // Start is called before the first frame update
    void Awake()
    {
        SetupVariables();
        //InvokeRepeating("FindPlayer", 1f, repeatFollowDelay);
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
        RandomizeOffsetAttackStandby();

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

    private void RandomizeOffsetAttackStandby() {
        offsetAttackStandby = new Vector2(offsetAttackStandbyRange.x,
                                    Random.Range(-offsetAttackStandbyRange.y, offsetAttackStandbyRange.y));
    }

    ////////////////// Find Player AI ////////////////////
    public void FindPlayer() {
        // Called thru invoke
        playerChar = GameObject.FindGameObjectWithTag("Player").transform.position;
        RandomizeOffsetAttackStandby();
    }

    public void StopFindPlayer() {
        CancelInvoke("FindPlayer");
    }

    public void FindPlayerRepeating() {
        InvokeRepeating("FindPlayer", 0f, repeatFollowDelay);
    }

    public void FollowPlayer(bool attackFromLeft, bool attackReady) {
        IsMoving();
        if (canFollow)  {

            if (attackReady) { 
                if (leftOfPlayer)
                    transform.position = Vector2.MoveTowards(transform.position, 
                        playerChar + leftOffset, 
                        baseMoveSpeed * Time.deltaTime);
                else
                    transform.position = Vector2.MoveTowards(transform.position, 
                        playerChar + rightOffset, 
                        baseMoveSpeed * Time.deltaTime);
            } else if (!attackReady) { 
                if (leftOfPlayer)
                    transform.position = Vector2.MoveTowards(transform.position, 
                        playerChar + leftOffset - offsetAttackStandby, 
                        baseMoveSpeed * idleSpeedMult * Time.deltaTime);
                else
                    transform.position = Vector2.MoveTowards(transform.position, 
                        playerChar + rightOffset + offsetAttackStandby, 
                        baseMoveSpeed * idleSpeedMult * Time.deltaTime);
            }
        }
    }

    private void IsMoving() {
        dist = (Vector2)transform.position - lastUpdatePos;
        currentSpeed = dist.magnitude / Time.deltaTime;
        lastUpdatePos = transform.position;

        if (currentSpeed > 0 && !isMoving)
            isMoving = true;
        else if (currentSpeed <= 0 && isMoving)
            isMoving = false;
    }

    public void CheckPlayerPos() {
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

    public void CheckPlayerInRange(ref RaycastHit2D playerDetected) {
        if (leftOfPlayer)
            playerDetected = Physics2D.Raycast(detectPos.position, Vector2.right, detectRange, playerLayer);
        else
            playerDetected = Physics2D.Raycast(detectPos.position, Vector2.left, detectRange, playerLayer);
    }

    // FOLLOW PROPERTIES /////////////////////////////////////////////////////////////////////////////////////

    public IEnumerator ResetAttackFollow()
    {
        yield return new WaitForSeconds(attackFollowDelay);
        canFollow = true;
        FindPlayerRepeating();
    }
    public IEnumerator ResetStunFollow()
    {
        yield return new WaitForSeconds(stunFollowDelay);
        canFollow = true;
        FindPlayerRepeating();
    }
    public void SetFollow(bool flag) {
        canFollow = flag;
        if (!flag)
            CancelInvoke("FindPlayer");
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
    public float GetPlayerDistance() {
        return Vector2.Distance(playerChar, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        //Gizmos.DrawLine(detectPos.position, (Vector2)detectPos.position + (Vector2.right * detectRange));
    }
}
