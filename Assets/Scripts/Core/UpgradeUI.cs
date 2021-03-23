using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private PlayerStats playerStats;

    [Header("Currency Text")]
    [SerializeField]
    private Text currentCurrencyText;
    [SerializeField]
    private Text requiredCurrencyText;

    private int currentCurrency = 100;
    private int requiredCurrency = 20;
    private int remainingCurrency;

    [Header("Levels Text")]
    [SerializeField]
    private Text currentLevelText;
    [SerializeField]
    private Text vitalityLevelText;
    [SerializeField]
    private Text efficiencyLevelText;
    [SerializeField]
    private Text strengthLevelText;
    [SerializeField]
    private Text staminaLevelText;
    [SerializeField]
    private Text specialLevelText;

    private int levelCap;
    private int baseCurrentLevel;
    private int baseVitalityLevel;
    private int baseEfficiencyLevel;
    private int baseStrengthLevel;
    private int baseStaminaLevel;
    private int baseSpecialLevel;

    private int futureCurrentLevel;
    private int futureVitalityLevel;
    private int futureEfficiencyLevel;
    private int futureStrengthLevel;
    private int futureStaminaLevel;
    private int futureSpecialLevel;


    [Header("Increase Buttons")]
    [SerializeField]
    private Button vitalityIncrease;
    [SerializeField]
    private Button efficiencyIncrease;
    [SerializeField]
    private Button strengthIncrease;
    [SerializeField]
    private Button staminaIncrease;
    [SerializeField]
    private Button specialIncrease;

    Button buttonn;

    void OnEnable()
    {
        UpdateCurrencyText();
        UpdateLevelValues();
        UpdateLevelText(0);
    }

    void Start()
    {
        UpdateCurrencyText();
        UpdateLevelValues();
        UpdateLevelText(0);
    }

    private void UpdateLevelValues()
    {
        if (playerStats == null)
            return;

        levelCap = playerStats.GetLevelCap();
        baseCurrentLevel = playerStats.GetPlayerLevel();
        baseVitalityLevel = futureVitalityLevel = playerStats.GetVitalityLevel();
        baseEfficiencyLevel = futureEfficiencyLevel = playerStats.GetEfficiencyLevel();
        baseStrengthLevel = futureStrengthLevel = playerStats.GetStrengthLevel();
        baseStaminaLevel = futureStaminaLevel = playerStats.GetStaminaLevel();
        baseSpecialLevel = futureSpecialLevel = playerStats.GetSpecialLevel();
    }

    private void UpdateLevelText(int index)
    {
        if (index == 0) { 
            currentLevelText.text = baseCurrentLevel.ToString();
            vitalityLevelText.text = baseVitalityLevel.ToString();
            efficiencyLevelText.text = baseEfficiencyLevel.ToString();
            strengthLevelText.text = baseStrengthLevel.ToString();
            staminaLevelText.text = baseStaminaLevel.ToString();
            specialLevelText.text = baseSpecialLevel.ToString();
        } else if (index == 1) {
            currentLevelText.text = futureCurrentLevel.ToString();
            vitalityLevelText.text = futureVitalityLevel.ToString();
            efficiencyLevelText.text = futureEfficiencyLevel.ToString();
            strengthLevelText.text = futureStrengthLevel.ToString();
            staminaLevelText.text = futureStaminaLevel.ToString();
            specialLevelText.text = futureSpecialLevel.ToString();
        }
    }

    private void UpdateCurrencyText()
    {
        currentCurrencyText.text = currentCurrency.ToString();
        requiredCurrencyText.text = requiredCurrency.ToString();
    }

    private void ConsumeCurrencyValues()
    {
        if (requiredCurrency <= currentCurrency)
            currentCurrency -= requiredCurrency;
    }

    private void FreeCurrencyValues()
    {
        currentCurrency += requiredCurrency;
    }

    private void UpdateIncreaseButtonInteractable()
    {
        if (playerStats == null)
            return;

        if (currentCurrency < requiredCurrency)
        {
            vitalityIncrease.interactable = false;
            efficiencyIncrease.interactable = false;
            strengthIncrease.interactable = false;
            staminaIncrease.interactable = false;
            specialIncrease.interactable = false;
        } else
        {
            if (futureVitalityLevel >= levelCap
                                                        || baseVitalityLevel >= levelCap)
                vitalityIncrease.interactable = false;
            else if (futureVitalityLevel < levelCap
                                                        || baseVitalityLevel < levelCap)
                vitalityIncrease.interactable = true;
        }
    }

    private void UpdateDecreaseButtonInteractable()
    {

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // BUTTON FUNCTIONS FOR TEXT ////////////////////////////////////////////////////////////////////////
    private void ChangeLevelColor(int index) { 
        if (index == 0) {
            if (futureVitalityLevel > baseVitalityLevel)
                vitalityLevelText.color = Color.green;
            else
                vitalityLevelText.color = Color.black;
        }
    }

    public void IncreaseStat(int index)
    {
        Debug.Log("Increasing");

        if (index == 0 && futureVitalityLevel < levelCap || baseVitalityLevel < levelCap) {
            futureCurrentLevel++;
            futureVitalityLevel++;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            ChangeLevelColor(0);
            UpdateIncreaseButtonInteractable();
        }


            
    }

    public void DecreaseStat(int index)
    {
        Debug.Log("Decreasing");

        if (index == 0 && futureVitalityLevel > baseVitalityLevel) {
            futureVitalityLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            ChangeLevelColor(0);
            UpdateIncreaseButtonInteractable();
            UpdateDecreaseButtonInteractable();
        }
    }
}
