using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private SaveData activeSave;
    private CameraFollow cam;

    public Vector3 newSpawn;

    // Start is called before the first frame update
    void Start()
    {
        activeSave = SaveManager.instance.activeSave;
        cam = Camera.main.GetComponent<CameraFollow>();

        if (SaveManager.instance.hasLoaded)
        {
            // Disable all enemies
            EnemyManager.Instance.DisableAllEnemies();

            // player loads in at their last position
            StartCoroutine(ResetCamSpeed());
            AreaManager.instance.LoadArea();
            Player.instance.transform.position = activeSave.playerCurrentPosition;

            Player.instance.LoadPlayerLevels();
            Player.instance.LoadCurrency(activeSave.playerCurrency);
            Player.instance.LoadHealth(activeSave.playerHealth);
            Player.instance.LoadEnergy(activeSave.playerEnergy);
            //cam.SetMinX(activeSave.minCameraPos);
            //cam.SetMaxX(activeSave.maxCameraPos);

        } else
        {
            // player starts at starting position
            Player.instance.transform.position = newSpawn;
            activeSave.playerRespawnPosition = newSpawn;
            cam.SetMinX(0);
            cam.SetMaxX(0);
        }
    }

    IEnumerator ResetCamSpeed() {
        cam.SetCamSpeed(100f);
        yield return new WaitForSeconds(1f);
        cam.SetCamSpeed(1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
