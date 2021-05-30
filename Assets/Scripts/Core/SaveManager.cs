using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class SaveManager : MonoBehaviour
{
    public SaveData activeSave;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class SaveData
{
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
