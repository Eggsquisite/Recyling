using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance;
    private static GameObject[] enemies;

    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EnemyManager>();
            }

            return instance;
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

    public void ResetAllEnemies() {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<BasicEnemy>().ResetToSpawn();
        }
    }

    public void DisableAllEnemies() { 
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<BasicEnemy>().SetIsInactive(true);
        }
    }

    public List<EnemyData> GetData() {
        List<EnemyData> dataList = new List<EnemyData>();
        EnemyData data = new EnemyData();

        for (int i = 0; i < enemies.Length; i++)
        {
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            data.id = tmpEnemy.name;
            data.isDead = tmpEnemy.GetIsDead();

            data.startPosition = tmpEnemy.GetSpawnPoint();

            if (data.isDead)
                data.deathPosition = tmpEnemy.transform.position;
            else
                data.deathPosition = tmpEnemy.GetSpawnPoint();

            dataList.Add(data);
            //Debug.Log(dataList[i].id + " is added!");
        }

        return dataList;
    }
}

[System.Serializable]
public class EnemyData
{
    public string id;
    public bool isDead;

    public Vector3 startPosition;
    public Vector3 deathPosition;
}
