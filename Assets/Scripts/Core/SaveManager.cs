using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public SaveData activeSave;
    public bool hasLoaded;

    private void Awake()
    {
        instance = this;
        Load();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save()
    {
        string dataPath = Application.persistentDataPath;

        var serializer = new XmlSerializer(typeof(SaveData));

        // Location of where to create/save the fileStream
        var stream = new FileStream(dataPath + "/" + activeSave.saveName + ".save", FileMode.Create);

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
    public int playerVitalityLevel;
    public int playerFocusLevel;
    public int playerStrengthLevel;
    public int playerStaminaLevel;
    public int playerSpecialLevel;

    // player death and respawn positions
    public Vector3 playerDeathPosition;
    public Vector3 playerRespawnPosition;

    // enemy save data
    public bool spawnBossOne;
    public bool spawnBossTwo;
    public bool spawnBossThree;
}
