using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    public List<LoadArea> areaLoader;

    // Start is called before the first frame update
    void Start()
    {
        if (areaLoader.Count > 0)
        {
            foreach (LoadArea area in areaLoader)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
