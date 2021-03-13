using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField]
    private float flyTime;
    [SerializeField]
    private float projectileVelocity;

    private int damage;
    private float pushDistance;

    private Rigidbody2D rb;
    private Collider2D coll;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
        StartCoroutine(Kill());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.localScale.x > 0) {
            rb.MovePosition((Vector2)transform.position + Vector2.right * projectileVelocity * Time.fixedDeltaTime);
        }
        else { 
            rb.MovePosition((Vector2)transform.position + Vector2.left * projectileVelocity * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && !collision.GetComponent<BasicEnemy>().GetIsInvincible()) {
            collision.GetComponent<BasicEnemy>().EnemyHurt(damage, pushDistance, 1);
            coll.enabled = false;
            Destroy(gameObject);
        }
    }

    IEnumerator Kill() {
        yield return new WaitForSeconds(flyTime);
        Destroy(gameObject);
    }

    public void SetStats(int dmg, float distance) {
        damage = dmg;
        pushDistance = distance;
    }

    public void SetVelocity(float speed) {
        projectileVelocity = speed;
    }
}
