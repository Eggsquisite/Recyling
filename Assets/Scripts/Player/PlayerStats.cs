using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Components")]
    private PlayerUI UI;
    private Player player;
    private PlayerUpgrades playerUpgrades;

    [Header("Player Stats")]
    [SerializeField]
    private int levelCap;
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int maxEnergy;
    [SerializeField]
    private int maxStamina;
    [SerializeField]
    private float healthRecoveryValue;
    [SerializeField] [Tooltip("Ratio of energy consumption when healing. Lower number means higher healing efficiency")]
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
    private float swordDamage;
    [SerializeField]
    private float specialAttackDmg;
    [SerializeField]
    private float blasterLightDmg;
    [SerializeField]
    private float blasterHeavyDmg;

    private float baseSwordDmg;
    private float baseSpecialDmg;
    private float baseBlasterLightDmg;
    private float baseBlasterHeavyDmg;

    [Header("Vitality Properties")]
    [SerializeField]
    private int vitalityUpgradeStrong;
    [SerializeField]
    private int vitalityUpgradeMid;
    [SerializeField]
    private int vitalityUpgradeWeak;

    [Header("Focus Properties")]
    [SerializeField] [Tooltip ("Amount of health recovered when healing")]
    private float healthFocusUpgrade;
    [SerializeField] [Tooltip ("Amount of energy gained when basic attacking an enemy")]
    private float energyFocusUpgrade;
    [SerializeField] [Tooltip ("Efficiency at which energy is consumed to recover a set amount of health")]
    private float energyToHealthFocusUpgrade;

    [Header("Strength Properties")]
    [SerializeField]
    private float strengthUpgrade = 10;
    [SerializeField]
    private float energyFocusUpgradeWeak;

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
    private float specialAttackUpgradeStrong;
    [SerializeField]
    private float specialAttackUpgradeWeak;
    [SerializeField]
    private float specialBlasterLightUpgradeStrong;
    [SerializeField]
    private float specialBlasterLightUpgradeWeak;
    [SerializeField]
    private float specialBlasterHeavyUpgradeStrong;
    [SerializeField]
    private float specialBlasterHeavyUpgradeWeak;

    [Header("Player Stat Levels")]
    private Dictionary<string, int> upgrades = new Dictionary<string, int>();
    // vitality     - health max value
    // focus        - health recovery and energy regen
    // strength     - attack damage/speed
    // stamina      - stamina max value/regen
    // special      - energy max value and special attack dmg

    private void Awake()
    {
        if (UI == null) UI = GetComponent<PlayerUI>();
        if (player == null) player = GetComponent<Player>();
        if (playerUpgrades == null) playerUpgrades = GetComponent<PlayerUpgrades>();

        upgrades.Add("playerLevel", 1);
        upgrades.Add("vitality", 1);
        upgrades.Add("focus", 1);
        upgrades.Add("strength", 1);
        upgrades.Add("stamina", 1);
        upgrades.Add("special", 1);

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

    public void RefreshResources()
    {
        UI.SetCurrentHealth(maxHealth);
        UI.SetCurrentEnergy(maxEnergy);
        UI.SetCurrentStamina(maxStamina);
    }

    public void IncreaseStat(int index, int newLevel) {
        if (index == -1) // FOR LOADING
        {
            // VITALITY ///////////////////////////////////////////////////////////////////
            // affects health growth
            for (int i = upgrades["vitality"]; i < SaveManager.instance.activeSave.playerVitalityLevel; i++)
            {
                upgrades["vitality"] = upgrades["vitality"] + 1;
                //Debug.Log("Vitality Level: " + upgrades["vitality"]);

                if (upgrades["vitality"] <= 4) { 
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeStrong;
                } else if (upgrades["vitality"] > 4 && upgrades["vitality"] <= 7) {
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeMid;
                } else if (upgrades["vitality"] > 7) {
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeWeak;
                }

                UI.SetMaxHealth(maxHealth);
                UI.SetCurrentHealth(maxHealth);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
            }

            // FOCUS ///////////////////////////////////////////////////////////////////////
            // affects recovery efficiency 
            for (int i = upgrades["focus"]; i < SaveManager.instance.activeSave.playerFocusLevel; i++)
            {
                upgrades["focus"] = upgrades["focus"] + 1;
                //Debug.Log("focus Level: " + upgrades["focus"]);

                healthRecoveryValue += healthFocusUpgrade;
                energyRecoveryValue += energyFocusUpgrade;
                // energyToHealth affects the ratio of energy consumed when healing
                energyToHealthMultiplier = 1 / (1 + upgrades["focus"] * energyToHealthFocusUpgrade);

                UI.SetHealthRecoveryValue(healthRecoveryValue);
                UI.SetEnergyRecoveryValue(energyRecoveryValue);
                UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
            }

            // STRENGTH ///////////////////////////////////////////////////////////////////
            // affects player M1 damage and slightly increases energy recovery
            for (int i = upgrades["strength"]; i < SaveManager.instance.activeSave.playerStrengthLevel; i++)
            {
                upgrades["strength"] = upgrades["strength"] + 1;
                //Debug.Log("strength Level: " + upgrades["strength"]);

                swordDamage += strengthUpgrade;
                energyRecoveryValue += energyFocusUpgradeWeak;
                UI.SetEnergyRecoveryValue(energyRecoveryValue);

                SetDamageVariables();
                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
            }

            // STAMINA /////////////////////////////////////////////////////////////////////
            // affects stamina growth and recovery speed
            for (int i = upgrades["stamina"]; i < SaveManager.instance.activeSave.playerStaminaLevel; i++)
            {
                upgrades["stamina"] = upgrades["stamina"] + 1;
                //Debug.Log("stamina Level: " + upgrades["stamina"]);

                staminaRecoveryValue += staminaRecoveryUpgrade; 
                if (upgrades["stamina"] <= 7) {
                    maxStamina += staminaMaxUpgradeStrong;
                } else if (upgrades["stamina"] > 7) {
                    maxStamina += levelCap % upgrades["stamina"] * 3 + staminaMaxUpgradeWeak;
                }

                UI.SetMaxStamina(maxStamina);
                UI.SetCurrentStamina(maxStamina);
                UI.SetStaminaRecoveryValue(staminaRecoveryValue);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
            }

            // SPECIAL /////////////////////////////////////////////////////////////////////
            // affects special damage (blaster attacks, M2)
            for (int i = upgrades["special"]; i < SaveManager.instance.activeSave.playerSpecialLevel; i++)
            {
                upgrades["special"] = upgrades["special"] + 1;
                //Debug.Log("special Level: " + upgrades["special"]);

                if (upgrades["special"] <= 7) { 
                    maxEnergy = baseMaxEnergy + upgrades["special"] * specialEnergyUpgradeStrong;

                    specialAttackDmg += specialAttackUpgradeStrong;
                    blasterLightDmg += specialBlasterLightUpgradeStrong;
                    blasterHeavyDmg += specialBlasterHeavyUpgradeStrong;
                } else if (upgrades["special"] > 7) {
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

            player.UpdatePlayerUpgrades();
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        else if (index == 0 && upgrades["vitality"] < levelCap) {
            // vitality: max health
            for (int i = upgrades["vitality"]; i < newLevel; i++)
            {
                upgrades["vitality"] = upgrades["vitality"] + 1;
                //Debug.Log("Vitality Level: " + upgrades["vitality"]);

                if (upgrades["vitality"] <= 4) { 
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeStrong;
                } else if (upgrades["vitality"] > 4 && upgrades["vitality"] <= 7) {
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeMid;
                } else if (upgrades["vitality"] > 7) {
                    maxHealth = UI.GetMaxHealth() + vitalityUpgradeWeak;
                }

                UI.SetMaxHealth(maxHealth);
                UI.SetCurrentHealth(maxHealth);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;

                player.UpdatePlayerUpgrades();
            }
        }
        else if (index == 1 && upgrades["focus"] < levelCap) {
            // focus: health and energy regen
            for (int i = upgrades["focus"]; i < newLevel; i++)
            {
                upgrades["focus"] = upgrades["focus"] + 1;
                Debug.Log("focus Level: " + upgrades["focus"]);

                healthRecoveryValue += healthFocusUpgrade;
                energyRecoveryValue += energyFocusUpgrade;
                energyToHealthMultiplier = 1 / (1 + upgrades["focus"] * energyToHealthFocusUpgrade);

                UI.SetHealthRecoveryValue(healthRecoveryValue);
                UI.SetEnergyRecoveryValue(energyRecoveryValue);
                // affects ratio of energy consumption when healing. Lower number is better efficiency
                UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;

                player.UpdatePlayerUpgrades();
            }
        }
        else if (index == 2 && upgrades["strength"] < levelCap) {
            // strength: attack damage
            for (int i = upgrades["strength"]; i < newLevel; i++)
            {
                upgrades["strength"] = upgrades["strength"] + 1;
                Debug.Log("strength Level: " + upgrades["strength"]);

                swordDamage += strengthUpgrade;
                energyRecoveryValue += energyFocusUpgradeWeak;
                UI.SetEnergyRecoveryValue(energyRecoveryValue);

                SetDamageVariables();
                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;

                player.UpdatePlayerUpgrades();
            }
        }
        else if (index == 3 && upgrades["stamina"] < levelCap) {
            // stamina: stamina max value/regen
            for (int i = upgrades["stamina"]; i < newLevel; i++)
            {
                upgrades["stamina"] = upgrades["stamina"] + 1;
                Debug.Log("stamina Level: " + upgrades["stamina"]);

                staminaRecoveryValue += staminaRecoveryUpgrade; 
                if (upgrades["stamina"] <= 7) {
                    maxStamina += staminaMaxUpgradeStrong;
                } else if (upgrades["stamina"] > 7) {
                    maxStamina += staminaMaxUpgradeWeak;
                }

                UI.SetMaxStamina(maxStamina);
                UI.SetCurrentStamina(maxStamina);
                UI.SetStaminaRecoveryValue(staminaRecoveryValue);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;

                player.UpdatePlayerUpgrades();
            }
        }
        else if (index == 4 && upgrades["special"] < levelCap) {
            // special: energy max value and special damage (blasters too)
            for (int i = upgrades["special"]; i < newLevel; i++)
            {
                upgrades["special"] = upgrades["special"] + 1;
                Debug.Log("special Level: " + upgrades["special"]);

                if (upgrades["special"] <= 7) { 
                    maxEnergy = baseMaxEnergy + upgrades["special"] * specialEnergyUpgradeStrong;

                    specialAttackDmg += specialAttackUpgradeStrong;
                    blasterLightDmg += specialBlasterLightUpgradeStrong;
                    blasterHeavyDmg += specialBlasterHeavyUpgradeStrong;
                } else if (upgrades["special"] > 7) {
                    maxEnergy += levelCap % upgrades["special"] * 3 + specialEnergyUpgradeWeak;

                    specialAttackDmg += specialAttackUpgradeWeak;
                    blasterLightDmg += specialBlasterLightUpgradeWeak;
                    blasterHeavyDmg += specialBlasterHeavyUpgradeWeak;
                }

                SetDamageVariables();
                UI.SetMaxEnergy(maxEnergy);
                UI.SetCurrentEnergy(maxEnergy);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;

                player.UpdatePlayerUpgrades();
            }
        }
    }

    public void SetMaxHealth(int newValue) {
        maxHealth = newValue;
        UI.SetMaxHealth(maxHealth);
        UI.SetCurrentHealth(maxHealth);
    }

    public void SetStaminaRecoveryDelay(float newValue) {
        staminaRecoveryDelay = newValue;
        UI.SetStaminaRecoveryDelay(staminaRecoveryDelay);
    }

    private void SetDamageVariables()
    {
        player.SetSwordDamage(swordDamage);
        player.SetSpecialAttackDmg(specialAttackDmg);
        player.SetBlasterLightDmg(blasterLightDmg);
        player.SetBlasterHeavyDmg(blasterHeavyDmg);
    }

    public void CheckUpgradeLevels(out int strengthUpgradeLevel, 
                                    out int specialUpgradeLevel,
                                    out int focusUpgradeLevel,
                                    out int vitalityUpgradeLevel,
                                    out int staminaUpgradeLevel) {
        strengthUpgradeLevel = playerUpgrades.CheckUpgradeLevels(upgrades["strength"]);
        specialUpgradeLevel = playerUpgrades.CheckUpgradeLevels(upgrades["special"]);
        focusUpgradeLevel = playerUpgrades.CheckUpgradeLevels(upgrades["focus"]);
        vitalityUpgradeLevel = playerUpgrades.CheckUpgradeLevels(upgrades["vitality"]);
        staminaUpgradeLevel = playerUpgrades.CheckUpgradeLevels(upgrades["stamina"]);

        /*        strengthUpgradeLevel = playerUpgrades.CheckStrengthUpgrade(upgrades["strength"]);
                specialUpgradeLevel = playerUpgrades.CheckSpecialUpgrade(upgrades["special"]);
                focusUpgradeLevel = playerUpgrades.CheckFocusUpgrade(upgrades["focus"]);
                vitalityUpgradeLevel = playerUpgrades.CheckVitalityUpgrade(upgrades["vitality"]);
                staminaUpgradeLevel = playerUpgrades.CheckStaminaUpgrade(upgrades["stamina"]);*/
    }

    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }

    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }

    /// GET CURRENT/UPGRADE LEVELS
    /// 
    public int GetLevelCap() {
        return levelCap;
    }
    public int GetPlayerLevel() {
        return upgrades["playerLevel"];
    }
    public int GetVitalityLevel() {
        return upgrades["vitality"];
    }
    public int GetFocusLevel() {
        return upgrades["focus"];
    }
    public int GetStrengthLevel() {
        return upgrades["strength"];
    }
    public int GetStaminaLevel() {
        return upgrades["stamina"];
    }
    public int GetSpecialLevel() {
        return upgrades["special"];
    }
}
