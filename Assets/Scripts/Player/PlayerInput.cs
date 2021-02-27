using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    private Collider2D interactable;
    private Player player;
    private float xAxis;
    private float yAxis;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
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

        // ATTACK INPUTS
        if (Input.GetKeyDown(KeyCode.Mouse0))
            player.BasicAttackInput();
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            player.SuperAttackInput();

        // DASH INPUT
        if (Input.GetKeyDown(KeyCode.Space))
            player.DashInput();
        else if (Input.GetKeyUp(KeyCode.Space))
            player.StopDashInput();

        // HEAL INPUT
        if (Input.GetKeyDown(KeyCode.LeftControl))
            player.RecoverInput();
        else if (Input.GetKeyUp(KeyCode.LeftControl))
            player.StopRecoverInput();

        // INTERACTION INPUT
        if (Input.GetKeyDown(KeyCode.E)) {
            interactable = Physics2D.OverlapCircle(transform.position, 1f, interactableLayer);
            if (interactable != null) {
                interactable.GetComponent<Interactable>().Interacting();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
