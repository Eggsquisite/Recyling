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

        if (!SaveManager.instance.hasLoaded)
        { 
            for (int i = 0; i < enemies.Length; i++)
            {
                data = new EnemyData();
                dataList = new List<EnemyData>();
                var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

                data.id = tmpEnemy.name;
                data.isDead = tmpEnemy.GetIsDead();
                data.facingLeft = true;
                data.startPosition = tmpEnemy.GetSpawnPoint();
                data.deathPosition = tmpEnemy.GetSpawnPoint();
                dataList.Add(data);
            }

            SaveManager.instance.SaveEnemies();
        } else
        {
            dataList = new List<EnemyData>();
            dataList = SaveManager.instance.LoadEnemyData();
            for (int i = 0; i < dataList.Count; i++)
            {
                if (dataList[i].isDead)
                {
                    enemies[i].GetComponent<BasicEnemy>().IsDead(dataList[i].facingLeft);
                    enemies[i].transform.position = dataList[i].deathPosition;
                }
            }
        }
    }

    public void ResetAllEnemies() {
        for (int i = 0; i < enemies.Length; i++)
        {
            data = new EnemyData();
            dataList = new List<EnemyData>();
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            tmpEnemy.ResetToSpawn();
            data.id = tmpEnemy.name;
            data.isDead = false;
            data.facingLeft = tmpEnemy.GetFacing();
            data.startPosition = tmpEnemy.GetSpawnPoint();
            data.deathPosition = tmpEnemy.GetSpawnPoint();
            dataList.Add(data);
        }

        SaveManager.instance.SaveEnemies();
    }

    public void DisableAllEnemies() { 
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<BasicEnemy>().SetIsInactive(true);
        }
    }

    public Vector2 GetDeathPosition(Transform obj, string id) {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name == id)
            {
                return dataList[i].deathPosition;
            }
        }

        return obj.transform.position;
    }
    
    public void EnemyDead(string id) {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (id == dataList[i].id)
            {
                dataList[i].isDead = true;
                Debug.Log(dataList[i].isDead);
            }
        }

        SaveManager.instance.SaveEnemies();
    }

    public List<EnemyData> UpdateEnemyData() {
        List<EnemyData> newList = new List<EnemyData>();
        for (int i = 0; i < enemies.Length; i++)
        {
            data = new EnemyData();
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            data.id = tmpEnemy.name;
            data.facingLeft = tmpEnemy.GetFacing();
            data.isDead = tmpEnemy.GetIsDead();

            data.startPosition = tmpEnemy.GetSpawnPoint();

            if (data.isDead)
                data.deathPosition = tmpEnemy.transform.position;
            else
                data.deathPosition = tmpEnemy.GetSpawnPoint();

            newList.Add(data);
        }
        
        return newList;
    }

    private void OnApplicationQuit()
    {
        SaveManager.instance.SaveEnemies();
    }
}

[System.Serializable]
public class EnemyData
{
    public string id;
    public bool isDead;
    public bool facingLeft;

    public Vector3 startPosition;
    public Vector3 deathPosition;
}
