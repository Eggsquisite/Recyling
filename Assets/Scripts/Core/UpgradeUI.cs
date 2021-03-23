using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Components")]
    // CAN ENABLE USE OF COMPONENTS THROUGH EVENTS //////////////// ******************************
    private FindPlayerScript player;
    private PlayerStats playerStats;

    [Header("Currency Text")]
    [SerializeField]
    private Text currentCurrencyText;
    [SerializeField]
    private Text requiredCurrencyText;

    private int currentCurrency = 100;
    private int requiredCurrency = 1;
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

    [Header("Decrease Buttons")]
    [SerializeField]
    private Button vitalityDecrease;
    [SerializeField]
    private Button efficiencyDecrease;
    [SerializeField]
    private Button strengthDecrease;
    [SerializeField]
    private Button staminaDecrease;
    [SerializeField]
    private Button specialDecrease;

    void OnEnable()
    {
        UpdateCurrencyText();
        UpdateLevelValues();
        UpdateLevelText(0);
        UpdateButtonInteractable();
    }

    void Start()
    {
        if (player == null) player = GetComponent<FindPlayerScript>();
        if (player != null) playerStats = player.GetComponent<PlayerStats>();

        UpdateCurrencyText();
        UpdateLevelValues();
        UpdateLevelText(0);
        UpdateButtonInteractable();
    }

    private void UpdateLevelValues()
    {
        if (playerStats == null)
            return;

        levelCap = playerStats.GetLevelCap();
        baseCurrentLevel = futureCurrentLevel = playerStats.GetPlayerLevel();
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

    private void UpdateButtonInteractable()
    {
        // DECREASE BUTTONS
        if (futureVitalityLevel == baseVitalityLevel)
            vitalityDecrease.interactable = false;
        else if (futureVitalityLevel > baseVitalityLevel)
            vitalityDecrease.interactable = true;

        if (futureEfficiencyLevel == baseEfficiencyLevel)
            efficiencyDecrease.interactable = false;
        else if (futureEfficiencyLevel > baseEfficiencyLevel)
            efficiencyDecrease.interactable = true;

        if (futureStrengthLevel == baseStrengthLevel)
            strengthDecrease.interactable = false;
        else if (futureStrengthLevel > baseStrengthLevel)
            strengthDecrease.interactable = true;

        if (futureStaminaLevel == baseStaminaLevel)
            staminaDecrease.interactable = false;
        else if (futureStaminaLevel > baseStaminaLevel)
            staminaDecrease.interactable = true;

        if (futureSpecialLevel == baseSpecialLevel)
            specialDecrease.interactable = false;
        else if (futureSpecialLevel > baseSpecialLevel)
            specialDecrease.interactable = true;

        // INCREASE BUTTONS
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

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // BUTTON FUNCTIONS FOR TEXT ////////////////////////////////////////////////////////////////////////
    private void ChangeLevelColor() { 
        if (futureVitalityLevel > baseVitalityLevel)
            vitalityLevelText.color = Color.green;
        else
            vitalityLevelText.color = Color.black;
        
    }

    public void IncreaseStat(int index)
    {
        if (index == 0 && futureVitalityLevel < levelCap && baseVitalityLevel < levelCap) {
            futureCurrentLevel += 1;
            futureVitalityLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        }


            
        ChangeLevelColor();
    }

    public void DecreaseStat(int index)
    {
        if (index == 0 && futureVitalityLevel > baseVitalityLevel) {
            futureVitalityLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        }

        ChangeLevelColor();
    }

    public void Confirm() 
    {
        // player total level will update through IncreaseStat()
        baseVitalityLevel = futureVitalityLevel;
        baseEfficiencyLevel = futureEfficiencyLevel;
        baseStrengthLevel = futureStrengthLevel;
        baseStaminaLevel = futureStaminaLevel;
        baseSpecialLevel = futureSpecialLevel;

        playerStats.IncreaseStat(0, baseVitalityLevel);
        playerStats.IncreaseStat(1, baseEfficiencyLevel);
        playerStats.IncreaseStat(2, baseStrengthLevel);
        playerStats.IncreaseStat(3, baseStaminaLevel);
        playerStats.IncreaseStat(4, baseSpecialLevel);

        UpdateLevelValues();
        UpdateLevelText(0);
        ChangeLevelColor();
    }
}
