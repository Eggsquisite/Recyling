using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    public float length;

    private Transform cam;
    private float startPos;

    private void Start()
    {
        startPos = transform.position.x;
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}
