using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Player player;
    private float xAxis;
    private float yAxis;

    private void Start()
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

        // ATTACK INPUTS
        if (Input.GetKeyDown(KeyCode.Mouse0))
            player.BasicAttackInput();
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            player.SuperAttackInput();

        // DASH INPUT
        if (Input.GetKeyDown(KeyCode.Space))
            player.DashInput();
    }
}
