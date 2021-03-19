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
    private float energyToHealthMultiplier;
    [SerializeField]
    private float energyRecoveryValue;
    [SerializeField]
    private float staminaRecoveryValue;
    [SerializeField]
    private float staminaRecoveryDelay;

    private float baseHealthRecovery;
    private float baseEnergyRecovery;
    private float baseStaminaRecoveryValue;
    private float baseStaminaRecoveryDelay;

    [Header("Player Stat Levels")]
    private Dictionary<string, int> upgrades;   // health max value
    private int levelCap;
    // vitality     - health max value
    // efficiency   - health recovery and energy regen
    // strength     - attack damage
    // stamina      - stamina max value/regen
    // special      - energy max value and special attack dmg

    private void Awake()
    {
        levelCap = 20;
        if (UI == null) UI = GetComponent<PlayerUI>();
        upgrades = new Dictionary<string, int>();
        upgrades.Add("vitality", 1);
        upgrades.Add("efficiency", 1);
        upgrades.Add("strength", 1);
        upgrades.Add("stamina", 1);
        upgrades.Add("special", 1);

        baseHealthRecovery = healthRecoveryValue;
        baseEnergyRecovery = energyRecoveryValue;
        baseStaminaRecoveryValue = staminaRecoveryValue;
        baseStaminaRecoveryDelay = staminaRecoveryDelay;
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

        UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);

        //UI.SetHealthRecoveryDelay(healthRecoveryDelay);
        //UI.SetEnergyRecoveryDelay(energyRecoveryDelay);
        UI.SetStaminaRecoveryDelay(staminaRecoveryDelay);

        UpdateMaxValues(-1);
    }

    public void IncreaseStat(int index) {
        if (index == 0 && upgrades["vitality"] < levelCap) {
            upgrades["vitality"] = upgrades["vitality"] + 1;
            Debug.Log("Vitality Level: " + upgrades["vitality"]);
        }
        else if (index == 1 && upgrades["efficiency"] < levelCap) {
            upgrades["efficiency"] = upgrades["efficiency"] + 1;
            Debug.Log("efficiency Level: " + upgrades["efficiency"]);
        }
        else if (index == 2 && upgrades["strength"] < levelCap) {
            upgrades["strength"] = upgrades["strength"] + 1;
            Debug.Log("strength Level: " + upgrades["strength"]);
        }
        else if (index == 3 && upgrades["stamina"] < levelCap) {
            upgrades["stamina"] = upgrades["stamina"] + 1;
            Debug.Log("stamina Level: " + upgrades["stamina"]);
        }
        else if (index == 4 && upgrades["special"] < levelCap) { 
            upgrades["special"] = upgrades["special"] + 1;
            Debug.Log("special Level: " + upgrades["special"]);
        }

        UpdateMaxValues(index);
    }

    private void UpdateMaxValues(int index) { 
        if (index == -1)
        {
            // do all
        }
        if (index == 0) 
        {
            maxHealth += upgrades["vitality"] * 50;
            UI.SetMaxHealth(maxHealth);
            UI.SetCurrentHealth(maxHealth);
        } else if (index == 1) // efficiency: health and energy regen
        {
            energyToHealthMultiplier = 1 / (1 + upgrades["efficiency"] * 0.05f);
            healthRecoveryValue = baseHealthRecovery + upgrades["efficiency"] * 0.125f;
            energyRecoveryValue = baseEnergyRecovery + upgrades["efficiency"] * 0.075f;

            UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);
            UI.SetEnergyRecoveryValue(energyRecoveryValue);
            UI.SetHealthRecoveryValue(healthRecoveryValue);
        }
    }

    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }

    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
}
