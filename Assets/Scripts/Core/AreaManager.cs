using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    public static AreaManager instance;
    public List<LoadArea> areaLoader;

    private bool areaFound;

    private void Awake()
    {
        instance = this;
        areaFound = false;
    }

    public void LoadArea()
    {
        if (areaLoader.Count > 0)
        {
            for (int i = 0; i < areaLoader.Count; i++)
            {
                if (areaFound)
                    return;

                if (areaLoader[i].GetAreaIndex() == SaveManager.instance.activeSave.areaToLoadIndex)
                {
                    areaLoader[i].LoadAreaFromSave();
                    areaFound = true;
                }
            }
        }
    }
}
