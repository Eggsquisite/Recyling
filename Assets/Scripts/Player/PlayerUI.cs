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
    private int healthRecoveryValue;
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
    private int energyRecoveryValue;
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
    private int staminaRecoveryValue;
    private float staminaTimer;
    private float staminaRecoveryDelay;

    private bool healthRecoverable, energyRecoverable, staminaRecoverable;
    private Coroutine healthRecoveryRoutine, energyRecoveryRoutine, staminaRecoveryRoutine;
    private Coroutine healthVisualDecayRoutine, energyVisualDecayRoutine, staminaVisualDecayRoutine;
    private Coroutine healthDestroyedTimerRoutine, energyDestroyedTimerRoutine, staminaDestroyedTimerRoutine;

    // Start is called before the first frame update
    void Start()
    {
        healthDestroyedValue.value = healthDestroyedValue.maxValue = healthCurrentValue.maxValue;
        energyDestroyedValue.value = energyDestroyedValue.maxValue = energyCurrentValue.maxValue;
        staminaDestroyedValue.value = staminaDestroyedValue.maxValue = staminaCurrentValue.maxValue;

        healthRecoverable = energyRecoverable = staminaRecoverable = true;
    }

    private void BarUpdate() {
/*        if (healthLost)
            DestroyedTimer(ref healthTimer, ref healthLost, ref healthDecaying, 1);
        else if (healthDecaying)
            VisualDecay(healthCurrentValue, healthDestroyedValue, ref healthDecaying, 1);

        if (energyLost)
            DestroyedTimer(ref energyTimer, ref energyLost, ref energyDecaying, 2);
        else if (energyDecaying)
            VisualDecay(energyCurrentValue, energyDestroyedValue, ref energyDecaying, 2);

        if (staminaLost)
            DestroyedTimer(ref staminaTimer, ref staminaLost, ref staminaDecaying, 3);
        else if (staminaDecaying)
            VisualDecay(staminaCurrentValue, staminaDestroyedValue, ref staminaDecaying, 3);*/
    }

/*    private void DestroyedTimer(ref float timer, ref bool flag, ref bool decayFlag, int index) {
        if (timer < visualDelay)
            timer += Time.deltaTime;
        else if (timer >= visualDelay) {
            timer = 0f;
            flag = false;
            decayFlag = true;

            if (index == 1)
                StartCoroutine(VisualDecay(healthCurrentValue, healthDestroyedValue, 1));
            else if (index == 2)
                StartCoroutine(VisualDecay(energyCurrentValue, energyDestroyedValue, 2));
            else if (index == 3)
                StartCoroutine(VisualDecay(staminaCurrentValue, staminaDestroyedValue, 3));
        }
    }*/

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

/*    private void VisualDecay(Slider currentValue, Slider decayValue, ref bool decayFlag, int index) {
        if (decayValue.value > currentValue.value) {
            decayValue.value -= decaySpeed * Time.deltaTime;
        } else if (decayValue.value <= currentValue.value && decayFlag) {
            decayValue.value = currentValue.value;
            decayFlag = false;

            if (index == 1)
                StartCoroutine(HealthRecovery());
            else if (index == 2)
                StartCoroutine(EnergyRecovery());
            else if (index == 3)
                StartCoroutine(StaminaRecovery());
        }
    }*/

    // HEALTH ///////////////////////////////////////////////////////////////////////////////////////
    public void SetMaxHealth(int newValue) {
        healthMaxValue.value = newValue;
        healthCurrentValue.maxValue = healthCurrentValue.value = newValue;
        healthDestroyedValue.maxValue = healthDestroyedValue.value = newValue;
    }

    public void SetCurrentHealth(int newValue) {
        // Reset timer and visual decay if decaying
        if (healthCurrentValue.value + newValue < healthCurrentValue.value)
        {
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
        while (healthCurrentValue.value < healthCurrentValue.maxValue && healthRecoveryValue > 0) {
            if (healthLost) {
                healthRecovering = false;

                if (healthRecoveryRoutine != null)
                    StopCoroutine(healthRecoveryRoutine);
                yield break;
            }
            SetCurrentEnergy(healthRecoveryValue);
            yield return new WaitForSeconds(recoverySpeed);
        }

        healthRecovering = false;
        yield break;
    }
    public void SetHealthRecoveryValue(int newValue) {
        healthRecoveryValue = newValue;
    }
    public int GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
    public void SetHealthRecoveryDelay(float newValue) {
        healthRecoveryDelay = newValue;
    }

    // ENERGY ///////////////////////////////////////////////////////////////////////////////////
    public void SetMaxEnergy(int newValue) {
        energyMaxValue.value = newValue;
        energyCurrentValue.maxValue = energyCurrentValue.value = newValue;
        energyDestroyedValue.maxValue = energyDestroyedValue.value = newValue;
    }

    public void SetCurrentEnergy(int newValue) {
        // Reset timer and visual decay if decaying
        if (energyCurrentValue.value + newValue < energyCurrentValue.value) {
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
    }

    public int GetCurrentEnergy() {
        return (int)energyCurrentValue.value;
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

    public void SetEnergyRecoveryValue(int newValue) {
        energyRecoveryValue = newValue;
    }
    public int GetEnergyRecoveryValue() {
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

    public void SetCurrentStamina(int newValue) {
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value) {
            staminaLost = true;
            staminaDecaying = false;
            staminaRecovering = false;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            if (staminaDestroyedTimerRoutine != null)
                StopCoroutine(staminaDestroyedTimerRoutine);
            StartCoroutine(DestroyedTimer(3));
        }
        else
            staminaDestroyedValue.value += newValue;

        staminaTimer = 0f;
        staminaCurrentValue.value += newValue;
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

    public void SetStaminaRecoveryValue(int newValue) {
        staminaRecoveryValue = newValue;
    }
    public int GetStaminaRecoveryValue() {
        return staminaRecoveryValue;
    }
    public void SetStaminaRecoveryDelay(float newValue) {
        staminaRecoveryDelay = newValue;
    }
    public void SetHealthRecoverable(bool value) {
        healthRecoverable = value;
    }
    public void SetEnergyRecoverable(bool value) {
        energyRecoverable = value;
    }
    public void SetStaminaRecoverable(bool value) {
        staminaRecoverable = value;
    }
}
