using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData activeSave;
    public string saveGame;
    public bool hasLoaded;

    private void Awake()
    {
        instance = this;
        Load();
    }

    public void Save()
    {
        CheckSave();
        if (activeSave.enemyData == null)
            GetEnemyData();

        GetPlayerValues();
        string filePath = Path.Combine(Application.persistentDataPath, activeSave.saveName + ".xml");
        var serializer = new XmlSerializer(typeof(SaveData));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            serializer.Serialize(stream, activeSave);
            stream.Close();
        }

        Debug.Log("save: " + activeSave.saveName + " saved!");
    }

    public void Load()
    {
        CheckSave();
        string filePath = Path.Combine(Application.persistentDataPath, activeSave.saveName + ".xml");

        //if (System.IO.File.Exists(dataPath + "/" + activeSave.saveName + ".save"))
        if (File.Exists(filePath))
        {
            string fileText = File.ReadAllText(filePath);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            using (StringReader reader = new StringReader(fileText))
            {
                activeSave = (SaveData)serializer.Deserialize(reader);
            }

            Debug.Log("Load: " + activeSave.saveName + " loaded!");

            hasLoaded = true;
        }
    }

    private void CheckSave() {
        if (activeSave.saveName != saveGame)
            activeSave.saveName = saveGame;
    }

    public void DeleteSaveData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, activeSave.saveName + ".xml");

        if (File.Exists(filePath))
            File.Delete(filePath);
        
    }

    private void GetPlayerValues() {
        activeSave.playerHealth = Player.instance.GetHealth();
        activeSave.playerEnergy = Player.instance.GetEnergy();
        activeSave.playerCurrency = Player.instance.GetCurrency();

        activeSave.playerVitalityLevel = Player.instance.GetVitalityLevel();
        activeSave.playerFocusLevel = Player.instance.GetFocusLevel();
        activeSave.playerStrengthLevel = Player.instance.GetStrengthLevel();
        activeSave.playerStaminaLevel = Player.instance.GetStaminaLevel();
        activeSave.playerSpecialLevel = Player.instance.GetSpecialLevel();

        //activeSave.playerCurrentPosition = Player.instance.transform.position;
    }

    private void GetEnemyData() {
        activeSave.enemyData = EnemyManager.Instance.GetData();
    }

    public List<EnemyData> LoadEnemyData() {
        return activeSave.enemyData;
    }

    // ALL SAVE REFERENCES //////////////////////////////////////////////////////////////////////////
    public void SaveCurrency(int newValue) {
        instance.activeSave.playerCurrency = newValue;
        Save();
    }

    public void SaveHealth(int newValue) {
        instance.activeSave.playerHealth = newValue;
        Save();
    }

    public void SaveEnergy(int newValue) {
        instance.activeSave.playerEnergy = newValue;
        Save();
    }

    public void SaveAreaToLoad(int index) {
        instance.activeSave.areaToLoadIndex = index;
    }

    public void SaveSpawnPoint() {
        instance.activeSave.playerRespawnPosition = Player.instance.transform.position;
        instance.activeSave.areaToRespawnIndex = instance.activeSave.areaToLoadIndex;
        Save();
    }
}

[System.Serializable]
public class SaveData
{
    // denotes which save the player wants to use
    [Header("Save Name")]
    public string saveName;

    // denotes where the camera should be in regards to min/max values
    [Header("Camera Clamp Values")]
    // variables saved in PlayerInput.cs
    public int areaToLoadIndex;
    public int areaToRespawnIndex;
    public float minCameraPos;
    public float maxCameraPos;

    // player level and stats
    //public int playerLevel;       // may not need, just add all levels together to get player level
    [Header("Player Stats")]
    public int playerCurrency;
    public int playerLostCurrency;
    public int playerHealth;
    public int playerEnergy;

    [Header("Player Levels")]
    public int playerVitalityLevel;
    public int playerFocusLevel;
    public int playerStrengthLevel;
    public int playerStaminaLevel;
    public int playerSpecialLevel;

    // player death and respawn positions
    [Header("Player Positional Values")]
    public Vector3 playerDeathPosition;
    public Vector3 playerRespawnPosition;
    public Vector3 playerCurrentPosition;

    // enemy save data
    [Header("Enemy Data")]
    public List<EnemyData> enemyData;

    [Header("Enemy Spawn Values")]
    public bool spawnBossOne;
    public bool spawnBossTwo;
    public bool spawnBossThree;

    // need to have individual saving for each enemy in case an enemy is dead and player restarts game,
    // rezzing the enemy
}
