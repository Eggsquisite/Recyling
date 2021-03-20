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

    private int baseMaxHealth;
    private int baseMaxEnergy;
    private int baseMaxStamina;

    private float baseHealthRecovery;
    private float baseEnergyRecovery;
    private float baseStaminaRecoveryValue;

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

    [Header("Vitality Properties")]
    [SerializeField]
    private int vitalityUpgradeStrong = 65;
    [SerializeField]
    private int vitalityUpgradeMid = 50;
    [SerializeField]
    private int vitalityUpgradeWeak = 20;

    [Header("Efficiency Properties")]
    [SerializeField]
    private float healthRecoveryEfficiency = 0.125f;
    [SerializeField]
    private float energyRecoveryEfficiency = 0.075f;
    [SerializeField]
    private float energyToHealthEfficiency = 0.05f;

    [Header("Strength Properties")]
    [SerializeField]
    private int strengthUpgrade = 10;

    [Header("Stamina Properties")]
    [SerializeField]
    private float staminaRecoveryUpgrade = 0.15f;
    [SerializeField]
    private int staminaMaxUpgradeStrong = 18;
    [SerializeField]
    private int staminaMaxUpgradeWeak = 3;

    [Header("Special Properties")]
    [SerializeField]
    private int specialEnergyUpgradeStrong = 35;
    [SerializeField]
    private int specialEnergyUpgradeWeak = 5;
    [SerializeField]
    private int specialAttackUpgradeStrong = 20;
    [SerializeField]
    private int specialAttackUpgradeWeak = 15;
    [SerializeField]
    private int specialBlasterLightUpgradeStrong = 3;
    [SerializeField]
    private int specialBlasterLightUpgradeWeak = 2;
    [SerializeField]
    private int specialBlasterHeavyUpgradeStrong = 12;
    [SerializeField]
    private int specialBlasterHeavyUpgradeWeak = 8;

    [Header("Player Stat Levels")]
    private int levelCap;
    private Dictionary<string, int> upgrades;   // health max value
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

        baseMaxHealth = maxHealth;
        baseMaxEnergy = maxEnergy;
        baseMaxStamina = maxStamina;

        baseHealthRecovery = healthRecoveryValue;
        baseEnergyRecovery = energyRecoveryValue;
        baseStaminaRecoveryValue = staminaRecoveryValue;

        baseSwordDmg = swordDamage;
        baseSpecialDmg = specialAttackDmg;
        baseBlasterLightDmg = blasterLightDmg;
        baseBlasterHeavyDmg = blasterHeavyDmg;
    }

    // Start is called before the first frame update
    void Start()
    {
        // load saved upgrade variables

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

        SetDamageVariables();
    }

    public void IncreaseStat(int index) {
        if (index == -1)
        {
            // VITALITY ///////////////////////////////////////////////////////////////////


            // EFFICIENCY /////////////////////////////////////////////////////////////////


            // STRENGTH ///////////////////////////////////////////////////////////////////


            // STAMINA /////////////////////////////////////////////////////////////////////


            // SPECIAL /////////////////////////////////////////////////////////////////////

        }
        else if (index == 0 && upgrades["vitality"] < levelCap) {
            // vitality: max health
            upgrades["vitality"] = upgrades["vitality"] + 1;
            Debug.Log("Vitality Level: " + upgrades["vitality"]);

            if (upgrades["vitality"] <= 10) { 
                maxHealth += vitalityUpgradeStrong;
            } else if (upgrades["vitality"] > 10 && upgrades["vitality"] <= 15) {
                maxHealth += vitalityUpgradeMid;
            } else if (upgrades["vitality"] > 15) {
                maxHealth += vitalityUpgradeWeak;
            }

            UI.SetMaxHealth(maxHealth);
            UI.SetCurrentHealth(maxHealth);
        }
        else if (index == 1 && upgrades["efficiency"] < levelCap) {
            // efficiency: health and energy regen
            upgrades["efficiency"] = upgrades["efficiency"] + 1;
            Debug.Log("efficiency Level: " + upgrades["efficiency"]);

            healthRecoveryValue += healthRecoveryEfficiency;
            energyRecoveryValue += energyRecoveryEfficiency;
            energyToHealthMultiplier = 1 / (1 + upgrades["efficiency"] * energyToHealthEfficiency);

            UI.SetHealthRecoveryValue(healthRecoveryValue);
            UI.SetEnergyRecoveryValue(energyRecoveryValue);
            UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);
        }
        else if (index == 2 && upgrades["strength"] < levelCap) {
            // strength: attack damage
            upgrades["strength"] = upgrades["strength"] + 1;
            Debug.Log("strength Level: " + upgrades["strength"]);

            if (upgrades["strength"] <= 10) { 
                swordDamage += strengthUpgrade;
            } else if (upgrades["strength"] > 10) {
                swordDamage += upgrades["strength"] / 2;
            }

            SetDamageVariables();
        }
        else if (index == 3 && upgrades["stamina"] < levelCap) {
            // stamina: stamina max value/regen
            upgrades["stamina"] = upgrades["stamina"] + 1;
            Debug.Log("stamina Level: " + upgrades["stamina"]);

            staminaRecoveryValue += staminaRecoveryUpgrade; 
            if (upgrades["stamina"] <= 15) {
                maxStamina += staminaMaxUpgradeStrong;
            } else if (upgrades["stamina"] > 15) {
                maxStamina += levelCap % upgrades["stamina"] * 3 + staminaMaxUpgradeWeak;
            }

            UI.SetMaxStamina(maxStamina);
            UI.SetCurrentStamina(maxStamina);
            UI.SetStaminaRecoveryValue(staminaRecoveryValue);
        }
        else if (index == 4 && upgrades["special"] < levelCap) {
            // special: energy max value and special damage (blasters too)
            upgrades["special"] = upgrades["special"] + 1;
            Debug.Log("special Level: " + upgrades["special"]);

            if (upgrades["special"] <= 10) { 
                maxEnergy = baseMaxEnergy + upgrades["special"] * specialEnergyUpgradeStrong;

                specialAttackDmg += specialAttackUpgradeStrong;
                blasterLightDmg += specialBlasterLightUpgradeStrong;
                blasterHeavyDmg += specialBlasterHeavyUpgradeStrong;
            } else if (upgrades["special"] > 10) {
                maxEnergy += levelCap % upgrades["special"] * 3 + specialEnergyUpgradeWeak;

                specialAttackDmg += specialAttackUpgradeWeak;
                blasterLightDmg += specialBlasterLightUpgradeWeak;
                blasterHeavyDmg += specialBlasterHeavyUpgradeWeak;
            }

            SetDamageVariables();
            UI.SetMaxEnergy(maxEnergy);
            UI.SetCurrentEnergy(maxEnergy);
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
