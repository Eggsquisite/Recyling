using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField]
    private float flyTime;
    [SerializeField]
    private float projectileVelocity;

    private int damage;
    private float pushDistance;

    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D coll;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
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
        if (collision.tag == "Player" && !collision.GetComponent<Player>().GetInvincible()) {
            collision.GetComponent<Player>().PlayerHurt(damage, pushDistance);
            anim.Play("arrow_dissipate");
            coll.enabled = false;
        }
    }

    IEnumerator Kill() {
        yield return new WaitForSeconds(flyTime);
        anim.Play("arrow_dissipate");
    }

    private void Dissipate() {
        Destroy(gameObject);
    }

    public void SetStats(int dmg, float distance) {
        damage = dmg;
        pushDistance = distance;
    }
}
