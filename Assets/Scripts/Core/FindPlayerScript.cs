using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPlayerScript : MonoBehaviour
{
    private GameObject player;

    public void PlayerFound(GameObject newPlayer) {
        player = newPlayer;
    }

    public GameObject GetPlayer()
    {
        return player;
    }
}
