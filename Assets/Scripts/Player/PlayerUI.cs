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

    [Header("Health")]
    [SerializeField]
    private Slider healthMaxValue;
    [SerializeField]
    private Slider healthCurrentValue;
    [SerializeField]
    private Slider healthDestroyedValue;

    private bool healthLost;
    private bool healthDecaying;
    private float healthTimer;

    [Header("Energy")]
    [SerializeField]
    private Slider energyMaxValue;
    [SerializeField]
    private Slider energyCurrentValue;
    [SerializeField]
    private Slider energyDestroyedValue;

    private bool energyLost;
    private bool energyDecaying;
    private float energyTimer;

    [Header("Stamina")]
    [SerializeField]
    private Slider staminaMaxValue;
    [SerializeField]
    private Slider staminaCurrentValue;
    [SerializeField]
    private Slider staminaDestroyedValue;

    private bool staminaLost;
    private bool staminaDecaying;
    private float staminaTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (healthLost)
            DestroyedTimer(ref healthTimer, ref healthLost, ref healthDecaying);
        else if (healthDecaying)
            VisualDecay(healthCurrentValue, healthDestroyedValue, ref healthDecaying);

        if (energyLost)
            DestroyedTimer(ref energyTimer, ref energyLost, ref energyDecaying);
        else if (energyDecaying)
            VisualDecay(energyCurrentValue, energyDestroyedValue, ref energyDecaying);
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

    private void VisualDecay(Slider currentValue, Slider decayValue, ref bool decayFlag) {

        if (decayValue.value > currentValue.value) {
            Debug.Log("Decaying");
            decayValue.value -= decaySpeed * Time.deltaTime;
        } else if (decayValue.value <= currentValue.value) {
            decayValue.value = currentValue.value;
            decayFlag = false;
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
        }

        healthCurrentValue.value += newValue;
    }

    public void SetMaxEnergy(int newValue) {
        energyMaxValue.value = newValue;
        energyCurrentValue.maxValue = energyCurrentValue.value = newValue;
        energyDestroyedValue.maxValue = energyDestroyedValue.value = newValue;
    }

    public void SetCurrentEnergy(int newValue) {
        energyCurrentValue.value = newValue;

        if (energyLost)
            energyTimer = 0f;
    }
}
