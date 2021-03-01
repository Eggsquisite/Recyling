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

    [SerializeField]
    private ResourceUI UI;

    [Header("Health")]
    //[SerializeField]
    private Slider healthMaxValue;
    //[SerializeField]
    private Slider healthCurrentValue;
    //[SerializeField]
    private Slider healthDestroyedValue;

    private bool healthLost;
    private bool healthDecaying;
    private bool healthRecovering;
    private float healthRecoveryValue;
    private float healthRecoveryDelay;

    [Header("Energy")]
    //[SerializeField]
    private Slider energyMaxValue;
    //[SerializeField]
    private Slider energyCurrentValue;
    //[SerializeField]
    private Slider energyDestroyedValue;

    private bool energyLost;
    private bool energyDecaying;
    private bool energyRecovering;
    private float energyRecoveryValue;
    private float energyRecoveryDelay;
    private float baseEnergyRecoveryValue;
    private float energyRecoverDuration;

    [Header("Stamina")]
    //[SerializeField]
    private Slider staminaMaxValue;
    //[SerializeField]
    private Slider staminaCurrentValue;
    //[SerializeField]
    private Slider staminaDestroyedValue;

    private bool staminaLost;
    private bool staminaDecaying;
    private bool staminaRecovering;
    private float staminaRecoveryValue;
    private float staminaRecoveryDelay;

    private bool healthRecoverable, energyRecoverable, staminaRecoverable;
    private Coroutine healthRecoveryRoutine, energyRecoveryRoutine, staminaRecoveryRoutine;
    private Coroutine healthVisualDecayRoutine, energyVisualDecayRoutine, staminaVisualDecayRoutine;
    private Coroutine healthDestroyedTimerRoutine, energyDestroyedTimerRoutine, staminaDestroyedTimerRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        healthRecoverable = energyRecoverable = staminaRecoverable = true;
        baseEnergyRecoveryValue = energyRecoveryValue;

        // initialize sliders - these are ALL slider variables not floats
        healthMaxValue = UI.GetHealthMaxValue();
        healthCurrentValue = UI.GetHealthCurrentValue();
        healthDestroyedValue = UI.GetHealthDestroyedValue();

        energyMaxValue = UI.GetEnergyMaxValue();
        energyCurrentValue = UI.GetEnergyCurrentValue();
        energyDestroyedValue = UI.GetEnergyDestroyedValue();

        staminaMaxValue = UI.GetStaminaMaxValue();
        staminaCurrentValue = UI.GetStaminaCurrentValue();
        staminaDestroyedValue = UI.GetStaminaDestroyedValue();
        /////////////////////////////////////////////////////////////////

        InitializeSliders();
    }

    private void InitializeSliders() {
        healthDestroyedValue.value = healthDestroyedValue.maxValue = healthCurrentValue.value = healthCurrentValue.maxValue;
        energyDestroyedValue.value = energyDestroyedValue.maxValue = energyCurrentValue.value = energyCurrentValue.maxValue;
        staminaDestroyedValue.value = staminaDestroyedValue.maxValue = staminaCurrentValue.value = staminaCurrentValue.maxValue;
    }

    IEnumerator DestroyedTimer(int index) {
        if (index == 1 && healthRecoveryRoutine != null)
            StopCoroutine(healthRecoveryRoutine);
        else if (index == 2 && energyRecoveryRoutine != null)
            StopCoroutine(energyRecoveryRoutine);
        else if (index == 3 && staminaRecoveryRoutine != null)
            StopCoroutine(staminaRecoveryRoutine);

        yield return new WaitForSeconds(visualDelay);
        
        if (index == 1 && !healthDecaying) {
            healthLost = false;

            if (healthVisualDecayRoutine != null)
                StopCoroutine(healthVisualDecayRoutine);
            healthVisualDecayRoutine = StartCoroutine(VisualDecay(healthCurrentValue, healthDestroyedValue, 1));
        } else if (index == 2 && !energyDecaying) {
            energyLost = false;

            if (energyVisualDecayRoutine != null)
                StopCoroutine(energyVisualDecayRoutine);
            energyVisualDecayRoutine = StartCoroutine(VisualDecay(energyCurrentValue, energyDestroyedValue, 2));
        } else if (index == 3 && !staminaDecaying) {
            staminaLost = false;

            if (staminaVisualDecayRoutine != null)
                StopCoroutine(staminaVisualDecayRoutine);
            staminaVisualDecayRoutine = StartCoroutine(VisualDecay(staminaCurrentValue, staminaDestroyedValue, 3));
        }
    }

    IEnumerator VisualDecay(Slider currentValue, Slider decayValue, int index) {
        if (index == 1)
            healthDecaying = true;
        else if (index == 2)
            energyDecaying = true;
        else if (index == 3)
            staminaDecaying = true;
        var currentTmp = currentValue.value;

        while (decayValue.value > currentTmp) {
            if (index == 1 && (!healthDecaying || healthLost)) {
                if (healthRecoveryRoutine != null)
                    StopCoroutine(healthRecoveryRoutine);
                yield break;
            }
            else if (index == 2 && (!energyDecaying || energyLost)) {
                if (energyRecoveryRoutine != null)
                    StopCoroutine(energyRecoveryRoutine);
                yield break;
            }
            else if (index == 3 && (!staminaDecaying || staminaLost)) {
                if (staminaRecoveryRoutine != null)
                    StopCoroutine(staminaRecoveryRoutine);
                yield break;
            }

            decayValue.value -= decaySpeed * Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        decayValue.value = currentTmp;
        
        if (index == 1 && !healthRecovering) {
            healthDecaying = false;

            if (healthRecoveryRoutine != null)
                StopCoroutine(healthRecoveryRoutine);
            healthRecoveryRoutine = StartCoroutine(HealthRecovery());
        } else if (index == 2 && !energyRecovering) {
            energyDecaying = false;

            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
            energyRecoveryRoutine = StartCoroutine(EnergyRecovery());
        } else if (index == 3 && !staminaRecovering) {
            staminaDecaying = false;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            staminaRecoveryRoutine = StartCoroutine(StaminaRecovery());
        }
    }

    // HEALTH ///////////////////////////////////////////////////////////////////////////////////////
    public void SetMaxHealth(int newValue) {
        healthMaxValue.value = newValue;
        healthCurrentValue.maxValue = healthCurrentValue.value = newValue;
        healthDestroyedValue.maxValue = healthDestroyedValue.value = newValue;
    }

    public void SetCurrentHealth(float newValue) {
        // Reset timer and visual decay if decaying
        if (healthCurrentValue.value + newValue < healthCurrentValue.value || healthCurrentValue.value <= 0) {
            healthLost = true;
            healthDecaying = false;
            healthRecovering = false;

            if (healthRecoveryRoutine != null)
                StopCoroutine(healthRecoveryRoutine);

            if (healthDestroyedTimerRoutine != null)
                StopCoroutine(healthDestroyedTimerRoutine);
            healthDestroyedTimerRoutine = StartCoroutine(DestroyedTimer(1));
        }
        else
            healthDestroyedValue.value += newValue;

        healthCurrentValue.value += newValue;
    }

    public int GetCurrentHealth() {
        return (int)healthCurrentValue.value;
    }

    IEnumerator HealthRecovery() {
        healthRecovering = true;

        yield return new WaitForSeconds(healthRecoveryDelay);
        while (healthCurrentValue.value < healthCurrentValue.maxValue && healthRecoveryValue > 0)
        {
            if (healthLost) {
                healthRecovering = false;

                if (healthRecoveryRoutine != null)
                    StopCoroutine(healthRecoveryRoutine);
                yield break;
            }
            SetCurrentHealth(healthRecoveryValue);
            yield return new WaitForSeconds(recoverySpeed);
        }

        healthRecovering = false;
        yield break;
    }
    public void SetHealthRecoveryValue(float newValue) {
        healthRecoveryValue = newValue;
    }
    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
    public void SetHealthRecoveryDelay(float newValue) {
        healthRecoveryDelay = newValue;
    }
    public int GetHealthMaxValue() {
        return (int)healthCurrentValue.maxValue;
    }

    // ENERGY ///////////////////////////////////////////////////////////////////////////////////
    public void SetMaxEnergy(int newValue) {
        energyMaxValue.value = newValue;
        energyCurrentValue.maxValue = energyCurrentValue.value = newValue;
        energyDestroyedValue.maxValue = energyDestroyedValue.value = newValue;
    }

    public void SetCurrentEnergy(float newValue) {
        // Reset timer and visual decay if decaying
        if (energyCurrentValue.value + newValue < energyCurrentValue.value || energyCurrentValue.value <= 0) {
            energyLost = true;
            energyDecaying = false;
            energyRecovering = false;

            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);

            if (energyDestroyedTimerRoutine != null)
                StopCoroutine(energyDestroyedTimerRoutine);
            energyDestroyedTimerRoutine = StartCoroutine(DestroyedTimer(2));
        }
        else
            energyDestroyedValue.value += newValue;

        energyCurrentValue.value += newValue;
        if (energyCurrentValue.value < 0)
            energyCurrentValue.value = 0;
    }

    public IEnumerator EnergyRegenOnHit(float recoveryValue) {
        energyRecoveryValue += recoveryValue;
        if (energyRecoveryRoutine != null)
            StopCoroutine(energyRecoveryRoutine);
        energyRecoveryRoutine = StartCoroutine(EnergyRecovery());

        yield return new WaitForSeconds(0.5f);
        energyRecoveryValue -= recoveryValue;
        if (energyRecoveryValue < 0)
            energyRecoveryValue = 0;

        if (energyRecoveryValue <= 0) {
            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
        }
    }

    public void EnergyWithoutDecay(float newValue) {
        if (energyCurrentValue.value + newValue < energyCurrentValue.value)
        {
            energyCurrentValue.value += newValue;
            energyDestroyedValue.value += newValue;

            if (energyCurrentValue.value < 0) { 
                energyCurrentValue.value = 0;
                energyDestroyedValue.value = 0;
            }

            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
            energyRecoveryRoutine = StartCoroutine(EnergyRecovery());
        }
    }

    IEnumerator EnergyRecovery() {
        energyRecovering = true;

        yield return new WaitForSeconds(energyRecoveryDelay);
        while (energyCurrentValue.value < energyCurrentValue.maxValue && energyRecoveryValue > 0) {
            if (energyLost) {
                energyRecovering = false;

                if (energyRecoveryRoutine != null)  
                    StopCoroutine(energyRecoveryRoutine);
                yield break;
            }
            SetCurrentEnergy(energyRecoveryValue);
            yield return new WaitForSeconds(recoverySpeed);
        }

        energyRecovering = false;
        yield break;
    }

    public int GetCurrentEnergy() {
        return (int)energyCurrentValue.value;
    }
    public void SetEnergyRecoveryValue(float newValue) {
        energyRecoveryValue = newValue;
    }
    public float GetEnergyRecoveryValue() {
        return energyRecoveryValue;
    }
    public void SetEnergyRecoveryDelay(float newValue) {
        energyRecoveryDelay = newValue;
    }

    // STAMINA ///////////////////////////////////////////////////////////////////////////////////
    public void SetMaxStamina(int newValue) {
        staminaMaxValue.value = newValue;
        staminaCurrentValue.maxValue = staminaCurrentValue.value = newValue;
        staminaDestroyedValue.maxValue = staminaDestroyedValue.value = newValue;
    }

    public void SetCurrentStamina(float newValue) {
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value || staminaCurrentValue.value <= 0) {
            staminaLost = true;
            staminaDecaying = false;
            staminaRecovering = false;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            if (staminaDestroyedTimerRoutine != null)
                StopCoroutine(staminaDestroyedTimerRoutine);
            StartCoroutine(DestroyedTimer(3));
        }
        else // for positive cases
            staminaDestroyedValue.value += newValue;

        staminaCurrentValue.value += newValue;
        if (staminaCurrentValue.value < 0)
            staminaCurrentValue.value = 0;
    }

    public void StaminaWithoutDecay(float newValue) {
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value) {
            staminaCurrentValue.value += newValue;
            staminaDestroyedValue.value += newValue;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            staminaRecoveryRoutine = StartCoroutine(StaminaRecovery());
        }
    }
    
    public int GetCurrentStamina() {
        return (int)staminaCurrentValue.value;
    }

    IEnumerator StaminaRecovery() {
        staminaRecovering = true;

        yield return new WaitForSeconds(staminaRecoveryDelay);
        while (staminaCurrentValue.value < staminaCurrentValue.maxValue && staminaRecoveryValue > 0)
        {
            if (staminaLost) {
                staminaRecovering = false;
                if (staminaRecoveryRoutine != null)
                    StopCoroutine(staminaRecoveryRoutine);
                yield break;
            }
            SetCurrentStamina(staminaRecoveryValue);
            yield return new WaitForSeconds(recoverySpeed);
        }

        staminaRecovering = false;
        yield break;
    }

    public void SetStaminaRecoveryValue(float newValue) {
        staminaRecoveryValue = newValue;
    }
    public float GetStaminaRecoveryValue() {
        return staminaRecoveryValue;
    }
    public void SetStaminaRecoveryDelay(float newValue) {
        staminaRecoveryDelay = newValue;
    }
    public void SetHealthRecoverable(bool value) {
        healthRecoverable = value;
        if (!healthRecoverable)
            if (healthRecoveryRoutine != null)
                StopCoroutine(healthRecoveryRoutine);
    }
    public void SetEnergyRecoverable(bool value) {
        energyRecoverable = value;
        if (!energyRecoverable)
            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
    }
    public void SetStaminaRecoverable(bool value) {
        staminaRecoverable = value;
        if (!staminaRecoverable)
            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
    }
}
