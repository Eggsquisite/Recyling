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
    private float energyRecoveryValue;
    [SerializeField]
    private float staminaRecoveryValue;
    [SerializeField]
    private float staminaRecoveryDelay;

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

        //UI.SetHealthRecoveryDelay(healthRecoveryDelay);
        //UI.SetEnergyRecoveryDelay(energyRecoveryDelay);
        UI.SetStaminaRecoveryDelay(staminaRecoveryDelay);

        UpdateMaxValues(-1);
    }

    public void IncreaseStat(int index) {
        if (index == 0 && upgrades["vitality"] < levelCap)
            upgrades["vitality"] = upgrades["vitality"] + 1;
        else if (index == 1)
            upgrades["efficiency"] = upgrades["efficiency"] + 1;
        else if (index == 2)
            upgrades["strength"] = upgrades["strength"] + 1;
        else if (index == 3)
            upgrades["stamina"] = upgrades["stamina"] + 1;
        else if (index == 4)
            upgrades["special"] = upgrades["special"] + 1;

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
        } else if (index == 1) // efficiency: health and energy regen
        {
            UI.SetHealthRecoveryValue(healthRecoveryValue + upgrades["efficiency"] * 1.25f);
            UI.SetEnergyRecoveryValue(energyRecoveryValue + upgrades["efficiency"] * 1.15f);
        }

    }

    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }

    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
}
