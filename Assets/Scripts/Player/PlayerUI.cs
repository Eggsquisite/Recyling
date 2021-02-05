using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private float visualDelay;
    [SerializeField]
    private float decaySpeed;
    [SerializeField]
    private float recoverySpeed;

    [Header("Health")]
    [SerializeField]
    private Slider healthMaxValue;
    [SerializeField]
    private Slider healthCurrentValue;
    [SerializeField]
    private Slider healthDestroyedValue;

    private bool healthLost;
    private bool healthDecaying;
    private bool healthRecovering;
    private int healthRecoveryRate;
    private float healthTimer;
    private float healthRecoveryDelay;

    [Header("Energy")]
    [SerializeField]
    private Slider energyMaxValue;
    [SerializeField]
    private Slider energyCurrentValue;
    [SerializeField]
    private Slider energyDestroyedValue;

    private bool energyLost;
    private bool energyDecaying;
    private bool energyRecovering;
    private int energyRecoveryRate;
    private float energyTimer;
    private float energyRecoveryDelay;

    [Header("Stamina")]
    [SerializeField]
    private Slider staminaMaxValue;
    [SerializeField]
    private Slider staminaCurrentValue;
    [SerializeField]
    private Slider staminaDestroyedValue;

    private bool staminaLost;
    private bool staminaDecaying;
    private bool staminaRecovering;
    private int staminaRecoveryRate;
    private float staminaTimer;
    private float staminaRecoveryDelay;

    // Start is called before the first frame update
    void Start()
    {
        healthDestroyedValue.value = healthDestroyedValue.maxValue = healthCurrentValue.maxValue;
        energyDestroyedValue.value = energyDestroyedValue.maxValue = energyCurrentValue.maxValue;
        staminaDestroyedValue.value = staminaDestroyedValue.maxValue = staminaCurrentValue.maxValue;
    }

    // Update is called once per frame
    void Update()
    {
        BarUpdate();
        //BarRecovery();
    }

    private void BarUpdate() {
        if (healthLost)
            DestroyedTimer(ref healthTimer, ref healthLost, ref healthDecaying);
        else if (healthDecaying)
            VisualDecay(healthCurrentValue, healthDestroyedValue, ref healthDecaying, 1);

        if (energyLost)
            DestroyedTimer(ref energyTimer, ref energyLost, ref energyDecaying);
        else if (energyDecaying)
            VisualDecay(energyCurrentValue, energyDestroyedValue, ref energyDecaying, 2);

        if (staminaLost)
            DestroyedTimer(ref staminaTimer, ref staminaLost, ref staminaDecaying);
        else if (staminaDecaying)
            VisualDecay(staminaCurrentValue, staminaDestroyedValue, ref staminaDecaying, 3);
    }

    private void BarRecovery() {
        if (!healthDecaying && !healthLost && !healthRecovering)
            StartCoroutine(HealthRecovery());
        if (!energyDecaying && !energyLost && !energyRecovering)
            StartCoroutine(EnergyRecovery());
        if (!staminaDecaying && !staminaLost && !staminaRecovering)
            StartCoroutine(StaminaRecovery());
    }

    private void DestroyedTimer(ref float timer, ref bool flag, ref bool decayFlag) {
        if (timer < visualDelay)
            timer += Time.deltaTime;
        else if (timer >= visualDelay) {
            timer = 0f;
            flag = false;
            decayFlag = true;
        }
    }

    private void VisualDecay(Slider currentValue, Slider decayValue, ref bool decayFlag, int index) {
        if (decayValue.value > currentValue.value) {
            decayValue.value -= decaySpeed * Time.deltaTime;
        } else if (decayValue.value <= currentValue.value) {
            decayValue.value = currentValue.value;
            decayFlag = false;

            if (index == 1)
                StartCoroutine(HealthRecovery());
            else if (index == 2)
                StartCoroutine(EnergyRecovery());
            else if (index == 3)
                StartCoroutine(StaminaRecovery());
        }
    }

    public void SetMaxHealth(int newValue) {
        healthMaxValue.value = newValue;
        healthCurrentValue.maxValue = healthCurrentValue.value = newValue;
        healthDestroyedValue.maxValue = healthDestroyedValue.value = newValue;
    }

    public void SetCurrentHealth(int newValue) {
        if (healthLost)
            healthTimer = 0f;

        // Reset timer and visual decay if decaying
        if (newValue < healthCurrentValue.value) { 
            healthLost = true;
            healthDecaying = false;
            healthRecovering = false;
        }

        healthCurrentValue.value += newValue;
    }

    public int GetCurrentHealth() {
        return Mathf.RoundToInt(healthCurrentValue.value);
    }

    public void SetMaxEnergy(int newValue) {
        energyMaxValue.value = newValue;
        energyCurrentValue.maxValue = energyCurrentValue.value = newValue;
        energyDestroyedValue.maxValue = energyDestroyedValue.value = newValue;
    }

    public void SetCurrentEnergy(int newValue) {
        if (energyLost)
            energyTimer = 0f;

        // Reset timer and visual decay if decaying
        if (newValue < energyCurrentValue.value) {
            energyLost = true;
            energyDecaying = false;
            energyRecovering = false;
        }

        energyCurrentValue.value += newValue;
    }

    public int GetCurrentEnergy() {
        return Mathf.RoundToInt(energyCurrentValue.value);
    }

    public void SetMaxStamina(int newValue) {
        staminaMaxValue.value = newValue;
        staminaCurrentValue.maxValue = staminaCurrentValue.value = newValue;
        staminaDestroyedValue.maxValue = staminaDestroyedValue.value = newValue;
    }

    public void SetCurrentStamina(int newValue) {
        if (staminaLost)
            staminaTimer = 0f;
        if (staminaRecovering) 
            StopCoroutine(StaminaRecovery());

        // Reset timer and visual decay if decaying
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value)
        {
            staminaLost = true;
            staminaDecaying = false;
            staminaRecovering = false;
        }
        else
            staminaDestroyedValue.value += newValue;

        staminaCurrentValue.value += newValue;
    }
    
    public int GetCurrentStamina() {
        return Mathf.RoundToInt(staminaCurrentValue.value);
    }

    // HEALTH ///////////////////////////////////////////////////////////////////////////////////////

    IEnumerator HealthRecovery() {
        if (!healthRecovering)
            healthRecovering = true;

        yield return new WaitForSeconds(healthRecoveryDelay);

        do
        {
            SetCurrentStamina(healthRecoveryRate);
            yield return new WaitForSeconds(recoverySpeed * Time.deltaTime);
        }
        while (healthCurrentValue.value < healthCurrentValue.maxValue);
        SetCurrentStamina((int)healthCurrentValue.maxValue);
    }
    public void SetHealthRecoveryRate(int newValue) {
        healthRecoveryRate = newValue;
    }
    public int GetHealthRecoveryRate() {
        return healthRecoveryRate;
    }
    public void SetHealthRecoveryDelay(float newValue) {
        healthRecoveryDelay = newValue;
    }

    // ENERGY ///////////////////////////////////////////////////////////////////////////////////

    IEnumerator EnergyRecovery() {
        if (!energyRecovering)
            energyRecovering = true;

        yield return new WaitForSeconds(energyRecoveryDelay);

        do
        {
            SetCurrentStamina(energyRecoveryRate);
            yield return new WaitForSeconds(recoverySpeed * Time.deltaTime);
        }
        while (energyCurrentValue.value < energyCurrentValue.maxValue);
        SetCurrentStamina((int)energyCurrentValue.maxValue);
    }
    public void SetEnergyRecoveryRate(int newValue) {
        energyRecoveryRate = newValue;
    }
    public int GetEnergyRecoveryRate() {
        return energyRecoveryRate;
    }
    public void SetEnergyRecoveryDelay(float newValue) {
        energyRecoveryDelay = newValue;
    }

    // STAMINA ///////////////////////////////////////////////////////////////////////////////////

    IEnumerator StaminaRecovery() {
        if (!staminaRecovering)
            staminaRecovering = true;
        yield return new WaitForSeconds(staminaRecoveryDelay);

        do
        {
            SetCurrentStamina(staminaRecoveryRate);
            yield return new WaitForSeconds(recoverySpeed * Time.deltaTime);
        }
        while (staminaCurrentValue.value < staminaCurrentValue.maxValue);
        SetCurrentStamina((int)staminaCurrentValue.maxValue);
    }
    public void SetStaminaRecoveryRate(int newValue) {
        staminaRecoveryRate = newValue;
    }
    public int GetStaminaRecoveryRate() {
        return staminaRecoveryRate;
    }
    public void SetStaminaRecoveryDelay(float newValue) {
        staminaRecoveryDelay = newValue;
    }
}
