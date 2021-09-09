using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool deathPickup;
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

    private Animator anim;
    private Collider2D coll;
    private GameObject player;

    private bool isActive;
    private bool isReady = true;
    private bool isPlayerInRange;
    private Coroutine interactRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (coll == null) coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            PlayerIsInRange(collision.GetComponent<PlayerInput>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player") 
            PlayerIsOutOfRange(collision.GetComponent<PlayerInput>());
    }

    private void PlayerIsInRange(PlayerInput pi) {
        //pi.SetIsInInteractRange(true);
        pi.AddInteractObject(this);
    }

    private void PlayerIsOutOfRange(PlayerInput pi) {
        //pi.SetIsInInteractRange(false);
        pi.RemoveInteractObject(this);
    }

    public void Interacting(GameObject newPlayer) {
        player = newPlayer;
        isActive = !isActive;

        if (isActive) {
            // begin delay on next interaction
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count > 0)
                anim.Play(animations[0]);

            ChooseInteraction();
        }
        else {
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count > 1)
                anim.Play(animations[1]);
        }
    }

    private void ChooseInteraction() {
        // reset enemies if set
        if (resetEnemiesOnUse)
            EnemyManager.Instance.ResetAllEnemies();

        if (healOnUse)
            player.GetComponent<PlayerStats>().RefreshResources();

        // SAVE SPAWN POINT HERE *************************
        if (setSpawnPoint && spawnPoint != null) { 
            SaveManager.instance.SaveSpawnPoint(spawnPoint);
        }

        if (deathPickup) {
            var tmp = GetComponent<DeathPickup>();
            tmp.InteractActivated();
        }
    }

    private void OpenUI() {
        // open up UI - called during animation event
        if (UI == null && UI.Count == 0)
            return;

        for (int i = 0; i < UI.Count; i++) {
            UI[i].SetActive(true);
            UI[i].GetComponent<FindPlayerScript>().PlayerFound(player);
        }
    }

    private void CloseUI() {
        // close down UI - called during animation event
        if (UI == null && UI.Count == 0)
            return;

        for (int i = 0; i < UI.Count; i++) {
            UI[i].SetActive(false);
        }
    }

    public bool GetIsActive() {
        return isActive;
    }

    IEnumerator InteractDelay() {
        isReady = false;
        yield return new WaitForSeconds(interactDelayBtwnActivations);
        isReady = true;
    }

    /// <summary>
    /// Not currently used: spawn point is just player position when they use the altar
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint() {
        if (spawnPoint == null)
            return null;

        return spawnPoint;
    }

    public bool GetIsReady() {
        return isReady;
    }
}
