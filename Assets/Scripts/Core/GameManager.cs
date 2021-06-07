using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private SaveData activeSave;

    public Vector3 newSpawn;

    // Start is called before the first frame update
    void Start()
    {
        activeSave = SaveManager.instance.activeSave;

        if (SaveManager.instance.hasLoaded)
        {
            // player loads in at their last position
            Player.instance.transform.position = activeSave.playerCurrentPosition;

            Player.instance.LoadPlayerLevels();
            Player.instance.LoadCurrency(activeSave.playerCurrency);
            Player.instance.LoadHealth(activeSave.playerHealth);
            Player.instance.LoadEnergy(activeSave.playerEnergy);
        } else
        {
            // player starts at starting position
            Player.instance.transform.position = newSpawn;
            activeSave.playerRespawnPosition = newSpawn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
