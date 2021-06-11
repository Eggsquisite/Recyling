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
                var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

                data.id = tmpEnemy.name;
                data.isDead = tmpEnemy.GetIsDead();
                data.startPosition = tmpEnemy.GetSpawnPoint();
                data.deathPosition = tmpEnemy.GetSpawnPoint();
                dataList.Add(data);
            }

            SaveManager.instance.Save();
        } else
        {
            dataList = new List<EnemyData>();
            dataList = SaveManager.instance.LoadEnemyData();
            for (int i = 0; i < enemies.Length; i++)
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

    public void SetIsDead(string id, Vector2 deathPosition, bool facing) { 
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name == id) { 
                dataList[i].isDead = true;
                dataList[i].deathPosition = deathPosition;
                dataList[i].facingLeft = facing;
                Debug.Log(name + " is facing: " + facing);
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

    public bool GetFacing(string id)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].name == id)
            {
                return dataList[i].facingLeft;
            }
        }
        return false;
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

    public List<EnemyData> GetData() {
        List<EnemyData> newList = new List<EnemyData>();
        for (int i = 0; i < enemies.Length; i++)
        {
            data = new EnemyData();
            var tmpEnemy = enemies[i].GetComponent<BasicEnemy>();

            data.id = tmpEnemy.name;
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
