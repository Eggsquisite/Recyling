using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

enum Direction
{
    Left, 
    Right,
    Up,
    Down
}

public class LoadArea : MonoBehaviour
{
    private CameraFollow cam;
    private Animator transition;
    private Transform player;

    [SerializeField]
    private bool liveBossArena;
    [SerializeField] [Tooltip ("If bossArena is true, give arena an index to reference")]
    private int bossArenaIndex;

    private static bool loadReady = true;
    public static bool isLoading;

    private Coroutine loadReadyRoutine;

    // Each load area corresponds to ONE specific area that it will change the camera clamp values to
    [Header("Camera Clamp Values")]
    [SerializeField]
    private int areaToLoadIndex;
    [SerializeField]
    private float min_X;
    [SerializeField]
    private float max_X;

    [Header("Enemy Areas")]
    [SerializeField]
    private GameObject enemiesToDisable;
    [SerializeField]
    private GameObject enemiesToEnable;

    [Header("Backgrounds")]
    [SerializeField]
    private GameObject backgroundToDisable;
    [SerializeField]
    private GameObject backgroundToEnable;

    private GameObject[] enemies;

    [Header("Direction")]
    [SerializeField]
    private float distanceToLoadPlayer;
    [SerializeField]
    private Direction areaToLoad;

    private void Awake() 
    {
        if (cam == null) cam = Camera.main.GetComponent<CameraFollow>();
        if (transition == null) transition = cam.GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        CheckBossArena();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // if player is not currently fighting a boss, load next area
        if (collision != null && collision.tag == "Player" && loadReady && !GameManager.instance.GetIsFightingBoss()) {
            isLoading = true;
            loadReady = false;

            player = collision.gameObject.transform;
            //playerManager.SetInvincible(true);
            //playerManager = collision.GetComponent<Player>();

            StartCoroutine(LoadNextArea());
            if (loadReadyRoutine != null)
                StopCoroutine(LoadReadyTimer());
            loadReadyRoutine = StartCoroutine(LoadReadyTimer());
        }
    }

    public IEnumerator LoadAreaFromSave(bool deadFlag) {
/*        if (deadFlag)
            transition.Play("DeadFadeIn");
        else*/
        transition.Play("Faded");

        yield return new WaitForSeconds(0f);

        CheckBossArena();
        SaveArea();
        cam.SetMinX(min_X);
        cam.SetMaxX(max_X);

        if (backgroundToEnable != null && backgroundToDisable != null)
        {
            backgroundToDisable.SetActive(false);
            backgroundToEnable.SetActive(true);
        }

        StartCoroutine(LoadReady(.5f, deadFlag));
    }

    IEnumerator LoadNextArea() {
        transition.Play("FadeIn");
        Player.instance.SetCollider(false);
        Player.instance.SetInvincible(true);
        Player.instance.SetStopMovement(true);
        //Player.instance.Stunned();

        CheckBossArena();
        // Save index of current area if bossArena is false, else current area is a bossArena 
        if (!liveBossArena)
            SaveArea();
        else { 
            GameManager.instance.SetBossArenaIndex(bossArenaIndex);
        }

        yield return new WaitForSeconds(0.5f);

        // Disable all enemies in the current area
        foreach (Transform enemy in enemiesToDisable.GetComponentsInChildren<Transform>())
        {
            if (enemy.GetComponent<BasicEnemy>() != null)
                enemy.GetComponent<BasicEnemy>().SetIsInactive(true);
        }

        // Disable current background scripts and enable next background scripts
        if (backgroundToEnable != null && backgroundToDisable != null) { 
            backgroundToDisable.SetActive(false);
            backgroundToEnable.SetActive(true);
        }

        // Depending on Direction value, move camera to the corresponding position
        if (areaToLoad == Direction.Right) 
        {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(min_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x + distanceToLoadPlayer, player.position.y);
        } 
        else if (areaToLoad == Direction.Left) 
        {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(max_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x - distanceToLoadPlayer, player.position.y);
        }
        else if (areaToLoad == Direction.Up) { 

        } else if (areaToLoad == Direction.Down) { 

        }

        StartCoroutine(LoadReady(0.25f, false));
    }

    IEnumerator LoadReady(float waitTime, bool deadFlag) {
        // when loading into a new scene, case for deleting boss healthbar if not already set inactive
        GameManager.instance.SetBossHealthbar(false);

        yield return new WaitForSeconds(waitTime);
        isLoading = false;
        cam.ResetBorders();
        transition.Play("FadeOut");

        StartCoroutine(EnableEnemies());
        // If player is dead, reset all enemies (excluding dead bosses)
        if (deadFlag)
            EnemyManager.Instance.ResetAllEnemies();

        Player.instance.SetCollider(true);
        Player.instance.SetStopMovement(false);
        //Player.instance.StartResetStunRoutine(0f);

        yield return new WaitForSeconds(1f);
        Player.instance.SetInvincible(false);
        //GameManager.instance.RescanPathfinding();

        if (deadFlag) { 
            Player.instance.Respawn();
            //Player.instance.RefreshResources();
        }
    }

    IEnumerator EnableEnemies() {
        // Wait to enable enemies to prevent enemies attacking while player loads in
        yield return new WaitForSeconds(1.5f);
        foreach (Transform enemy in enemiesToEnable.GetComponentsInChildren<Transform>())
        {
            if (enemy.GetComponent<BasicEnemy>() != null)
                enemy.GetComponent<BasicEnemy>().SetIsInactive(false);
        }
    }

    /// <summary>
    /// Delay before player can load into another area
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadReadyTimer() {
        yield return new WaitForSeconds(2.5f);
        loadReady = true;
    }

    /// <summary>
    /// Save the current index (position in array of areaLoader) to load
    /// </summary>
    private void SaveArea() {
        SaveManager.instance.SaveAreaToLoad(areaToLoadIndex);
    }

    private void CheckBossArena() {
        // If this value is true, that means the corresponding boss is TRUE and has been DEFEATED
        if (GameManager.instance.CheckBossArenaIndex(bossArenaIndex)) {
            liveBossArena = false;
            Debug.Log("Saving area: " + liveBossArena);
        }
    }

    // FOR AREA MANAGER TO REFERENCE WHEN LOADING GAME
    public float GetMinCamX() {
        return min_X;
    }

    public float GetMaxCamX() {
        return max_X;
    }

    public int GetAreaIndex() {
        return areaToLoadIndex;
    }
}
