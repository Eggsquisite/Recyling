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
    public static bool isLoading;

    // Each load area corresponds to ONE specific area that it will change the camera clamp values to
    [Header("Camera Clamp Values")]
    [SerializeField]
    private float min_X;
    [SerializeField]
    private float max_X;

    [Header("Areas")]
    [SerializeField]
    private GameObject areaToDisable;
    [SerializeField]
    private GameObject areaToEnable;

    [Header("Direction")]
    [SerializeField]
    private float distanceToLoadPlayer;
    [SerializeField]
    private Direction areaToLoad;

    private void Start()
    {
        if (cam == null) cam = Camera.main.GetComponent<CameraFollow>();
        if (transition == null) transition = cam.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "Player") {
            isLoading = true;
            player = collision.gameObject.transform;
            playerManager = collision.GetComponent<Player>();
            playerManager.SetInvincible(true);
            StartCoroutine(LoadNextArea());
        }
    }

    IEnumerator LoadNextArea() {
        transition.Play("FadeIn");
        playerManager.SetStopMovement(true);
        yield return new WaitForSeconds(0.5f);

        areaToDisable.SetActive(false);
        areaToEnable.SetActive(true);

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

        yield return new WaitForSeconds(.5f);
        isLoading = false;
        cam.ResetBorders();
        playerManager.SetStopMovement(false);
        playerManager.SetInvincible(false);
        transition.Play("FadeOut");
    }
}
