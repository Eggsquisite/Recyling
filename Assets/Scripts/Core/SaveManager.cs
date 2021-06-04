using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData activeSave;
    public bool hasLoaded;

    private void Awake()
    {
        instance = this;
       // Load();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Save();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            DeleteSaveData();
        }
    }

    public void Save()
    {
        string dataPath = Application.persistentDataPath;

        var serializer = new XmlSerializer(typeof(SaveData));

        // Location of where to create/save the fileStream
        var stream = new FileStream(dataPath + "/" + activeSave.saveName + ".save", FileMode.Create);

        if (stream.Position > 0)
        {
            stream.Position = 0;
        }

        // Serialize the actual object to save (SaveData using its saveName)
        serializer.Serialize(stream, activeSave);
        stream.Close();

        Debug.Log("save: " + activeSave.saveName + " created!");
    }

    public void Load()
    {
        string dataPath = Application.persistentDataPath;

        if (System.IO.File.Exists(dataPath + "/" + activeSave.saveName + ".save"))
        {
            var serializer = new XmlSerializer(typeof(SaveData));

            // Location of where to open the save
            var stream = new FileStream(dataPath + "/" + activeSave.saveName + ".save", FileMode.Create);

            activeSave = serializer.Deserialize(stream) as SaveData;
            stream.Close();

            Debug.Log("Save: " + activeSave.saveName + " loaded!");

            hasLoaded = true;
        }
    }

    public void DeleteSaveData()
    {
        string dataPath = Application.persistentDataPath;

        if (System.IO.File.Exists(dataPath + "/" + activeSave.saveName + ".save"))
        {
            File.Delete(dataPath + "/" + activeSave.saveName + ".save");
        }
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
