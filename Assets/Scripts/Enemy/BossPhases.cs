using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhases : MonoBehaviour
{
    private int currentPhase;
    private bool secondPhase;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool GetSecondPhase() {
        return secondPhase;
    }
}
