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
    [SerializeField]
    private float healthFocusUpgrade;
    [SerializeField]
    private float energyFocusUpgrade;
    [SerializeField]
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
    private int levelCap;
    private Dictionary<string, int> upgrades;   // health max value
    // vitality     - health max value
    // focus   - health recovery and energy regen
    // strength     - attack damage
    // stamina      - stamina max value/regen
    // special      - energy max value and special attack dmg

    private void Awake()
    {
        levelCap = 20;
        if (UI == null) UI = GetComponent<PlayerUI>();
        if (player == null) player = GetComponent<Player>();
        upgrades = new Dictionary<string, int>();
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

            // FOCUS ///////////////////////////////////////////////////////////////////////
            // affects recovery efficiency 
            for (int i = upgrades["focus"]; i < SaveManager.instance.activeSave.playerFocusLevel; i++)
            {
                upgrades["focus"] = upgrades["focus"] + 1;
                //Debug.Log("focus Level: " + upgrades["focus"]);

                healthRecoveryValue += healthFocusUpgrade;
                energyRecoveryValue += energyFocusUpgrade;
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

                if (upgrades["strength"] <= 10) { 
                    swordDamage += strengthUpgrade;
                } else if (upgrades["strength"] > 10) {
                    swordDamage += upgrades["strength"] / 2;
                }

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

            // SPECIAL /////////////////////////////////////////////////////////////////////
            // affects special damage (blaster attacks, M2)
            for (int i = upgrades["special"]; i < SaveManager.instance.activeSave.playerSpecialLevel; i++)
            {
                upgrades["special"] = upgrades["special"] + 1;
                //Debug.Log("special Level: " + upgrades["special"]);

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
        else if (index == 0 && upgrades["vitality"] < levelCap) {
            // vitality: max health
            for (int i = upgrades["vitality"]; i < newLevel; i++)
            {
                upgrades["vitality"] = upgrades["vitality"] + 1;
                //Debug.Log("Vitality Level: " + upgrades["vitality"]);

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
                UI.SetEnergyToHealthMultiplier(energyToHealthMultiplier);

                upgrades["playerLevel"] = upgrades["playerLevel"] + 1;
            }
        }
        else if (index == 2 && upgrades["strength"] < levelCap) {
            // strength: attack damage
            for (int i = upgrades["strength"]; i < newLevel; i++)
            {
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
        }
        else if (index == 3 && upgrades["stamina"] < levelCap) {
            // stamina: stamina max value/regen
            for (int i = upgrades["stamina"]; i < newLevel; i++)
            {
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
        }
        else if (index == 4 && upgrades["special"] < levelCap) {
            // special: energy max value and special damage (blasters too)
            for (int i = upgrades["special"]; i < newLevel; i++)
            {
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
