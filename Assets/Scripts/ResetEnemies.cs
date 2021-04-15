using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEnemies : MonoBehaviour
{
    private static ResetEnemies _instance;
    private static GameObject[] enemies;

    public static ResetEnemies Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ResetEnemies>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


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
