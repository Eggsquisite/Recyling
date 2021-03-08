using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadArea : MonoBehaviour
{
    private CameraFollow cam;
    private Animator transition;
    private Transform player;
    private Player playerManager;

    // Each load area corresponds to ONE specific area that it will change the camera clamp values to
    [Header("Camera Clamp Values")]
    [SerializeField]
    private float min_X;
    [SerializeField]
    private float max_X;

    [Header("Direction")]
    public bool areaToRight;
    public bool areaToLeft;
    public bool areaAbove;
    public bool areaBelow;

    private void Start()
    {
        if (cam == null) cam = Camera.main.GetComponent<CameraFollow>();
        if (transition == null) transition = cam.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "Player") {
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

        if (areaToRight) {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(min_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x + 2.5f, player.position.y);

            // enable area
            // disable previous area
        } else if (areaToLeft) {
            cam.SetMinX(min_X);
            cam.SetMaxX(max_X);
            cam.transform.position = new Vector3(max_X, cam.transform.position.y, cam.transform.position.z);
            player.position = new Vector2(player.position.x - 2.5f, player.position.y);

            // enable area
            // disable previous area
        }
        else if (areaAbove) { 

        } else if (areaBelow) { 

        }

        yield return new WaitForSeconds(.5f);
        cam.ResetBorders();
        playerManager.SetStopMovement(false);
        playerManager.SetInvincible(false);
        transition.Play("FadeOut");
    }
}
