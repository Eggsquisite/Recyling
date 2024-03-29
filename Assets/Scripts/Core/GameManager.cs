using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SaveData activeSave;
    private CameraFollow cam;
    private Animator transition;

    public float cameraShakeSpeed;
    public GameObject bossHealthBar;
    public GameObject deathPanel;
    public GameObject deathObject;
    public Vector3 newGameSpawn;

    private int bossArenaIndex;
    private bool isFightingBoss;
    private Vector3 tmpVector;
    private Coroutine cameraShakeRoutine;

    private void Awake()
    {
        if (instance == null)
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
            StartCoroutine(IncreaseCamSpeed());

            Player.instance.SetInvincible(true);
            Player.instance.LoadPlayerLevels();

            // if spawn DeathObject is true, spawn it at players' death position
            if (deathObject != null && activeSave.spawnDeathObject) {
                var tmp = Instantiate(deathObject, activeSave.playerDeathPosition, Quaternion.identity, null);
                tmp.GetComponent<DeathPickup>().SetCurrencyStored(activeSave.playerLostCurrency);
            }

            // If player is not dead, load them at their last position with the same health, energy, and currency
            if (!activeSave.playerIsDead) { 
                AreaManager.instance.LoadArea(activeSave.areaToLoadIndex, false);
                Player.instance.transform.position = activeSave.playerCurrentPosition;

                Player.instance.LoadCurrency(activeSave.playerCurrency);
                Player.instance.LoadHealth(activeSave.playerHealth);
                Player.instance.LoadEnergy(activeSave.playerEnergy);
            } 
            // if player IS dead, reset all enemies and load them at their respawn position
            else {
                EnemyManager.Instance.ResetAllEnemies();
                AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex, true);
                Player.instance.transform.position = activeSave.playerRespawnPosition;
                Player.instance.LoadCurrency(activeSave.playerCurrency);

                Player.instance.RefreshResources();
                activeSave.playerIsDead = false;
            }

        } else {
            // player starts at starting position

            Debug.Log("Loading new save");
            Player.instance.transform.position = newGameSpawn;
            activeSave.playerRespawnPosition = newGameSpawn;
            activeSave.areaToLoadIndex = 1;
            cam.SetMinX(0);
            cam.SetMaxX(0);
        }
    }

    IEnumerator IncreaseCamSpeed() {
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
        SetIsFightingBoss(false);
        transition.Play("PlayerDead");

        SaveManager.instance.SaveSpawnDeathObject(true);
        activeSave.playerDeathPosition = Player.instance.transform.position;

        // this will move the camera 
        //Player.instance.transform.position = activeSave.playerRespawnPosition;

        yield return new WaitForSeconds(0.25f);
        if (deathPanel != null) deathPanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (deathPanel != null) deathPanel.SetActive(false);
        // spawn deathObject at players death position
        if (deathObject != null) {
            var tmp = Instantiate(deathObject, activeSave.playerDeathPosition, Quaternion.identity, null);
            tmp.GetComponent<DeathPickup>().SetCurrencyStored(activeSave.playerLostCurrency);
        }

        StartCoroutine(IncreaseCamSpeed());
        AreaManager.instance.LoadArea(activeSave.areaToRespawnIndex, true);
        Player.instance.transform.position = activeSave.playerRespawnPosition;

        Player.instance.RefreshResources();
        Player.instance.SetInvincible(true);
        //Player.instance.transform.position = activeSave.playerRespawnPosition;
    }

    public void RescanPathfinding() {
        //AstarPath.active.Scan();
    }

    public void BeginCameraShake(float duration, float magnitude) {
        if (cameraShakeRoutine != null)
            StopCoroutine(cameraShakeRoutine);
        cameraShakeRoutine = StartCoroutine(CameraShake(duration, magnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude) {
        Vector3 originalPos = cam.transform.parent.transform.position;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (cam.transform.localPosition != tmpVector) { 
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-0.15f, 0.15f) * magnitude;

                tmpVector = new Vector3(x, y, originalPos.z);
            }

            cam.transform.parent.localPosition = Vector3.MoveTowards(cam.transform.parent.localPosition,
                                                                        tmpVector,
                                                                        cameraShakeSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.parent.localPosition = originalPos;
    }

    /// <summary>
    /// Set boss healthbar active based off of flag - if flag is true, set isFightingBoss true
    /// </summary>
    /// <param name="flag"></param>
    public void SetBossHealthbar(bool flag) { 
        if (bossHealthBar != null) {
            bossHealthBar.SetActive(flag);
        }
    }

    /// <summary>
    /// When a boss arena is loaded, set bossArenaIndex to the index of the boss arena
    /// When player beats the boss, set that index false in SaveManager 
    /// </summary>
    /// <param name="index"></param>
    public void SetBossArenaIndex(int index) {
        bossArenaIndex = index;
        SetIsFightingBoss(true);
    }

    public void BossDefeated() {
        SetIsFightingBoss(false);
        SaveManager.instance.SaveAreaToLoad(activeSave.areaToLoadIndex + 1);

        if (bossArenaIndex == 1)
            activeSave.bossArenaOneDefeated = true;
        else if (bossArenaIndex == 2)
            activeSave.bossArenaTwoDefeated = true;
        else if (bossArenaIndex == 3)
            activeSave.bossArenaThreeDefeated = true;
    }

    private void SetIsFightingBoss(bool flag) {
        isFightingBoss = flag;
    }

    public bool GetIsFightingBoss() {
        return isFightingBoss;
    }

    /// <summary>
    /// Called when game is Loaded, 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool CheckBossArenaIndex(int index) { 
        if (index == 1) {
            return activeSave.bossArenaOneDefeated;
        } else if (index == 2) {
            return activeSave.bossArenaTwoDefeated;
        } else if (index == 3) {
            return activeSave.bossArenaThreeDefeated;
        }
        return false;
    }
}
