using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Player Detection")]
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private Transform detectPos;
    [SerializeField]
    private float detectRange;

    [Header("Follow Properties")]
    [SerializeField]
    private float repeatFollowDelay;
    [SerializeField]
    private float attackFollowDelay;
    [SerializeField]
    private float stunFollowDelay;

    [Header("Movement Properties")]
    [SerializeField]
    private LayerMask borderLayer;
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

    private int abovePlayer;
    private bool isMoving;
    private bool canFollow;
    private bool leftOfPlayer;
    private float currentSpeed;
    private float baseMoveSpeed;
    private float xScaleValue;

    private Vector2 offsetAttackStandby;
    private Vector2 lastUpdatePos = Vector2.zero;
    private Vector2 leftOffset, rightOffset, playerChar, dist, followVelocity, direction, desiredPosition;

    // Start is called before the first frame update
    void Awake()
    {
        RandomizeOffsetAttackStandby();
        xScaleValue = transform.localScale.x;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (playerChar.x > transform.position.x) {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x) {
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
        if (canFollow)
            RandomizeOffsetAttackStandby();
    }

    public void StopFindPlayer() {
        CancelInvoke("FindPlayer");
    }

    public void FindPlayerRepeating() {
        StopFindPlayer();
        InvokeRepeating("FindPlayer", 0f, repeatFollowDelay);
    }

    public void FollowPlayer(bool attackFromLeft, bool attackReady) {
        IsMoving();
        if (canFollow)  {
            if (attackReady) { 
                if (leftOfPlayer) {
                    desiredPosition = playerChar + leftOffset;
                    followVelocity = Vector2.MoveTowards(rb.position,
                                                        desiredPosition,
                                                        baseMoveSpeed * Time.fixedDeltaTime);
                }
                else {
                    desiredPosition = playerChar + rightOffset;
                    followVelocity = Vector2.MoveTowards(rb.position,
                                                        desiredPosition,
                                                        baseMoveSpeed * Time.fixedDeltaTime);
                }
            } 
            else if (!attackReady) {
                if (leftOfPlayer) { 
                    desiredPosition = playerChar + leftOffset - offsetAttackStandby;
                    followVelocity = Vector2.MoveTowards(rb.position,
                                                        desiredPosition,
                                                        baseMoveSpeed * idleSpeedMult * Time.fixedDeltaTime);
                }
                else {
                    desiredPosition = playerChar + rightOffset + offsetAttackStandby;
                    followVelocity = Vector2.MoveTowards(rb.position,
                                                        desiredPosition,
                                                        baseMoveSpeed * idleSpeedMult * Time.fixedDeltaTime);
                }
            }

            RaycastHit2D hit = CalculateDirection();
            if (hit.collider != null) {
                //rb.MovePosition(hit.point);
                if (Vector2.Distance(rb.position, hit.point) > 0.5f)
                    rb.MovePosition(followVelocity);
            }
            else
                rb.MovePosition(followVelocity);
        }
    }

    private RaycastHit2D CalculateDirection() {
        direction = desiredPosition - rb.position;
        return Physics2D.Raycast(rb.position, direction, direction.magnitude, borderLayer);
    }

    public void IsMoving() {
        dist = (Vector2)transform.position - lastUpdatePos;
        currentSpeed = dist.magnitude / Time.deltaTime;
        lastUpdatePos = transform.position;

        if (currentSpeed > 0 && !isMoving)
            isMoving = true;
        else if (currentSpeed <= 0 && isMoving)
            isMoving = false;
    }

    public void CheckPlayerPos() {
        if (playerChar.x > transform.position.x && !leftOfPlayer) {
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer) {
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

        // case for above, below, and same y.pos as the player
        if (playerChar.y > transform.position.y && abovePlayer != 1)
            abovePlayer = 1;
        else if (playerChar.y < transform.position.y && abovePlayer != -1)
            abovePlayer = -1;
        else if (playerChar.y == transform.position.y && abovePlayer != 0)
            abovePlayer = 0;
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
    public int GetAbovePlayer() {
        return abovePlayer;
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
        if (rb != null)
            Gizmos.DrawRay(rb.position, direction);
        //Gizmos.DrawLine(rb.position, direction);
    }
}
