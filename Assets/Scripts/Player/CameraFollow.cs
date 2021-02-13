using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [Range(0, 10)]
    public float camSpeed;
    public bool canFollow;

    // Update is called once per frame
    void LateUpdate()
    {
        if (!canFollow)
            return;
        else
        { 
            Vector3 targetPosition = new Vector3(target.position.x + offset.x, 
                                                    transform.position.y, 
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
    }
}
