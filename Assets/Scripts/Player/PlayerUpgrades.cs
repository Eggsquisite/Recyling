using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{

    [Header("Universal Stat Threshold")]
    [SerializeField]
    private int statUpgradeThreshold1;
    [SerializeField]
    private int statUpgradeThreshold2;

    [Header("Strength Upgrade Properties")]
    [SerializeField] [Tooltip("Faster attack speed multiplier")]
    private float strengthUpgradeValue1;
    [SerializeField] [Tooltip("Player damages surrounding enemies when landing from jetpack")]
    private float strengthUpgradeValue2;

    //[SerializeField]
    private int strengthUpgradeThreshold1;
    //[SerializeField]
    private int strengthUpgradeThreshold2;

    [Header("Special Upgrade Properties")]
    [SerializeField] [Tooltip("Pushback increase to blaster")]
    private float specialUpgradeValue1; 
    [SerializeField] [Tooltip("Overall energy reduction as percentage")]
    private float specialUpgradeValue2;

    //[SerializeField]
    private int specialUpgradeThreshold1;
    //[SerializeField]
    private int specialUpgradeThreshold2;

    [Header("Focus Upgrade Properties")]
    [SerializeField] [Tooltip("Heal walk speed multiplier increase")]
    private float focusUpgradeValue1;

    //[SerializeField]
    private int focusUpgradeThreshold1;
    //[SerializeField]
    private int focusUpgradeThreshold2;

    [Header("Vitality Upgrade Properties")]
    [SerializeField]
    private int vitalityHealthUpgradeValue;

    //[SerializeField]
    private int vitalityUpgradeThreshold1;
    //[SerializeField]
    private int vitalityUpgradeThreshold2;

    [Header("Stamina Upgrade Properties")]
    //[SerializeField]
    private int staminaUpgradeThreshold1;
    //[SerializeField]
    private int staminaUpgradeThreshold2;

    private int strengthUpgradeLevel;
    private int specialUpgradeLevel;
    private int focusUpgradeLevel;
    private int vitalityUpgradeLevel;
    private int staminaUpgradeLevel;

    // Universal upgrade checker; use if all upgrade value thresholds are consitent for each stat
    public int CheckUpgradeLevels(int statLevel) {
        if (statLevel < statUpgradeThreshold1)
            return 0;
        else if (statLevel >= statUpgradeThreshold1 && statLevel < statUpgradeThreshold2)
            return 1;
        else if (statLevel >= statUpgradeThreshold2)
            return 2;
        else
            return 0;
    }

    // STRENGTH PROPERTIES ////////////////////////////////////////////////////////////////////////////////
    public int CheckStrengthUpgrade(int strengthLevel) {
        if (strengthLevel < strengthUpgradeThreshold1)
            return 0;
        else if (strengthLevel >= strengthUpgradeThreshold1 && strengthLevel < strengthUpgradeThreshold2)
            return 1;
        else if (strengthLevel >= strengthUpgradeThreshold2)
            return 2;
        else
            return 0;
    }
    
    public float GetStrengthUpgradeValues(int index) {
        if (index == 1)
            return strengthUpgradeValue1;
        else if (index == 2)
            return strengthUpgradeValue2;
        else
            return 0f;
    }

    // SPECIAL PROPERTIES ////////////////////////////////////////////////////////////////////////////////
    public int CheckSpecialUpgrade(int specialLevel) {
        if (specialLevel < specialUpgradeThreshold1)
            return 0;
        else if (specialLevel >= specialUpgradeThreshold1 && specialLevel < specialUpgradeThreshold2)
            return 1;
        else if (specialLevel >= specialUpgradeThreshold2)
            return 2;
        else
            return 0;
    }

    public float GetSpecialUpgradeValues(int index) {
        if (index == 1)
            return specialUpgradeValue1;
        else if (index == 2)
            return specialUpgradeValue2;
        else
            return 0f;
    }

    // FOCUS PROPERTIES ////////////////////////////////////////////////////////////////////////////////
    public int CheckFocusUpgrade(int focusLevel) {
        if (focusLevel < focusUpgradeThreshold1)
            return 0;
        else if (focusLevel >= focusUpgradeThreshold1 && focusLevel < focusUpgradeThreshold2)
            return 1;
        else if (focusLevel >= focusUpgradeThreshold2)
            return 2;
        else
            return 0;
    }

    public float GetFocusUpgradeValues(int index) {
        if (index == 1)
            return focusUpgradeValue1;
        else
            return 0f;
    }

    // VITALITY PROPERTIES ////////////////////////////////////////////////////////////////////////////////
    public int CheckVitalityUpgrade(int vitalityLevel) {
        if (vitalityLevel < vitalityUpgradeThreshold1)
            return 0;
        else if (vitalityLevel >= vitalityUpgradeThreshold1 && vitalityLevel < vitalityUpgradeThreshold2)
            return 1;
        else if (vitalityLevel >= vitalityUpgradeThreshold2)
            return 2;
        else
            return 0;
    }

    public int GetVitalityUpgradeValues(int index) {
        if (index == 1)
            return vitalityHealthUpgradeValue;
        else
            return 0;
    }

    // STAMINA PROPERTIES ////////////////////////////////////////////////////////////////////////////////
    public int CheckStaminaUpgrade(int staminaLevel) {
        if (staminaLevel < staminaUpgradeThreshold1)
            return 0;
        else if (staminaLevel >= staminaUpgradeThreshold1 && staminaLevel < staminaUpgradeThreshold2)
            return 1;
        else if (staminaLevel >= staminaUpgradeThreshold2)
            return 2;
        else
            return 0;
    }
}
