using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool resetEnemiesOnUse;
    [SerializeField]
    private bool healOnUse;
    [SerializeField]
    private bool setSpawnPoint;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private float interactDelayBtwnActivations;
    [SerializeField]
    private List<string> animations;
    [SerializeField]
    private List<GameObject> UI;

    private ResetEnemies reset;
    private Animator anim;
    private GameObject player;
    private bool isActive;
    private bool isReady = true;
    private Coroutine interactRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (reset == null) reset = GetComponent<ResetEnemies>();
    }

    public void Interacting(GameObject newPlayer) {
        if (!isReady)
            return;

        player = newPlayer;
        isActive = !isActive;

        if (isActive) {
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count != 0)
                anim.Play(animations[0]);
        }
        else {
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count != 0)
                anim.Play(animations[1]);
        }
    }

    private void OpenUI() {
        // open up UI
        for (int i = 0; i < UI.Count; i++) {
            UI[i].SetActive(true);
            UI[i].GetComponent<FindPlayerScript>().PlayerFound(player);

            if (healOnUse)
                player.GetComponent<PlayerStats>().RefreshResources();
        }

        if (resetEnemiesOnUse)
            reset.ResetAllEnemies();
    }

    private void CloseUI() { 
        // close down UI
        for (int i = 0; i < UI.Count; i++) {
            UI[i].SetActive(false);
        }
    }

    IEnumerator InteractDelay() {
        isReady = false;
        yield return new WaitForSeconds(interactDelayBtwnActivations);
        isReady = true;
    }

    public Transform GetSpawnPoint() {
        if (spawnPoint == null)
            return null;

        return spawnPoint;
    }

    public bool GetIsReady() {
        return isReady;
    }
}
