using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    public static AreaManager instance;
    private LoadArea[] areaLoader;

    private bool areaFound;

    private void Awake()
    {
        instance = this;
        areaFound = false;

        areaLoader = GetComponentsInChildren<LoadArea>();
    }

    public void CameraFade() { 
        
    }

    public void LoadArea(int index, bool deadFlag)
    {
        areaFound = false;
        if (areaLoader.Length > 0)
        {
            for (int i = 0; i < areaLoader.Length; i++)
            {
                if (areaFound)
                    return;

                if (areaLoader[i].GetAreaIndex() == index)
                {
                    StartCoroutine(areaLoader[i].LoadAreaFromSave(deadFlag));
                    areaFound = true;
                }
            }
        }
    }
}
