using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Transform leftBorder;
    public Transform rightBorder;
    public float min_X, max_X;

    [Range(0, 10)]
    public float camSpeed;
    public bool canFollow;

    private float borderOffset = 7.5f;
    private Vector2 targetOffset;
    private Transform originalTarget;

    private void Start() {
        originalTarget = target;
        leftBorder.parent = null;
        rightBorder.parent = null;

        leftBorder.position = new Vector3(min_X - borderOffset,
                                            transform.position.y,
                                            transform.position.z);
        rightBorder.position = new Vector3(max_X + borderOffset,
                                            transform.position.y,
                                            transform.position.z);
    }

    // Update is called once per frame
    void LateUpdate() {
        if (!canFollow)
            return;
        else
        {
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x, 
                                                    min_X, max_X), 
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
