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
    private Player playerManager;

    private static bool loadReady = true;
    public static bool isLoading;

    private Coroutine loadReadyRoutine;

    // Each load area corresponds to ONE specific area that it will change the camera clamp values to
    [Header("Camera Clamp Values")]
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "Player" && loadReady) {
            isLoading = true;
            loadReady = false;

            player = collision.gameObject.transform;
            playerManager = collision.GetComponent<Player>();
            playerManager.SetInvincible(true);

            StartCoroutine(LoadNextArea());
            if (loadReadyRoutine != null)
                StopCoroutine(LoadReadyTimer());
            loadReadyRoutine = StartCoroutine(LoadReadyTimer());
        }
    }

    IEnumerator LoadNextArea() {
        transition.Play("FadeIn");
        playerManager.SetStopMovement(true);
        playerManager.SetCollider(false);
        yield return new WaitForSeconds(0.5f);

        foreach (Transform enemy in enemiesToDisable.GetComponentsInChildren<Transform>())
        {
            if (enemy.GetComponent<BasicEnemy>() != null)
                enemy.GetComponent<BasicEnemy>().SetIsInactive(true);
        }
        
        foreach (Transform enemy in enemiesToEnable.GetComponentsInChildren<Transform>())
        {
            if (enemy.GetComponent<BasicEnemy>() != null)
                enemy.GetComponent<BasicEnemy>().SetIsInactive(false);
        }

        if (backgroundToEnable != null && backgroundToDisable != null) { 
            backgroundToDisable.SetActive(false);
            backgroundToEnable.SetActive(true);
        }

        if (areaToLoad == Direction.Right) {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(min_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x + distanceToLoadPlayer, player.position.y);

            // enable area
            // disable previous area
        } else if (areaToLoad == Direction.Left) {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(max_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x - distanceToLoadPlayer, player.position.y);

            // enable area
            // disable previous area
        }
        else if (areaToLoad == Direction.Up) { 

        } else if (areaToLoad == Direction.Down) { 

        }

        yield return new WaitForSeconds(0.25f);
        isLoading = false;
        cam.ResetBorders();
        transition.Play("FadeOut");
        playerManager.SetCollider(true);
        playerManager.SetStopMovement(false);

        yield return new WaitForSeconds(1f);
        playerManager.SetInvincible(false);
    }

    IEnumerator LoadReadyTimer() {
        yield return new WaitForSeconds(2.5f);
        loadReady = true;
    }
}
