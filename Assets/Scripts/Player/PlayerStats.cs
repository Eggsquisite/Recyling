using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerUI UI;

    [Header("Player Stats")]
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int maxEnergy;
    [SerializeField]
    private int maxStamina;
    [SerializeField]
    private float healthRecoveryValue;
    [SerializeField]
    private float healthRecoveryDelay;
    [SerializeField]
    private float energyRecoveryValue;
    [SerializeField]
    private float energyRecoveryDelay;
    [SerializeField]
    private float staminaRecoveryValue;
    [SerializeField]
    private float staminaRecoveryDelay;

    [Header("Player Stat Levels")]
    private Dictionary<string, int> upgrades;   // health max value
    private int efficiency; // health recovery and energy regen
    private int strength;   // attack damage
    private int stamina;    // stamina max value and regen
    private int special;    // energy max value and special damage
    private int levelMax;

    private void Awake()
    {
        levelMax = 20;
        if (UI == null) UI = GetComponent<PlayerUI>();
        upgrades = new Dictionary<string, int>();
        upgrades.Add("vitality", 1);
        upgrades.Add("efficiency", 1);
        upgrades.Add("strength", 1);
        upgrades.Add("stamina", 1);
        upgrades.Add("special", 1);
    }

    // Start is called before the first frame update
    void Start()
    {

        UI.SetMaxHealth(maxHealth);
        UI.SetMaxEnergy(maxEnergy);
        UI.SetMaxStamina(maxStamina);

        UI.SetCurrentHealth(maxHealth);
        UI.SetCurrentEnergy(maxEnergy);
        UI.SetCurrentStamina(maxStamina);

        UI.SetHealthRecoveryValue(healthRecoveryValue);
        UI.SetEnergyRecoveryValue(energyRecoveryValue);
        UI.SetStaminaRecoveryValue(staminaRecoveryValue);

        UI.SetHealthRecoveryDelay(healthRecoveryDelay);
        UI.SetEnergyRecoveryDelay(energyRecoveryDelay);
        UI.SetStaminaRecoveryDelay(staminaRecoveryDelay);

        UpdateMaxValues(-1);
    }

    public void IncreaseStat(int index) {
        if (index == 0 && upgrades["vitality"] < levelMax)
            upgrades["vitality"] = upgrades["vitality"] + 1;
        else if (index == 1)
            efficiency += 1;
        else if (index == 2)
            strength += 1;
        else if (index == 3)
            stamina += 1;
        else if (index == 4)
            special += 1;

        Debug.Log(upgrades["vitality"]);
        UpdateMaxValues(index);
    }

    private void UpdateMaxValues(int index) { 
        if (index == -1)
        {
            // do all
        }
        if (index == 0) 
        {
            int tmp = maxHealth + upgrades["vitality"] * 50;
            UI.SetMaxHealth(tmp);
            UI.SetCurrentHealth(tmp);
        }

    }

    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }
}
