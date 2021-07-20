using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SortingOrder : MonoBehaviour
{
    [SerializeField]
    private Transform reference;
    [SerializeField]
    private ParticleSystemRenderer prtSystem;

    private SpriteRenderer sp;
    private bool isParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        if (sp == null) sp = GetComponent<SpriteRenderer>();
        if (prtSystem != null) isParticleSystem = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (sp == null)
            return;

        if (reference == null && !isParticleSystem)
            sp.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
        else if (!isParticleSystem)
            sp.sortingOrder = Mathf.RoundToInt(reference.position.y * 100f) * -1;
        else if (isParticleSystem)
            prtSystem.sortingOrder = sp.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
    }
}
