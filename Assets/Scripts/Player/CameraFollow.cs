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

    public float min_X, max_X, min_Y, max_Y;
    public float camSpeed;
    public bool canFollow;

    private float borderOffset = 7.5f;
    private Vector2 targetOffset;
    private Transform originalTarget;

    [Header("Camera Change Properties")]
    public float cameraFocus;

    private float baseCameraFocus;
    private bool isFocused;

    private void Start() {
        originalTarget = target;
        leftBorder.parent = null;
        rightBorder.parent = null;
        baseCameraFocus = Camera.main.orthographicSize;
        if (anim == null) anim = GetComponent<Animator>();
        if (cam == null) cam = GetComponent<Camera>();

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
        else if (isFocused)
        {
            FocusCamera();
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x, 
                                                    min_X, max_X),
                                                    Mathf.Clamp(target.position.y + targetOffset.y,
                                                    min_Y, max_Y), 
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        } else {
            ResetFocusCamera();
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
        isFocused = flag;
    }

    public void FocusCamera() {
        cam.orthographicSize = Mathf.SmoothStep(cam.orthographicSize, 3f, Time.deltaTime);
    }

    public void ResetFocusCamera() {
        cam.orthographicSize = Mathf.SmoothStep(cam.orthographicSize, baseCameraFocus, Time.deltaTime);
    }

    public void ResetCameraTarget() {
        target = originalTarget;
        targetOffset = Vector2.zero;
    }
}
