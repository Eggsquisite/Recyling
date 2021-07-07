using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField]
    private TMPro.TextMeshProUGUI currencyText;
    [SerializeField]
    private TMPro.TextMeshProUGUI futureCurrencyText;

    [Header("Health")]
    [SerializeField]
    private Slider healthMaxValue;
    [SerializeField]
    private Slider healthCurrentValue;
    [SerializeField]
    private Slider healthDestroyedValue;

    [Header("Energy")]
    [SerializeField]
    private Slider energyMaxValue;
    [SerializeField]
    private Slider energyCurrentValue;
    [SerializeField]
    private Slider energyDestroyedValue;

    [Header("Stamina")]
    [SerializeField]
    private Slider staminaMaxValue;
    [SerializeField]
    private Slider staminaCurrentValue;
    [SerializeField]
    private Slider staminaDestroyedValue;

    public TMPro.TextMeshProUGUI GetCurrencyText() {
        return currencyText;
    }
    public TMPro.TextMeshProUGUI GetFutureCurrencyText() {
        return futureCurrencyText;
    }
    public Slider GetHealthMaxValue() {
        return healthMaxValue;
    }
    public Slider GetHealthCurrentValue() {
        return healthCurrentValue;
    }
    public Slider GetHealthDestroyedValue() {
        return healthDestroyedValue;
    }

    public Slider GetEnergyMaxValue() {
        return energyMaxValue;
    }
    public Slider GetEnergyCurrentValue() {
        return energyCurrentValue;
    }
    public Slider GetEnergyDestroyedValue() {
        return energyDestroyedValue;
    }

    public Slider GetStaminaMaxValue() {
        return staminaMaxValue;
    }
    public Slider GetStaminaCurrentValue() {
        return staminaCurrentValue;
    }
    public Slider GetStaminaDestroyedValue() {
        return staminaDestroyedValue;
    }

}
