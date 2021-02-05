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
    private int healthRecoveryValue;
    [SerializeField]
    private float healthRecoveryDelay;
    [SerializeField]
    private int energyRecoveryValue;
    [SerializeField]
    private float energyRecoveryDelay;
    [SerializeField]
    private int staminaRecoveryValue;
    [SerializeField]
    private float staminaRecoveryDelay;

    // Start is called before the first frame update
    void Start()
    {
        if (UI == null) UI = GetComponent<PlayerUI>();
        InitializeUI();
    }

    private void InitializeUI()
    {
        UI.SetCurrentHealth(maxHealth);
        UI.SetCurrentEnergy(maxEnergy);
        UI.SetCurrentStamina(maxStamina);

        UI.SetHealthRecoveryRate(healthRecoveryValue);
        UI.SetEnergyRecoveryRate(energyRecoveryValue);
        UI.SetStaminaRecoveryRate(staminaRecoveryValue);

        UI.SetHealthRecoveryDelay(healthRecoveryDelay);
        UI.SetEnergyRecoveryDelay(energyRecoveryDelay);
        UI.SetStaminaRecoveryDelay(staminaRecoveryDelay);
    }

    //  HEALTH ////////////////////////////////////////////////////

    public int GetCurrentHealth() {
        return UI.GetCurrentHealth();
    }

    public void SetCurrentHealth(int damage) {
        UI.SetCurrentHealth(damage);
    }

    //  ENERGY /////////////////////////////////////////////////

    public int GetCurrentEnergy() {
        return UI.GetCurrentEnergy();
    }
    public void SetCurrentEnergy(int newEnergy) {
        UI.SetCurrentEnergy(newEnergy);
    }

    //  STAMINA //////////////////////////////////////////////////

    public int GetCurrentStamina() {
        return UI.GetCurrentStamina();
    }
    public void SetCurrentStamina(int newStamina) {
        UI.SetCurrentStamina(newStamina);
    }
}