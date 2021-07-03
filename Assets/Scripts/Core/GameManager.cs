using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SaveData activeSave;
    private CameraFollow cam;
    private Animator transition;

    public GameObject deathPanel;
    public GameObject deathObject;
    public Vector3 newGameSpawn;

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

            Player.instance.SetInvincible(true);
            Player.instance.LoadPlayerLevels();

            if (!activeSave.playerIsDead) { 
                AreaManager.instance.LoadArea(activeSave.areaToLoadIndex, false);
                Player.instance.transform.position = activeSave.playerCurrentPosition;

                Player.instance.LoadCurrency(activeSave.playerCurrency);
                Player.instance.LoadHealth(activeSave.playerHealth);
                Player.instance.LoadEnergy(activeSave.playerEnergy);
            } else {
                AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex, false);
                Player.instance.transform.position = activeSave.playerRespawnPosition;
                Player.instance.LoadCurrency(0);

                Player.instance.RefreshResources();
                activeSave.playerIsDead = false;
            }

        } else
        {
            // player starts at starting position
            Player.instance.transform.position = newGameSpawn;
            activeSave.playerRespawnPosition = newGameSpawn;
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
        activeSave.playerDeathPosition = Player.instance.transform.position;
        Player.instance.transform.position = activeSave.playerRespawnPosition;

        yield return new WaitForSeconds(0.25f);
        if (deathPanel != null) deathPanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        // spawn deathObject at players death position
        if (deathObject != null) {
            Instantiate(deathObject, Player.instance.transform.position, Quaternion.identity);
        }

        if (deathPanel != null) deathPanel.SetActive(false);
        StartCoroutine(ResetCamSpeed());
        AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex, true);

        Player.instance.RefreshResources();
        Player.instance.SetInvincible(true);
        //Player.instance.transform.position = activeSave.playerRespawnPosition;
    }
}
