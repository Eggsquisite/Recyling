using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private List<Transform> instantiatePos;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private float adjustedVelocity;

    private int damage;
    private int damage2;

    public void SetDamage(int dmg) {
        damage = dmg;
    }

    public void SetSpecialDamage(int dmg) {
        damage2 = dmg;
    }

    private void ShootProjectile(int index) {
        var newProjectile = Instantiate(prefab, instantiatePos[index].position, Quaternion.Euler(transform.localScale));
        newProjectile.transform.localScale = transform.localScale;
        if (newProjectile.GetComponent<EnemyProjectile>() != null)
            newProjectile.GetComponent<EnemyProjectile>().SetDamage(damage);
        else if (newProjectile.GetComponent<PlayerProjectile>() != null)
            newProjectile.GetComponent<PlayerProjectile>().SetDamage(damage);
    }

    private void ShootSpecialProjectile(int index) {
        var newProjectile = Instantiate(prefab, instantiatePos[index].position, Quaternion.Euler(transform.localScale));
        newProjectile.transform.localScale = transform.localScale;
        if (newProjectile.GetComponent<EnemyProjectile>() != null)
            newProjectile.GetComponent<EnemyProjectile>().SetDamage(damage2);
        else if (newProjectile.GetComponent<PlayerProjectile>() != null) 
        { 
            newProjectile.GetComponent<PlayerProjectile>().SetDamage(damage2);
            if (adjustedVelocity > 0)
                newProjectile.GetComponent<PlayerProjectile>().SetVelocity(adjustedVelocity);
        }
    }
}
