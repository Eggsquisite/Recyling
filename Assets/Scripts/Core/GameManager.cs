using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int health;
    public int energy;
    public int currency;

    public Vector3 deathPosition;
    public Vector3 respawnPosition;
    public Vector3 currentPosition;

    // Start is called before the first frame update
    void Start()
    {
        respawnPosition = currentPosition = Player.instance.transform.position;

        if (SaveManager.instance.hasLoaded)
        {
            respawnPosition = currentPosition = SaveManager.instance.activeSave.playerRespawnPosition;
            Player.instance.transform.position = respawnPosition;

            currency = SaveManager.instance.activeSave.playerCurrency;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
