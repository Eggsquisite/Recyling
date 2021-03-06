using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Animator anim;
    private Camera cam;

    [Header("Follow Properties")]
    [SerializeField]
    private Transform leftBorder;
    [SerializeField]
    private Transform rightBorder;
    [SerializeField]
    private Transform bottomBorder;
    [SerializeField]
    public float min_X;
    [SerializeField]
    private float max_X;
    [SerializeField]
    private float camSpeed;
    [SerializeField]
    private bool canFollow;

    private Transform target;
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

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (cam == null) cam = GetComponent<Camera>();
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        originalTarget = target;

        startingSize = cam.orthographicSize;
        currentSize = cam.orthographicSize;
        focusMin_X = min_X - 1.5f;
        focusMax_X = max_X + 1.5f;

        leftBorder.parent = null;
        rightBorder.parent = null;
        bottomBorder.parent = null;
        ResetBorders();
    }

    private void Update()
    {
        if (isFocused)
            FocusCamera();
        else
            ResetFocusCamera();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!canFollow || target == null)
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
        }
        else
        {
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x,
                                                    min_X, max_X),
                                                    2f,
                                                    transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void SetCameraTarget(Transform newTarget, Vector2 newOffset)
    {
        target = newTarget;
        targetOffset = newOffset;
    }

    public void SetIsFocused(bool flag)
    {
        currentSize = cam.orthographicSize;
        timeElapsed = 0.0f;
        isFocused = flag;
    }

    public void FocusCamera()
    {
        if (timeElapsed < focusDuration + 1f) {
            timeElapsed += Time.deltaTime / focusDuration;
            var tmp = Mathf.Lerp(currentSize, focusSize, timeElapsed);
            cam.orthographicSize = tmp;
        }
    }

    public void ResetFocusCamera()
    {
        if (timeElapsed < focusDuration + 1f) {
            timeElapsed += Time.deltaTime / focusDuration;
            var tmp = Mathf.Lerp(currentSize, startingSize, timeElapsed);
            cam.orthographicSize = tmp;
        }
    }

    public void ResetCameraTarget()
    {
        target = originalTarget;
        targetOffset = Vector2.zero;
    }

    public void ResetBorders() {
        leftBorder.position = new Vector3(min_X - borderOffset,
                                    transform.position.y,
                                    transform.position.z);
        rightBorder.position = new Vector3(max_X + borderOffset,
                                    transform.position.y,
                                    transform.position.z);
        bottomBorder.position = new Vector3((min_X + max_X) / 2,
                                    bottomBorder.position.y,
                                    transform.position.z);
    }

    public float GetMinX() {
        return min_X;
    }

    public float GetMaxX() {
        return max_X;
    }

    public void SetCamSpeed(float newValue) {
        camSpeed = newValue;
    }

    public void SetMinX(float newValue) {
        min_X = newValue;
        focusMin_X = min_X - 1.5f;
        SaveManager.instance.activeSave.minCameraPos = newValue;
    }

    public void SetMaxX(float newValue) {
        max_X = newValue;
        focusMax_X = max_X + 1.5f;
        SaveManager.instance.activeSave.maxCameraPos = newValue;
    }
}
