using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SortingOrder : MonoBehaviour
{
    [SerializeField]
    private Transform reference;
    private SpriteRenderer sp;

    // Start is called before the first frame update
    void Start()
    {
        if (sp == null) sp = GetComponent<SpriteRenderer>();        
    }

    // Update is called once per frame
    void Update()
    {
        if (sp == null)
            return;

        if (reference == null)
            sp.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
        else
            sp.sortingOrder = Mathf.RoundToInt(reference.position.y * 100f) * -1;
    }
}
