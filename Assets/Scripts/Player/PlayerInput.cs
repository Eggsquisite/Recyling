using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    private Collider2D interactable;
    public Interactable interactObject;
    public List<Interactable> interactList;
    private Player player;
    private PlayerStats playerStats;

    private float xAxis;
    private float yAxis;

    private int smallestIndex;
    private bool isInteracting;
    private bool isInInteractRange;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();

        interactList = new List<Interactable>();
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

            // DEV TESTING STUFF
            if (Input.GetKeyDown(KeyCode.Y))
                //playerStats.IncreaseStat(3, 4);
                Player.instance.GetComponent<PlayerUI>().SetCurrency(100);
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        // INTERACTION INPUT
/*        if (Input.GetKeyDown(KeyCode.E)) {
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
        }*/

        if (Input.GetKeyDown(KeyCode.E)) { 
            if (isInInteractRange) {
                interactObject = CheckInteractListDistances();
                DisableOtherInteractables();
                if (interactObject != null && interactObject.GetIsReady()) {
                    if (!interactObject.GetIsActive())
                        isInteracting = true;
                    else
                        isInteracting = false;

                    interactObject.Interacting(gameObject);
                    SaveManager.instance.Save(); // saving when interacting
                }
            }
        }
    }

    /// <summary>
    /// Iterates through all the interactions in contact with player and picks the closest one
    /// NOT WORKING CORRECTLY, ALWAYS CHOOSING LAST ELEMENT IN ARRAY REGARDLESS OF DISTANCE
    /// </summary>
    /// <returns></returns>
    private Interactable CheckInteractListDistances() {
        float tmpDistance = 0f;
        float currentDistance = 0f;
        smallestIndex = 0;

        if (interactList.Count == 0) {
            return null;
        }
        else if (interactList.Count == 1) {
            smallestIndex = 0;
            return interactList[0];
        }
        else if (interactList.Count > 1)
        {
            smallestIndex = 0;
            currentDistance = Vector2.Distance(transform.position, interactList[0].transform.position);
            for (int i = 1; i < interactList.Count; i++) {
                tmpDistance = Vector2.Distance(transform.position, interactList[i].transform.position);
                if (tmpDistance < currentDistance) { 
                    currentDistance = tmpDistance;
                    smallestIndex = i;
                }
            }

            return interactList[smallestIndex];
        }

        return null;
    }

    /// <summary>
    /// Iterate through interact list, and turn off each active interactable whenever player begins 
    /// a new interaction
    /// </summary>
    private void DisableOtherInteractables() {
        for(int i = 0; i < interactList.Count; i++)
        {
            if (interactList[i].GetIsActive() && i != smallestIndex)
                interactList[i].Interacting(gameObject);
        }
    }

    /// <summary>
    /// If player is out of interact range and is interacting with an object, turn off that interactable
    /// </summary>
    /// <param name="flag"></param>
    public void SetIsInInteractRange(bool flag) {
        isInInteractRange = flag;
        if (!flag) {
            isInteracting = false;

            if (interactObject != null && interactObject.GetIsActive()) {
                Debug.Log("Turning off");
                interactObject.Interacting(gameObject);

                SaveManager.instance.Save(); // saving when out of range of interactable 
            }
        }
    }

    public void AddInteractObject(Interactable newObject) {
        interactList.Add(newObject);
    }

    /// <summary>
    /// If removing an interact object from the list (as when player is out of its interact collider), 
    /// remove it from the interactList and deactivate if it is active
    /// </summary>
    /// <param name="index"></param>
    public void RemoveInteractObject(Interactable tmpObject) {
        if (tmpObject.GetIsActive()) { 
            isInteracting = false;
            tmpObject.Interacting(gameObject);
        }

        interactList.Remove(tmpObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Interactable")
            isInInteractRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Interactable")
            isInInteractRange = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
