using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SortingOrder : MonoBehaviour
{
    private SpriteRenderer sp;

    // Start is called before the first frame update
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();        
    }

    // Update is called once per frame
    void Update()
    {
        sp.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
    }
}
