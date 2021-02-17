using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTrigger : MonoBehaviour
{
    public Vector2 cameraOffset;
    private Vector2 cameraStopPosition;

    // Start is called before the first frame update
    void Start()
    {
        cameraStopPosition = (Vector2)transform.position + cameraOffset;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") 
            Camera.main.GetComponent<CameraFollow>().SetCameraTarget(transform, cameraOffset);
    }
}
