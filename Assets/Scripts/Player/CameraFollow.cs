using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Range(0, 10)]
    public float camSpeed;
    public bool canFollow;

    private Vector2 targetOffset;
    private Transform originalTarget;

    private void Start()
    {
        originalTarget = target;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!canFollow)
            return;
        else
        {
            Vector3 targetPosition = new Vector3(target.position.x + targetOffset.x, 
                                                    transform.position.y, 
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void SetCameraTarget(Transform newTarget, Vector2 newOffset) {
        target = newTarget;
        targetOffset = newOffset;
    }

    public void ResetCameraTarget() {
        target = originalTarget;
        targetOffset = Vector2.zero;
    }
}
