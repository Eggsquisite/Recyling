using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    private bool previousPos, nextPos;

    public void SetNextPos(bool flag) {
        nextPos = flag;
        previousPos = !flag;
    }
}
