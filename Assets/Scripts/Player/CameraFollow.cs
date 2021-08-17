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
    private float min_Y;
    [SerializeField]
    private float max_Y;
    [SerializeField]
    private float camSpeed;
    [SerializeField]
    private bool canFollow;

    private Vector2 targetOffset;
    private Transform target;
    private Transform originalTarget;

    private float borderOffset = 7.5f;

    [Header("Camera Change Properties")]
    public float focusSize;
    public float focusDuration;
    public float focusMin_Y, focusMax_Y;

    private float focusMin_X, focusMax_X;
    private float timeElapsed = 0.0f;
    private float startingSize;
    private float currentSize;
    private bool isFocused;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (cam == null) cam = GetComponent<Camera>();

        startingSize = cam.orthographicSize;
        currentSize = cam.orthographicSize;
        focusMin_X = min_X - 1.5f;
        focusMax_X = max_X + 1.5f;

        leftBorder.parent = null;
        rightBorder.parent = null;
        bottomBorder.parent = null;
        ResetBorders();
    }

    private void Start()
    {
        if (target == null) target = Player.instance.transform;
        originalTarget = target;
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
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x, focusMin_X, focusMax_X),
                                            Mathf.Clamp(target.position.y + 1.25f, focusMin_Y, focusMax_Y),
                                            transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
        else
        {
            // y offset allows camera to follow player more closely
            Vector3 targetPosition = new Vector3(Mathf.Clamp(target.position.x + targetOffset.x, min_X, max_X),
                                            Mathf.Clamp(target.position.y + 1.25f, min_Y, max_Y),
                                            transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, camSpeed * Time.fixedDeltaTime);
            transform.position = smoothedPosition;
        }
    }

    public void SetCameraTarget(Transform newTarget, Vector2 newOffset)
    {
        target = newTarget;
        //targetOffset = newOffset;
    }

    public void SetIsFocused(bool flag)
    {
        currentSize = cam.orthographicSize;
        timeElapsed = 0.0f;
        isFocused = flag;
    }

    public void FocusCamera()
    {
        // need to slow camera zoom
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
