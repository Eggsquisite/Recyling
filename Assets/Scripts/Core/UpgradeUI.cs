using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("Components")]
    // CAN ENABLE USE OF COMPONENTS THROUGH EVENTS //////////////// ******************************
    private FindPlayerScript findPlayer;
    private PlayerStats playerStats;

    [Header("Currency Text")]
    [SerializeField]
    private Text currentCurrencyText;
    [SerializeField]
    private Text requiredCurrencyText;

    private int baseCurrency = 100;
    private int futureCurrency;
    private int requiredCurrency = 10;
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

    private bool isConfirmed;
    private int upgradeCounter;

    [Header("Increase Buttons")]
    [SerializeField]
    private Button confirm;
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
        if (findPlayer == null) findPlayer = GetComponent<FindPlayerScript>();
        if (findPlayer != null) playerStats = findPlayer.GetPlayerStats();

        isConfirmed = false;
        UpdateCurrencyValues();
        UpdateCurrencyText();

        UpdateLevelValues();
        UpdateLevelText(0);

        UpdateButtonInteractable();
    }

    void OnDisable()
    {
        if (!isConfirmed)
            Cancel();
    }

    void Start()
    {
        if (findPlayer == null) findPlayer = GetComponent<FindPlayerScript>();
        if (findPlayer != null) playerStats = findPlayer.GetPlayerStats();

        isConfirmed = false;
        UpdateCurrencyValues();
        UpdateCurrencyText();

        UpdateLevelValues();
        UpdateLevelText(0);

        UpdateButtonInteractable();
    }

    private void UpdateLevelValues()
    {
        if (playerStats == null)
            return;

        upgradeCounter = 0;
        CheckUpgradeCount(0);
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

    private void UpdateCurrencyValues()
    {
        futureCurrency = baseCurrency;
    }

    private void UpdateCurrencyText()
    {
        currentCurrencyText.text = futureCurrency.ToString();
        requiredCurrencyText.text = requiredCurrency.ToString();
    }

    private void ConsumeCurrencyValues()
    {
        if (requiredCurrency <= futureCurrency)
            futureCurrency -= requiredCurrency;
    }

    private void FreeCurrencyValues()
    {
        futureCurrency += requiredCurrency;
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
        if (futureCurrency < requiredCurrency)
        {
            vitalityIncrease.interactable = false;
            efficiencyIncrease.interactable = false;
            strengthIncrease.interactable = false;
            staminaIncrease.interactable = false;
            specialIncrease.interactable = false;
        } else
        {
            if (futureVitalityLevel >= levelCap)
                vitalityIncrease.interactable = false;
            else 
                vitalityIncrease.interactable = true;

            if (futureEfficiencyLevel >= levelCap)
                efficiencyIncrease.interactable = false;
            else 
                efficiencyIncrease.interactable = true;

            if (futureStrengthLevel >= levelCap)
                strengthIncrease.interactable = false;
            else 
                strengthIncrease.interactable = true;

            if (futureStaminaLevel >= levelCap)
                staminaIncrease.interactable = false;
            else
                staminaIncrease.interactable = true;

            if (futureSpecialLevel >= levelCap)
                specialIncrease.interactable = false;
            else
                specialIncrease.interactable = true;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // BUTTON FUNCTIONS FOR TEXT ////////////////////////////////////////////////////////////////////////
    private void ChangeLevelColor() { 
        if (futureVitalityLevel > baseVitalityLevel)
            vitalityLevelText.color = Color.green;
        else
            vitalityLevelText.color = Color.black;

        if (futureEfficiencyLevel > baseEfficiencyLevel)
            efficiencyLevelText.color = Color.green;
        else
            efficiencyLevelText.color = Color.black;

        if (futureStrengthLevel > baseStrengthLevel)
            strengthLevelText.color = Color.green;
        else
            strengthLevelText.color = Color.black;

        if (futureStaminaLevel > baseStaminaLevel)
            staminaLevelText.color = Color.green;
        else
            staminaLevelText.color = Color.black;

        if (futureSpecialLevel > baseSpecialLevel)
            specialLevelText.color = Color.green;
        else
            specialLevelText.color = Color.black;
    }

    public void IncreaseStat(int index)
    {
        CheckUpgradeCount(1);
        isConfirmed = false;
        if (index == 0 && futureVitalityLevel < levelCap) {
            futureCurrentLevel += 1;
            futureVitalityLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 1 && futureEfficiencyLevel < levelCap)
        {
            futureCurrentLevel += 1;
            futureEfficiencyLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 2 && futureStrengthLevel < levelCap)
        {
            futureCurrentLevel += 1;
            futureStrengthLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 3 && futureStaminaLevel < levelCap)
        {
            futureCurrentLevel += 1;
            futureStaminaLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 4 && futureSpecialLevel < levelCap)
        {
            futureCurrentLevel += 1;
            futureSpecialLevel += 1;

            ConsumeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        }

        ChangeLevelColor();
    }

    public void DecreaseStat(int index)
    {
        CheckUpgradeCount(-1);
        if (index == 0 && futureVitalityLevel > baseVitalityLevel) {
            futureVitalityLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 1 && futureEfficiencyLevel > baseEfficiencyLevel)
        {
            futureEfficiencyLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 2 && futureStrengthLevel > baseStrengthLevel)
        {
            futureStrengthLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 3 && futureStaminaLevel > baseStaminaLevel)
        {
            futureStaminaLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        } else if (index == 4 && futureSpecialLevel > baseSpecialLevel)
        {
            futureSpecialLevel -= 1;
            futureCurrentLevel -= 1;

            FreeCurrencyValues();
            UpdateCurrencyText();

            UpdateLevelText(1);
            UpdateButtonInteractable();
        }

        ChangeLevelColor();
    }

    private void CheckUpgradeCount(int value)
    {
        upgradeCounter += value;

        if (upgradeCounter > 0)
            confirm.interactable = true;
        else
            confirm.interactable = false;
    }

    public void Confirm() 
    {
        isConfirmed = true;
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

        // also implement Player script having currency values
        baseCurrency = futureCurrency;

        UpdateLevelValues();
        UpdateLevelText(0);
        ChangeLevelColor();
        UpdateButtonInteractable();
    }

    public void Cancel()
    {
        futureCurrentLevel = baseCurrentLevel;
        futureVitalityLevel = baseVitalityLevel;
        futureEfficiencyLevel = baseEfficiencyLevel;
        futureStrengthLevel = baseStrengthLevel;
        futureStaminaLevel = baseStaminaLevel;
        futureSpecialLevel = baseSpecialLevel;

        futureCurrency = baseCurrency;

        UpdateLevelValues();
        UpdateLevelText(0);
        ChangeLevelColor();
        UpdateButtonInteractable();
    }
}
