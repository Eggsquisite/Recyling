using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private List<Transform> instantiatePos;
    [SerializeField]
    private GameObject prefab;

    private int damage;

    public void SetDamage(int dmg) {
        damage = dmg;
    }

    private void ShootProjectile(int index) {
        var newProjectile = Instantiate(prefab, instantiatePos[index].position, Quaternion.Euler(transform.localScale));
        newProjectile.transform.localScale = transform.localScale;
        if (newProjectile.GetComponent<EnemyProjectile>() != null)
            newProjectile.GetComponent<EnemyProjectile>().SetDamage(damage);
    }
}
