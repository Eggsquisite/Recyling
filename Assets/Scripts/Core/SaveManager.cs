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
        CheckSave();
        Load();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Save()
    {
        CheckSave();
        SavePlayerValues();
        string filePath = Path.Combine(Application.persistentDataPath, activeSave.saveName + ".xml");
        var serializer = new XmlSerializer(typeof(SaveData));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            serializer.Serialize(stream, activeSave);
            stream.Close();
        }

        Debug.Log("save: " + activeSave.saveName + " created!");
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
        { 
            activeSave.saveName = saveGame;
            Debug.Log("Changing savename: " + activeSave.saveName);
        }
    }

    public void DeleteSaveData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "filename.xml");

        if (File.Exists(filePath))
            File.Delete(filePath);
        
    }

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

    private void SavePlayerValues() {
        if (activeSave.saveName == saveGame) 
        {
            activeSave.playerHealth = Player.instance.GetHealth();
            activeSave.playerEnergy = Player.instance.GetEnergy();
            activeSave.playerCurrency = Player.instance.GetCurrency();

            activeSave.playerVitalityLevel = Player.instance.GetVitalityLevel();
            activeSave.playerFocusLevel = Player.instance.GetFocusLevel();
            activeSave.playerStrengthLevel = Player.instance.GetStrengthLevel();
            activeSave.playerStaminaLevel = Player.instance.GetStaminaLevel();
            activeSave.playerSpecialLevel = Player.instance.GetSpecialLevel();

            activeSave.playerCurrentPosition = Player.instance.transform.position;
        }
        else
        {
            activeSave.saveName = saveGame;
        }
    }
}

[System.Serializable]

public class SaveData
{
    // denotes which save the player wants to use
    public string saveName;

    // player level and stats
    //public int playerLevel;       // may not need, just add all levels together to get player level
    public int playerCurrency;
    public int playerHealth;
    public int playerEnergy;

    public int playerVitalityLevel;
    public int playerFocusLevel;
    public int playerStrengthLevel;
    public int playerStaminaLevel;
    public int playerSpecialLevel;

    // player death and respawn positions
    public Vector3 playerDeathPosition;
    public Vector3 playerRespawnPosition;
    public Vector3 playerCurrentPosition;

    // enemy save data
    public bool spawnBossOne;
    public bool spawnBossTwo;
    public bool spawnBossThree;
}
