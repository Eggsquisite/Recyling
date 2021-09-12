using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    [Header("Strength Upgrade Properties")]
    [SerializeField]
    private int strengthUpgrade1;
    [SerializeField]
    private int strengthUpgrade2;
    [SerializeField] [Tooltip("Faster attack speed multiplier")]
    private float strengthUpgradeValue1;
    [SerializeField] [Tooltip("Player damages surrounding enemies when landing from jetpack")]
    private float strengthUpgradeValue2;

    [Header("Special Upgrade Properties")]
    [SerializeField]
    private int specialUpgrade1;
    [SerializeField]
    private int specialUpgrade2;

    [Header("Focus Upgrade Properties")]
    [SerializeField]
    private int focusUpgrade1;
    [SerializeField]
    private int focusUpgrade2;

    [Header("Vitality Upgrade Properties")]
    [SerializeField]
    private int vitalityUpgrade1;
    [SerializeField]
    private int vitalityUpgrade2;

    [Header("Stamina Upgrade Properties")]
    [SerializeField]
    private int staminaUpgrade1;
    [SerializeField]
    private int staminaUpgrade2;

    private int strengthUpgradeLevel;
    private int specialUpgradeLevel;
    private int focusUpgradeLevel;
    private int vitalityUpgradeLevel;
    private int staminaUpgradeLevel;

    public int CheckStrengthUpgrade(int strengthLevel) {
        if (strengthLevel < strengthUpgrade1)
            return 0;
        else if (strengthLevel >= strengthUpgrade1 && strengthLevel < strengthUpgrade2)
            return 1;
        else if (strengthLevel >= strengthUpgrade2)
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
}
