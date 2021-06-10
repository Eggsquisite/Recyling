using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance;
    private static GameObject[] enemies;

    public EnemyData data;
    public List<EnemyData> dataList;

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
        for (int i = 0; i < enemies.Length; i++)
        {
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            data.id = tmpEnemy.name;
            data.isDead = tmpEnemy.GetIsDead();

            if (data.isDead)
                tmpEnemy.IsDead();

            data.startPosition = tmpEnemy.GetSpawnPoint();

            if (data.isDead)
                data.deathPosition = tmpEnemy.transform.position;
            else
                data.deathPosition = tmpEnemy.GetSpawnPoint();

            Debug.Log(data.id + " is added!");
            dataList.Add(data);
        }
        
        SaveManager.instance.Save();
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

    public void SetIsDead(string id, Vector2 deathPosition) { 
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name == id) { 
                dataList[i].isDead = true;
                dataList[i].deathPosition = deathPosition;
            }
        }

        SaveManager.instance.Save();
    }

    public bool GetIsDead(string id) {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name == id) {
                if (dataList[i].isDead)
                    enemies[i].transform.position = dataList[i].deathPosition;

                return dataList[i].isDead;
            }
        }

        return false;
    }

    public List<EnemyData> GetData() {
        //dataList = new List<EnemyData>();
        Debug.Log("Refreshing and retrieving enemy data...");

/*        for (int i = 0; i < enemies.Length; i++)
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
        }*/

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
