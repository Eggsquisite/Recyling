using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SaveData activeSave;
    private CameraFollow cam;
    private Animator transition;

    public GameObject deathObject;
    public Vector3 newSpawn;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        activeSave = SaveManager.instance.activeSave;
        if (cam == null) cam = Camera.main.GetComponent<CameraFollow>();
        if (transition == null) transition = cam.GetComponentInChildren<Animator>();

        if (SaveManager.instance.hasLoaded)
        {
            // Disable all enemies
            EnemyManager.Instance.DisableAllEnemies();

            // player loads in at their last position
            StartCoroutine(ResetCamSpeed());
            AreaManager.instance.LoadArea(activeSave.areaToLoadIndex, false);
            Player.instance.transform.position = activeSave.playerCurrentPosition;

            Player.instance.SetInvincible(true);
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
        transition.Play("PlayerDead");
        yield return new WaitForSeconds(2f);

        // spawn deathObject at players death position
        if (deathObject != null) {
            Instantiate(deathObject, Player.instance.transform.position, Quaternion.identity);
        }

        StartCoroutine(ResetCamSpeed());
        Player.instance.SetInvincible(true);
        AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex, true);
        Player.instance.transform.position = activeSave.playerRespawnPosition;
    }
}
