using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    private Collider2D interactable;
    private Player player;
    private PlayerStats playerStats;

    private float xAxis;
    private float yAxis;

    private bool isInteracting;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        // MOVEMENT INPUTS
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        player.CheckForMovement(xAxis, yAxis);

        // RUN INPUT
        if (Input.GetKeyDown(KeyCode.LeftShift))
            player.ShiftToRun(true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            player.ShiftToRun(false);

        if (!isInteracting) {
            // SWITCH WEAPON
            if (Input.GetKeyDown(KeyCode.Q))
                player.SwitchWeaponInput();

            // ATTACK INPUTS
            if (player.GetPlayerWeapon() == 1)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    player.BasicAttackInput();
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                    player.SuperAttackInput();
            } else if (player.GetPlayerWeapon() == 2)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                    player.BlasterAttackInput();
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                    player.SuperBlasterAttackInput();

            }

            // DODGE INPUT
            if (Input.GetKeyDown(KeyCode.Space))
                player.DodgeInput();

            // HEAL INPUT
            if (Input.GetKeyDown(KeyCode.LeftControl))
                player.RecoverInput();
            else if (Input.GetKeyUp(KeyCode.LeftControl))
                player.StopRecoverInput();
        
            if (Input.GetKeyDown(KeyCode.Y))
                playerStats.IncreaseStat(3, 4);
        }

        // INTERACTION INPUT
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!isInteracting) 
            {
                var tmp = Physics2D.OverlapCircle(transform.position, 1f, interactableLayer);
                if (tmp != null) { 
                    interactable = tmp;
                    isInteracting = true;
                }
                if (interactable != null && interactable.GetComponent<Interactable>().GetIsReady()) {
                    // If player is interacting, set movement to 0 and stop all other inputs
                    interactable.GetComponent<Interactable>().Interacting(gameObject);

                    player.CheckForMovement(0f, 0f);

                    // SAVE SPAWN POINT HERE *************************
                    SaveManager.instance.SaveSpawnPoint();
                }
            } else if (isInteracting) 
            { 
                if (interactable != null 
                        && interactable.GetComponent<Interactable>().GetIsReady()
                        && Vector2.Distance(interactable.transform.position, transform.position) <= 2.5f) {
                    isInteracting = false;
                    interactable.GetComponent<Interactable>().Interacting(gameObject);
                    interactable = null;

                    SaveManager.instance.Save(); // after interacting
                }
            }
        }

        if (isInteracting 
                && interactable != null 
                && Vector2.Distance(interactable.transform.position, transform.position) > 2.5f) {
            isInteracting = false;
            interactable.GetComponent<Interactable>().Interacting(gameObject);
            interactable = null;

            SaveManager.instance.Save(); // after interacting
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
