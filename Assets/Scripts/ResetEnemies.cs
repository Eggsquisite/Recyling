using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEnemies : MonoBehaviour
{
    private GameObject[] enemies;

    private void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    public void ResetAllEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<BasicEnemy>().ResetToSpawn();
        }
    }
}
