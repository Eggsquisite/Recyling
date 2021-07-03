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
    private float detectInRange;
    [SerializeField]
    private bool detectIsRay;

    private bool facingLeft;
    // GameObject player;
    private Vector2 playerChar;

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

    private float idleSpeedMult = 1f;

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
    private bool attackFromLeft;

    private float xScaleValue;
    private float currentSpeed;
    private float baseMoveSpeed;

    private Vector2 offsetAttackStandby;
    private Vector2 lastUpdatePos = Vector2.zero;
    private Vector2 leftOffset, rightOffset, dist, followVelocity;
    private Vector2 directionToMove, directionToPlayer, desiredPosition;

    [Header("Patrol Properties")]
    [SerializeField]
    private bool canPatrol;
    [SerializeField]
    private Vector2 offsetPatrolDestination;
    [SerializeField]
    private float waitTime;

    private bool patrolReady;

    private Vector2 originalPos;
    private Vector2 currentPatrolDestination;

    private Coroutine patrolWaitRoutine;

    [Header("Teleport Properties")]
    [SerializeField]
    private bool canTeleport;
    [SerializeField]
    private float minTeleportDuration;
    [SerializeField]
    private float maxTeleportDuration;
    [SerializeField]
    private float minTeleportCooldown;
    [SerializeField]
    private float maxTeleportCooldown;

    private bool teleportReady;
    private bool isTeleporting;

    private float teleportDuration;
    private float teleportCooldown;

    private Coroutine teleportRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        RandomizeOffsetAttackStandby();
        originalPos = transform.position;
        xScaleValue = transform.localScale.x;
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (playerChar.x > transform.position.x) {
            facingLeft = false;
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x) {
            facingLeft = true;
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

        teleportReady = true;
        teleportCooldown = Random.Range(minTeleportCooldown, maxTeleportCooldown);
        teleportDuration = Random.Range(minTeleportDuration, maxTeleportDuration);
        baseMoveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        leftOffset = new Vector2(Random.Range(-minOffset, -maxOffset), 0f);
        rightOffset = new Vector2(Random.Range(minOffset, maxOffset), 0f);
    }

    private void FixedUpdate()
    {
        if (canPatrol && patrolReady) 
            Patrolling(); 
    }

    private void RandomizeOffsetAttackStandby() {
        offsetAttackStandby = new Vector2(offsetAttackStandbyRange.x,
                                    Random.Range(-offsetAttackStandbyRange.y, offsetAttackStandbyRange.y));
    }

    /// <summary>
    /// FIND PLAYER AI /////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public void FindPlayer() {
        // Called thru invoke
        //player = GameObject.FindGameObjectWithTag("Player");
        //playerChar = player.transform.position;
        playerChar = Player.instance.transform.position;
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

    public void FollowPlayer(bool attackReady) {
        if (isTeleporting)
            return;

        IsMoving();
        if (canFollow)  {
            if (attackReady) { 
                if (attackFromLeft) {
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
                if (attackFromLeft) { 
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

            RaycastHit2D hit = CalculateDirectionToMove(desiredPosition - rb.position);
            if (hit.collider != null && hit.collider.tag == "LeftBorder") {
                if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                    rb.MovePosition(followVelocity);
                attackFromLeft = true;
            }
            else if (hit.collider != null && hit.collider.tag == "RightBorder") {
                if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                    rb.MovePosition(followVelocity);
                attackFromLeft = false;
            }
            if (hit.collider != null) {
                //rb.MovePosition(hit.point);
                if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                    rb.MovePosition(followVelocity);
            }
            else
                rb.MovePosition(followVelocity);
        } 
    }

    public RaycastHit2D CalculateDirectionToMove(Vector2 direction) {
        //directionToMove = desiredPosition - rb.position;
        return Physics2D.Raycast(rb.position, direction, direction.magnitude, borderLayer);
    }

    public RaycastHit2D DetectPlayer() {
        if (detectIsRay) { 
            // useful for ranged characters or enemies that don't move vertically during attack
            // CalculateRaycastToPlayer
            if (leftOfPlayer)
                return Physics2D.Raycast(detectPos.position, Vector2.right, detectInRange, playerLayer);
            else
                return Physics2D.Raycast(detectPos.position, Vector2.left, detectInRange, playerLayer);
        }
        else {
            // CalculateDirectionToPlayer
            directionToPlayer = playerChar - rb.position;
            return Physics2D.Raycast(rb.position, directionToPlayer, detectInRange, playerLayer);
        }            
    }

    public void IsMoving() {
        dist = (Vector2)transform.position - lastUpdatePos;
        currentSpeed = dist.magnitude / Time.deltaTime;
        lastUpdatePos = transform.position;

        if (!canFollow)
            isMoving = false;
        else if (currentSpeed > 0 && !isMoving)
            isMoving = true;
        else if (currentSpeed <= 0 && isMoving)
            isMoving = false;
    }

    public bool CheckPlayerPos() {
        // calculate leftOfPlayer and set scale to 1/-1 
        if (playerChar.x > transform.position.x && !leftOfPlayer) {
            facingLeft = false;
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x && leftOfPlayer) {
            facingLeft = true;
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }

        CheckPlayerVertical();
        return facingLeft;
    }

    public void SetDirection(bool left) {
        if (left)
        {
            facingLeft = true;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        } else
        {
            facingLeft = false;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
    }

    public void ResetDirection() {
        if (playerChar.x > transform.position.x)
        {
            facingLeft = false;
            leftOfPlayer = true;
            transform.localScale = new Vector2(xScaleValue, transform.localScale.y);
        }
        else if (playerChar.x <= transform.position.x)
        {
            facingLeft = true;
            leftOfPlayer = false;
            transform.localScale = new Vector2(-xScaleValue, transform.localScale.y);
        }
    }

    public void CheckPlayerVertical() {
        // case for above, below, and same y.pos as the player
        if (playerChar.y > transform.position.y + 0.05f && abovePlayer != 1)
            abovePlayer = 1;
        else if (playerChar.y < transform.position.y - 0.05f && abovePlayer != -1)
            abovePlayer = -1;
        else if (playerChar.y >= transform.position.y - 0.05f
                    && playerChar.y <= transform.position.y + 0.05f
                    && abovePlayer != 0)
            abovePlayer = 0;
    }

    /// <summary>
    /// PATROL CODE //////////////////////////////////////////////////////////////////////////////
    /// </summary>
    
    public void BeginPatrol() {
        if (!canPatrol)
            return;

        RandomizeOffsetPatrol();
        if (patrolWaitRoutine != null)
            StopCoroutine(patrolWaitRoutine);
        patrolWaitRoutine = StartCoroutine(PatrolWait());
    }

    public void StopPatrol() {
        if (!canPatrol)
            return;

        patrolReady = false;
    }

    IEnumerator PatrolWait() {
        yield return new WaitForSeconds(waitTime);
        canFollow = false;
        patrolReady = true;
    }

    private void Patrolling() {
        IsMoving();

        followVelocity = Vector2.MoveTowards(rb.position,
                                                currentPatrolDestination,
                                                baseMoveSpeed * idleSpeedMult * Time.fixedDeltaTime);

        RaycastHit2D hit = CalculateDirectionToMove(desiredPosition - rb.position);
        if (hit.collider != null && hit.collider.tag == "LeftBorder")
        {
            if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                rb.MovePosition(followVelocity);
            attackFromLeft = true;
        }
        else if (hit.collider != null && hit.collider.tag == "RightBorder")
        {
            if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                rb.MovePosition(followVelocity);
            attackFromLeft = false;
        }
        if (hit.collider != null)
        {
            //rb.MovePosition(hit.point);
            if (Vector2.Distance(rb.position, hit.point) > 0.25f)
                rb.MovePosition(followVelocity);
        }
        else
            rb.MovePosition(followVelocity);
    }
    
    private void RandomizeOffsetPatrol() {
        currentPatrolDestination = new Vector2(
            originalPos.x + Random.Range(-offsetPatrolDestination.x, offsetPatrolDestination.x),
            originalPos.y + Random.Range(-offsetPatrolDestination.y, offsetPatrolDestination.y));
    }   

    /// <summary>
    /// TELEPORT CODE ////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public void TeleportToPlayer() {
        teleportReady = false;
        if (attackFromLeft)
            rb.position = playerChar + leftOffset;
        else
            rb.position = playerChar + rightOffset;

        if (teleportRoutine != null)
            StopCoroutine(teleportRoutine);
        teleportRoutine = StartCoroutine(TeleportCooldown());
    }

    IEnumerator TeleportCooldown() {
        yield return new WaitForSeconds(teleportCooldown);
        teleportCooldown = Random.Range(minTeleportCooldown, maxTeleportCooldown);
        teleportDuration = Random.Range(minTeleportDuration, maxTeleportDuration);
        teleportReady = true;
    }

    private void RandomizeAttackFromLeft() {
        int tmp = Random.Range(0, 10);
        if (attackFromLeft) {
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = true;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = false;
        }
        else if (!attackFromLeft) {
            if (tmp >= 0 && tmp < 8)
                attackFromLeft = false;
            else if (tmp >= 8 && tmp < 10)
                attackFromLeft = true;
        }
    }

    // FOLLOW PROPERTIES /////////////////////////////////////////////////////////////////////////////////////

    public IEnumerator ResetAttackFollow()
    {
        RandomizeAttackFromLeft();
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
    public GameObject GetPlayer() {
        /*if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        return player;*/
        return null;
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
    public bool GetCanTeleport() {
        return canTeleport;
    }
    public bool GetIsTeleporting() {
        return isTeleporting;
    }
    public void SetIsTeleporting(bool flag) {
        isTeleporting = flag;
    }
    public bool GetTeleportReady() {
        return teleportReady;
    }
    public void SetTeleportReady(bool flag) { 
        teleportReady = flag;
    }
    public float GetTeleportDuration() {
        return teleportDuration;
    }

    public bool GetFacingDirection() {
        return facingLeft;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        //Gizmos.DrawLine(detectPos.position, (Vector2)detectPos.position + (Vector2.right * detectRange));
        if (rb != null) { 
            Gizmos.DrawRay(rb.position, directionToMove);
            Gizmos.color = Color.grey;
            Gizmos.DrawLine(rb.position, directionToPlayer);
        }
    }
}
