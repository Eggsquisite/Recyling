using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("Strength Upgrade Properties")]
    [SerializeField]
    private int strengthUpgradeThreshold1;
    [SerializeField]
    private int strengthUpgradeThreshold2;
    [SerializeField] [Tooltip("Faster attack speed multiplier")]
    private float strengthUpgradeValue1;
    [SerializeField] [Tooltip("Player damages surrounding enemies when landing from jetpack")]
    private float strengthUpgradeValue2;

    [Header("Special Upgrade Properties")]
    [SerializeField]
    private int specialUpgradeThreshold1;
    [SerializeField]
    private int specialUpgradeThreshold2;

    [Header("Focus Upgrade Properties")]
    [SerializeField]
    private int focusUpgradeThreshold1;
    [SerializeField]
    private int focusUpgradeThreshold2;

    [Header("Vitality Upgrade Properties")]
    [SerializeField]
    private int vitalityUpgradeThreshold1;
    [SerializeField]
    private int vitalityUpgradeThreshold2;

    [Header("Stamina Upgrade Properties")]
    [SerializeField]
    private int staminaUpgradeThreshold1;
    [SerializeField]
    private int staminaUpgradeThreshold2;

    private int strengthUpgradeLevel;
    private int specialUpgradeLevel;
    private int focusUpgradeLevel;
    private int vitalityUpgradeLevel;
    private int staminaUpgradeLevel;

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
