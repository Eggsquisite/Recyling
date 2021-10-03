using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private List<Transform> instantiatePos;
    [SerializeField]
    private List<float> pushbackDistance;
    [SerializeField]
    private float adjustedVelocity;

    private int damage;
    private int damage2;
    private bool pushbackUpgrade;

    public void SetDamage(float dmg) {
        damage = Mathf.RoundToInt(dmg);
    }

    public void SetSpecialDamage(float dmg) {
        damage2 = Mathf.RoundToInt(dmg);
    }

    public void SetPushbackDistance(float distance) {
        pushbackDistance[0] = distance;
    }

    /// <summary>
    /// Called through animation event
    /// </summary>
    /// <param name="index"></param>
    private void ShootProjectile(int index) {
        var newProjectile = Instantiate(prefab, instantiatePos[index].position, Quaternion.Euler(transform.localScale));
        newProjectile.transform.localScale = transform.localScale;

        // if enemy is calling this function, else use player projectile
        if (newProjectile.GetComponent<EnemyProjectile>() != null) { 
            newProjectile.GetComponent<EnemyProjectile>().SetStats(damage, pushbackDistance[index]);
        }
        else if (newProjectile.GetComponent<PlayerProjectile>() != null) { 
            // if player has upgraded special enough, add pushback, else add none
            if (pushbackUpgrade)
                newProjectile.GetComponent<PlayerProjectile>().SetStats(damage, pushbackDistance[index]);
            else
                newProjectile.GetComponent<PlayerProjectile>().SetStats(damage, 0f);
        }
    }

    /// <summary>
    /// Called through animation event
    /// </summary>
    /// <param name="index"></param>
    private void ShootSpecialProjectile(int index) {
        var newProjectile = Instantiate(prefab, instantiatePos[index].position, Quaternion.Euler(transform.localScale));
        newProjectile.transform.localScale = transform.localScale;
        if (newProjectile.GetComponent<EnemyProjectile>() != null)
            newProjectile.GetComponent<EnemyProjectile>().SetStats(damage2, pushbackDistance[index]);
        else if (newProjectile.GetComponent<PlayerProjectile>() != null) 
        { 
            newProjectile.GetComponent<PlayerProjectile>().SetStats(damage2, pushbackDistance[index]);
            if (adjustedVelocity > 0)
                newProjectile.GetComponent<PlayerProjectile>().SetVelocity(adjustedVelocity);
        }
    }
}
