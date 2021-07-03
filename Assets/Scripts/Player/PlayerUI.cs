using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private float visualDelay;
    [SerializeField]
    private float updateSpeed;
    [SerializeField]
    private float updateModifier;

    [SerializeField]
    private ResourceUI UI;

    [Header("Currency Upgrade")]
    [SerializeField]
    private int baseRequiredCurrencyModifier;

    private int prevRequiredCurrency;

    private float requiredCurrency;
    private float requiredCurrencyModifier;

    [Header("Currency")]
    private Text currencyText;
    private Text futureCurrencyText;

    private bool isAdding;

    private int baseTmp;
    private int currencyTmp;
    private float baseCurrency;
    private int futureCurrency;

    private Coroutine addCurrencyRoutine;

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
    private float energyToHealthMultiplier;

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

    private int energyRegenInUse = 0;
    private float energyRecoveryValue;

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
    private Coroutine futureHealthRecoveryRoutine;
    private Coroutine healthRecoveryRoutine, energyRecoveryRoutine, staminaRecoveryRoutine;
    private Coroutine healthVisualDecayRoutine, energyVisualDecayRoutine, staminaVisualDecayRoutine;
    private Coroutine healthDestroyedTimerRoutine, energyDestroyedTimerRoutine, staminaDestroyedTimerRoutine;

    // Start is called before the first frame update
    void Awake()
    {
        if (UI == null)
            Debug.Log("Canvas UI is Not Assigned to PlayerUI.cs in Inspector");

        healthRecoverable = energyRecoverable = staminaRecoverable = true;
        currencyText = UI.GetCurrencyText();
        futureCurrencyText = UI.GetFutureCurrencyText();

        currencyTmp = Mathf.RoundToInt(baseCurrency);
        currencyText.text = currencyTmp.ToString();
        futureCurrencyText.gameObject.SetActive(false);

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

    /// CURRENCY CODE ///////////////////////////////////////////////////////////////////////////
    public void LoadCurrency(int newValue)
    {
        baseCurrency = newValue;
        currencyText.text = newValue.ToString();
    } 

    public void SetCurrency(int newValue) {
        if (newValue > 0) {
            if (addCurrencyRoutine != null)
                StopCoroutine(addCurrencyRoutine);

            addCurrencyRoutine = StartCoroutine(AddCurrencyValues(newValue));
        } else { 
            baseCurrency += newValue;
            futureCurrency = newValue;
            futureCurrencyText.gameObject.SetActive(true);
            futureCurrencyText.text = futureCurrency.ToString();
            currencyTmp = Mathf.RoundToInt(baseCurrency);
            currencyText.text = currencyTmp.ToString();
        }
    }

    public void TurnOffText() {
        // called during animation event
        futureCurrency = 0;
        futureCurrencyText.gameObject.SetActive(false);
    }

    IEnumerator AddCurrencyValues(int newValue) {
        futureCurrency += newValue;
        futureCurrencyText.gameObject.SetActive(true);
        futureCurrencyText.text = "+" + futureCurrency.ToString();

        if (!isAdding) { 
            yield return new WaitForSeconds(0.5f);
            baseTmp = Mathf.RoundToInt(baseCurrency);
        }
    
        int tmp = baseTmp + futureCurrency;
        isAdding = true;
        while (baseCurrency < tmp)
        {
            baseCurrency += tmp / 10 * updateModifier * Time.deltaTime;
            if (baseCurrency > tmp)
                baseCurrency = tmp;

            currencyTmp = Mathf.RoundToInt(baseCurrency);
            currencyText.text = currencyTmp.ToString();
            yield return new WaitForSeconds(Time.deltaTime);
        }

        yield return new WaitForSeconds(0.5f);
        isAdding = false;
        TurnOffText();
        SaveManager.instance.SaveCurrency(Mathf.RoundToInt(baseCurrency));
    }

    public int GetCurrency() {
        return Mathf.RoundToInt(baseCurrency);
    }

    public int GetRequiredCurrency(int futureLevel) {
        requiredCurrencyModifier = baseRequiredCurrencyModifier * 1.33f * futureLevel;
        requiredCurrency = requiredCurrencyModifier * 1.18f * futureLevel + baseRequiredCurrencyModifier;
        return Mathf.RoundToInt(requiredCurrency);
    }

    /// TIMER AND VISUAL DECAY ///////////////////////////////////////////////////////////////////////////
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

            decayValue.value -= updateSpeed * updateModifier * Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        decayValue.value = currentTmp;
        
        if (index == 1 && !healthRecovering) {
            healthDecaying = false;

            /*if (healthRecoveryRoutine != null)
                StopCoroutine(healthRecoveryRoutine);
            healthRecoveryRoutine = StartCoroutine(HealthRecovery(healthRecoveryDelay));*/
        } else if (index == 2 && !energyRecovering) {
            energyDecaying = false;

            /*if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
            energyRecoveryRoutine = StartCoroutine(EnergyRecovery(energyRecoveryDelay));*/
        } else if (index == 3 && !staminaRecovering) {
            staminaDecaying = false;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            staminaRecoveryRoutine = StartCoroutine(StaminaRecovery(staminaRecoveryDelay));
        }
    }

    // HEALTH ///////////////////////////////////////////////////////////////////////////////////////
    public void LoadCurrentHealth(float newValue) {
        healthDestroyedValue.value = healthCurrentValue.value = newValue;
    }

    public void SetMaxHealth(int newValue) {
        // IF THROWING ERROR, ASSIGN CANVAS IN INSPECTOR
        healthMaxValue.value = newValue;
        healthCurrentValue.maxValue = healthCurrentValue.value = newValue;
        healthDestroyedValue.maxValue = healthDestroyedValue.value = newValue;
    }

    public void SetCurrentHealth(float newValue) {
        // Reset timer and visual decay if decaying
        if (healthCurrentValue.value + newValue < healthCurrentValue.value) {
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

    public void SetFutureHealth(float multiplier) {
        // increase future health, then heal up to that amount if player is not hit
        healthDestroyedValue.value += healthRecoveryValue * updateModifier * multiplier * Time.deltaTime;
    }

    public void BeginFutureHealthRecovery() {
        if (futureHealthRecoveryRoutine != null)
            StopCoroutine(futureHealthRecoveryRoutine);
        futureHealthRecoveryRoutine = StartCoroutine(FutureHealthRecovery());
    }

    IEnumerator FutureHealthRecovery() { 
        // visual health fill up
        while (healthCurrentValue.value < healthDestroyedValue.value && !healthDecaying)
        {
            healthCurrentValue.value += updateSpeed * updateModifier * Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void StopFutureHealthRecovery() {
        if (futureHealthRecoveryRoutine != null)
            StopCoroutine(futureHealthRecoveryRoutine);
    }

    public int GetCurrentHealth() {
        return (int)healthCurrentValue.value;
    }
    
    public int GetFutureHealth() {
        return (int)healthDestroyedValue.value;
    }

/*    IEnumerator HealthRecovery(float delay) {
        healthRecovering = true;

        yield return new WaitForSeconds(delay);
        while (healthCurrentValue.value < healthCurrentValue.maxValue && healthRecoveryValue > 0)
        {
            if (healthLost) {
                healthRecovering = false;

                if (healthRecoveryRoutine != null)
                    StopCoroutine(healthRecoveryRoutine);
                yield break;
            }
            SetCurrentHealth(healthRecoveryValue * recoveryModifier * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        healthRecovering = false;
        yield break;
    }*/

    public void SetHealthRecoveryValue(float newValue) {
        healthRecoveryValue = newValue;
    }
    public float GetHealthRecoveryValue() {
        return healthRecoveryValue;
    }
    public int GetHealthMaxValue() {
        return (int)healthCurrentValue.maxValue;
    }
    public bool GetHealthDecaying() {
        return healthDecaying;
    }
    public void SetEnergyToHealthMultiplier(float newValue) {
        energyToHealthMultiplier = newValue;
    }

    // ENERGY ///////////////////////////////////////////////////////////////////////////////////
    public void LoadCurrentEnergy(float newValue) {
        energyDestroyedValue.value = energyCurrentValue.value = newValue;
    }

    public void SetMaxEnergy(int newValue) {
        energyMaxValue.value = newValue;
        energyCurrentValue.maxValue = energyCurrentValue.value = newValue;
        energyDestroyedValue.maxValue = energyDestroyedValue.value = newValue;
    }

    public void SetCurrentEnergy(float newValue) {
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

    public IEnumerator EnergyRegenOnHit(float energyRegenMultiplier) {
        var tmp = energyRecoveryValue;
        energyRegenInUse += 1;
        energyRecoveryValue = energyRecoveryValue * energyRegenInUse * energyRegenMultiplier;
        if (energyRecoveryRoutine != null)
            StopCoroutine(energyRecoveryRoutine);
        energyRecoveryRoutine = StartCoroutine(EnergyRecovery(0f));

        yield return new WaitForSeconds(0.5f);
        energyRecoveryValue = tmp;
        energyRegenInUse -= 1;
        if (energyRegenInUse < 0) 
            energyRegenInUse = 0;

        if (energyRegenInUse <= 0) {
            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
        }
    }

    public void EnergyWithoutDecay(float newValue) {
        if (energyCurrentValue.value + newValue < energyCurrentValue.value)
        {
            energyCurrentValue.value += newValue * energyToHealthMultiplier * updateModifier * Time.deltaTime;
            energyDestroyedValue.value += newValue * energyToHealthMultiplier * updateModifier * Time.deltaTime;

            if (energyCurrentValue.value < 0) { 
                energyCurrentValue.value = 0;
                energyDestroyedValue.value = 0;
            }

/*            if (energyRecoveryRoutine != null)
                StopCoroutine(energyRecoveryRoutine);
            energyRecoveryRoutine = StartCoroutine(EnergyRecovery(0f));*/
        }
    }

    IEnumerator EnergyRecovery(float delay) {
        energyRecovering = true;

        yield return new WaitForSeconds(delay);
        while (energyCurrentValue.value < energyCurrentValue.maxValue && energyRecoveryValue > 0) {
            if (energyLost) {
                energyRecovering = false;

                if (energyRecoveryRoutine != null)  
                    StopCoroutine(energyRecoveryRoutine);
                yield break;
            }
            SetCurrentEnergy(energyRecoveryValue * updateModifier * Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
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
    // STAMINA ///////////////////////////////////////////////////////////////////////////////////
    public void SetMaxStamina(int newValue) {
        staminaMaxValue.value = newValue;
        staminaCurrentValue.maxValue = staminaCurrentValue.value = newValue;
        staminaDestroyedValue.maxValue = staminaDestroyedValue.value = newValue;
    }

    public void SetCurrentStamina(float newValue) {
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value) {
            staminaLost = true;
            staminaDecaying = false;
            staminaRecovering = false;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            if (staminaDestroyedTimerRoutine != null)
                StopCoroutine(staminaDestroyedTimerRoutine);
            staminaDestroyedTimerRoutine = StartCoroutine(DestroyedTimer(3));
        }
        else // for positive cases
            staminaDestroyedValue.value += newValue;

        staminaCurrentValue.value += newValue;
    }

    public void StaminaWithoutDecay(float newValue) {
        if (staminaCurrentValue.value + newValue < staminaCurrentValue.value) {
            staminaCurrentValue.value += newValue;
            staminaDestroyedValue.value += newValue;

            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
            staminaRecoveryRoutine = StartCoroutine(StaminaRecovery(0.25f));
        }
    }
    
    public int GetCurrentStamina() {
        return (int)staminaCurrentValue.value;
    }

    IEnumerator StaminaRecovery(float delay) {
        staminaRecovering = true;
        yield return new WaitForSeconds(delay);

        while (staminaCurrentValue.value < staminaCurrentValue.maxValue && staminaRecoveryValue > 0)
        {
            if (staminaLost) {
                staminaRecovering = false;
                if (staminaRecoveryRoutine != null)
                    StopCoroutine(staminaRecoveryRoutine);

                yield break;
            }
            SetCurrentStamina(staminaRecoveryValue * updateModifier * Time.deltaTime);
            //Debug.Log("Stamina recov value: " + staminaRecoveryValue);
            yield return new WaitForSeconds(Time.deltaTime);
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
        if (!staminaRecoverable) { 
            if (staminaRecoveryRoutine != null)
                StopCoroutine(staminaRecoveryRoutine);
        }
    }
}
