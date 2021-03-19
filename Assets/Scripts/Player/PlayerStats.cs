using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Components")]
    private PlayerUI UI;
    private Player player;

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

    [Header("Attack Properties")]
    [SerializeField]
    private int swordDamage;
    [SerializeField]
    private int specialAttackDmg;
    [SerializeField]
    private int blasterLightDmg;
    [SerializeField]
    private int blasterHeavyDmg;

    private int baseSwordDmg;
    private int baseSpecialDmg;
    private int baseBlasterLightDmg;
    private int baseBlasterHeavyDmg;

    private int strengthMultiplier = 10;
    private int blasterMultiplierWeak = 5;
    private int blasterMultiplierStrong = 5;

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
        if (player == null) player = GetComponent<Player>();
        upgrades = new Dictionary<string, int>();
        upgrades.Add("vitality", 0);
        upgrades.Add("efficiency", 0);
        upgrades.Add("strength", 0);
        upgrades.Add("stamina", 0);
        upgrades.Add("special", 0);

        baseHealthRecovery = healthRecoveryValue;
        baseEnergyRecovery = energyRecoveryValue;
        baseStaminaRecoveryValue = staminaRecoveryValue;
        baseStaminaRecoveryDelay = staminaRecoveryDelay;

        baseSwordDmg = swordDamage;
        baseSpecialDmg = specialAttackDmg;
        baseBlasterLightDmg = blasterLightDmg;
        baseBlasterHeavyDmg = blasterHeavyDmg;
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

        //IncreaseStat(-1);
        SetDamageVariables();
    }

    public void IncreaseStat(int index) {
        if (index == -1)
        {
            // VITALITY ///////////////////////////////////////////////////////////////////
            maxHealth += upgrades["vitality"] * 50;
            UI.SetMaxHealth(maxHealth);
            UI.SetCurrentHealth(maxHealth);

            // EFFICIENCY /////////////////////////////////////////////////////////////////
            energyToHealthMultiplier = 1 / (1 + upgrades["efficiency"] * 0.05f);
            healthRecoveryValue = baseHealthRecovery + upgrades["efficiency"] * 0.125f;
            energyRecoveryValue = baseEnergyRecovery + upgrades["efficiency"] * 0.075f;

            UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);
            UI.SetEnergyRecoveryValue(energyRecoveryValue);
            UI.SetHealthRecoveryValue(healthRecoveryValue);

            // STRENGTH ///////////////////////////////////////////////////////////////////
            if (upgrades["strength"] <= 10) { 
                swordDamage = baseSwordDmg + upgrades["strength"] * strengthMultiplier;
            } else if (upgrades["strength"] > 10) {
                swordDamage += upgrades["strength"] / 2;
            }
            SetDamageVariables();

            // STAMINA /////////////////////////////////////////////////////////////////////


            // SPECIAL /////////////////////////////////////////////////////////////////////

        }
        else if (index == 0 && upgrades["vitality"] < levelCap) {
            // vitality: max health
            upgrades["vitality"] = upgrades["vitality"] + 1;

            maxHealth += upgrades["vitality"] * 50;
            UI.SetMaxHealth(maxHealth);
            UI.SetCurrentHealth(maxHealth);

            Debug.Log("Vitality Level: " + upgrades["vitality"]);
        }
        else if (index == 1 && upgrades["efficiency"] < levelCap) {
            // efficiency: health and energy regen
            upgrades["efficiency"] = upgrades["efficiency"] + 1;
            Debug.Log("efficiency Level: " + upgrades["efficiency"]);

            energyToHealthMultiplier = 1 / (1 + upgrades["efficiency"] * 0.05f);
            healthRecoveryValue = baseHealthRecovery + upgrades["efficiency"] * 0.125f;
            energyRecoveryValue = baseEnergyRecovery + upgrades["efficiency"] * 0.075f;

            UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);
            UI.SetEnergyRecoveryValue(energyRecoveryValue);
            UI.SetHealthRecoveryValue(healthRecoveryValue);
        }
        else if (index == 2 && upgrades["strength"] < levelCap) {
            // strength: attack damage
            upgrades["strength"] = upgrades["strength"] + 1;
            Debug.Log("strength Level: " + upgrades["strength"]);

            if (upgrades["strength"] <= 10) { 
                swordDamage = baseSwordDmg + upgrades["strength"] * strengthMultiplier;
            } else if (upgrades["strength"] > 10) {
                swordDamage += upgrades["strength"] / 2;
            }
            SetDamageVariables();
        }
        else if (index == 3 && upgrades["stamina"] < levelCap) {
            // stamina: stamina max value/regen
            upgrades["stamina"] = upgrades["stamina"] + 1;
            Debug.Log("stamina Level: " + upgrades["stamina"]);


        }
        else if (index == 4 && upgrades["special"] < levelCap) {
            // special: energy max value and special damage (blasters too)
            upgrades["special"] = upgrades["special"] + 1;
            Debug.Log("special Level: " + upgrades["special"]);


        }
    }

    private void SetDamageVariables()
    {
        player.SetSwordDamage(swordDamage);
        player.SetSpecialAttackDmg(specialAttackDmg);
        player.SetBlasterLightDmg(blasterLightDmg);
        player.SetBlasterHeavyDmg(blasterHeavyDmg);
    }

    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }

    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
}
