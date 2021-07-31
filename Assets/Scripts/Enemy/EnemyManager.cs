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

    /// <summary>
    /// FINDING SAVE DATA -> %APPDATA%/LocalLow/DefaultCompany/ForBruder
    /// </summary>


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // case for new game
        if (!SaveManager.instance.hasLoaded)
        { 
            for (int i = 0; i < enemies.Length; i++)
            {
                data = new EnemyData();
                dataList = new List<EnemyData>();
                var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

                data.id = tmpEnemy.name;
                data.isDead = tmpEnemy.GetIsDead();
                data.isBoss = tmpEnemy.GetIsBoss();
                data.facingLeft = true;
                data.startPosition = tmpEnemy.GetSpawnPoint();
                data.deathPosition = tmpEnemy.GetSpawnPoint();
                dataList.Add(data);
            }

            SaveManager.instance.SaveEnemies();
        } else
        {
            // case for loading in a game
            dataList = new List<EnemyData>();
            dataList = SaveManager.instance.LoadEnemyData();
            for (int i = 0; i < dataList.Count; i++)
            {
                // if enemy is dead and not a boss, load their dead body at the position they died
                if (dataList[i].isDead)
                {
                    //Debug.Log(dataList[i].id + " is dead! and is boss: " + dataList[i].isBoss);
                    enemies[i].GetComponent<BasicEnemy>().IsDead(dataList[i].facingLeft);
                    enemies[i].transform.position = dataList[i].deathPosition;
                }
            }
        }
    }

    public void ResetAllEnemies() {
        List<EnemyData> newList = new List<EnemyData>();
        for (int i = 0; i < enemies.Length; i++)
        {
            data = new EnemyData();
            //dataList = new List<EnemyData>();
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            if (dataList[i].isBoss && dataList[i].isDead)
            {
                data.id = tmpEnemy.name;
                data.isDead = true;
                data.isBoss = tmpEnemy.GetIsBoss();
                data.facingLeft = tmpEnemy.GetFacing();
                data.startPosition = tmpEnemy.GetSpawnPoint();
                data.deathPosition = tmpEnemy.GetSpawnPoint();

                Debug.Log(dataList[i].id + " boss is dead and not respawning!");
                newList.Add(data);
            }
            else { 
                // if enemy is NOT a boss and IS dead, reset them
                tmpEnemy.ResetToSpawn();
                data.id = tmpEnemy.name;
                data.isDead = false;
                data.isBoss = tmpEnemy.GetIsBoss();
                data.facingLeft = tmpEnemy.GetFacing();
                data.startPosition = tmpEnemy.GetSpawnPoint();
                data.deathPosition = tmpEnemy.GetSpawnPoint();

                newList.Add(data);
            }

        }

        dataList = new List<EnemyData>();
        dataList = newList;
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
            data.isDead = tmpEnemy.GetIsDead();
            data.isBoss = tmpEnemy.GetIsBoss();

            data.facingLeft = tmpEnemy.GetFacing();

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
    public bool isBoss;
    public bool facingLeft;

    public Vector3 startPosition;
    public Vector3 deathPosition;
}
