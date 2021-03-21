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
    private int vitalityUpgradeStrong;
    [SerializeField]
    private int vitalityUpgradeMid;
    [SerializeField]
    private int vitalityUpgradeWeak;

    [Header("Efficiency Properties")]
    [SerializeField]
    private float healthRecoveryEfficiency;
    [SerializeField]
    private float energyRecoveryEfficiency;
    [SerializeField]
    private float energyToHealthEfficiency;

    [Header("Strength Properties")]
    [SerializeField]
    private int strengthUpgrade = 10;

    [Header("Stamina Properties")]
    [SerializeField]
    private float staminaRecoveryUpgrade;
    [SerializeField]
    private int staminaMaxUpgradeStrong;
    [SerializeField]
    private int staminaMaxUpgradeWeak;

    [Header("Special Properties")]
    [SerializeField]
    private int specialEnergyUpgradeStrong;
    [SerializeField]
    private int specialEnergyUpgradeWeak;
    [SerializeField]
    private int specialAttackUpgradeStrong;
    [SerializeField]
    private int specialAttackUpgradeWeak;
    [SerializeField]
    private int specialBlasterLightUpgradeStrong;
    [SerializeField]
    private int specialBlasterLightUpgradeWeak;
    [SerializeField]
    private int specialBlasterHeavyUpgradeStrong;
    [SerializeField]
    private int specialBlasterHeavyUpgradeWeak;

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
        upgrades.Add("playerLevel", 0);
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

            upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
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

            upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
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

            upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
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

            upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
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

            upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
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
