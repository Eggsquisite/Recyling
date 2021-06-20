using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SaveData activeSave;
    private CameraFollow cam;

    public Vector3 newSpawn;

    private void Awake()
    {
        instance = this;
    }

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
            AreaManager.instance.LoadArea(activeSave.areaToLoadIndex);
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
            cam.SetMinX(0);
            cam.SetMaxX(0);
        }
    }

    IEnumerator ResetCamSpeed() {
        cam.SetCamSpeed(100f);
        yield return new WaitForSeconds(1f);
        cam.SetCamSpeed(1.5f);
    }

    public void BeginRespawn(int currency)
    {
        activeSave.playerLostCurrency = currency;
        StartCoroutine(RespawnPlayer());
    }

    IEnumerator RespawnPlayer() {
        // wait 2 seconds before transitioning back to respawn location
        Debug.Log("Respawning");
        yield return new WaitForSeconds(3f);

        StartCoroutine(ResetCamSpeed());
        AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex);
        Player.instance.transform.position = activeSave.playerRespawnPosition;

        Player.instance.Respawn();
        EnemyManager.Instance.ResetAllEnemies();
    }
}
