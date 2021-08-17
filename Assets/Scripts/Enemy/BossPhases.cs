using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhases : MonoBehaviour
{
    [Header("Components")]
    private BasicEnemy enemy;

    [SerializeField] [Tooltip ("Health percent at which boss begins next phase (If null, set to 0)")]
    private float healthThresholdPercent;
    [SerializeField] [Tooltip ("Speed at which boss carries out certain animations (Default: 1)")]
    private float animSpeedMult;
    [SerializeField]
    private float moveSpeedMult;
    [SerializeField] [Tooltip("Amount to reduce both min/max attack delay")]
    private float attackDelayReduction;

    private int currentPhase;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void InitializePhaseVariables(out float thresholdPercent,
                                                out float animSpeed,
                                                out float moveSpeed,
                                                out float attackDelay) 
    {
        thresholdPercent = healthThresholdPercent;
        animSpeed = animSpeedMult;
        moveSpeed = moveSpeedMult;
        attackDelay = attackDelayReduction;
    }
}
