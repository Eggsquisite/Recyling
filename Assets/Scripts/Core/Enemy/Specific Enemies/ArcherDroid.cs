using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherDroid : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    private int damage;

    public void SetDamage(int dmg) {
        damage = dmg;
    }

    private void ShootArrow() {
        var newArrow = Instantiate(prefab, new Vector2(transform.position.x, transform.position.y), Quaternion.Euler(transform.localScale));
        newArrow.GetComponent<EnemyProjectile>().SetDamage(damage);
        newArrow.transform.localScale = transform.localScale;
    }
}
