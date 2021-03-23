using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPlayerScript : MonoBehaviour
{
    private GameObject player;

    public void PlayerFound(GameObject newPlayer) {
        player = newPlayer;
    }

    public PlayerStats GetPlayerStats()
    {
        if (player != null)
            return player.GetComponent<PlayerStats>();
        else
            return null;
    }

    public PlayerUI GetPlayerUI() 
    {
        if (player != null)
            return player.GetComponent<PlayerUI>();
        else
            return null;
    }
}
