using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Animator anim;
    private Camera cam;

    [Header("Follow Properties")]
    public Transform target;
    public Transform leftBorder;
    public Transform rightBorder;
    public Transform bottomBorder;

    public float min_X, max_X;
    public float camSpeed;
    public bool canFollow;

    private float borderOffset = 7.5f;
    private Vector2 targetOffset;
    private Transform originalTarget;

    [Header("Camera Change Properties")]
    public float focusSize;
    public float focusDuration;
    public float min_Y, max_Y;

    private float focusMin_X, focusMax_X;
    private float timeElapsed = 0.0f;
    private float startingSize;
    private float currentSize;
    private bool isFocused;

    private void Awake() {
        if (anim == null) anim = GetComponent<Animator>();
        if (cam == null) cam = GetComponent<Camera>();
        originalTarget = target;
        startingSize = cam.orthographicSize;
        currentSize = cam.orthographicSize;
        focusMin_X = min_X - 3.5f;
        focusMax_X = max_X + 3.5f;

        leftBorder.parent = null;
        rightBorder.parent = null;
        bottomBorder.parent = null;
        leftBorder.position = new Vector3(min_X - borderOffset,
                                            transform.position.y,
                                            transform.position.z);
        rightBorder.position = new Vector3(max_X + borderOffset,
                                            transform.position.y,
                                            transform.position.z);

    }

    private void Update()
    {
        if (isFocused)
            FocusCamera();
        else
            ResetFocusCamera();
    }

    // Update is called once per frame
    void LateUpdate() {
        if (!canFollow)
            return;
        else if (isFocused)
        {
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x, 
                                                    focusMin_X, focusMax_X),
                                                    Mathf.Clamp(target.position.y + targetOffset.y,
                                                    min_Y, max_Y), 
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        } else {
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x,
                                                    min_X, max_X),
                                                    2f,
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void SetCameraTarget(Transform newTarget, Vector2 newOffset) {
        target = newTarget;
        targetOffset = newOffset;
    }

    public void SetIsFocused(bool flag) {
        currentSize = cam.orthographicSize;
        timeElapsed = 0.0f;
        isFocused = flag;
    }

    public void FocusCamera() {
        if (timeElapsed < focusDuration) { 
            timeElapsed += Time.deltaTime / focusDuration;
            var tmp = Mathf.Lerp(currentSize, focusSize, timeElapsed);
            cam.orthographicSize = tmp;
        }
    }

    public void ResetFocusCamera() {
        if (timeElapsed < focusDuration) {
            timeElapsed += Time.deltaTime / focusDuration;
            var tmp = Mathf.Lerp(currentSize, startingSize, timeElapsed);
            cam.orthographicSize = tmp;
        }
    }

    public void ResetCameraTarget() {
        target = originalTarget;
        targetOffset = Vector2.zero;
    }
}
