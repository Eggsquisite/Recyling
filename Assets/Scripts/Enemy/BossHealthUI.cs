using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField]
    private Slider futureHealthSlider;
    [SerializeField]
    private Slider currentHealthSlider;
    [SerializeField]
    private TMPro.TextMeshProUGUI damagedText;

    [SerializeField]
    private float updateWaitDelay;
    [SerializeField]
    private float healthVisualSpeed;

    private bool updateHealth;
    private Coroutine updateHealthRoutine;

    private void Update()
    {
        if (updateHealth)
        {
            if (currentHealthSlider.value < futureHealthSlider.value)
                futureHealthSlider.value -= healthVisualSpeed * Time.deltaTime;
            else if (currentHealthSlider.value >= futureHealthSlider.value) {
                futureHealthSlider.value = currentHealthSlider.value;
                updateHealth = false;
            }
        }
    }

    public void SetMaxHealth(int maxHealth) {
        currentHealthSlider.value = currentHealthSlider.maxValue = maxHealth;
        futureHealthSlider.value = futureHealthSlider.maxValue = maxHealth;
    }

    public void UpdateHealth(int damagedHealth) {
        Debug.Log("Updating boss health");
        currentHealthSlider.value -= damagedHealth;

        if (updateHealthRoutine != null)
            StopCoroutine(updateHealthRoutine);
        updateHealthRoutine = StartCoroutine(UpdateHealthRoutine());
    }

    IEnumerator UpdateHealthRoutine() {
        // wait for a delay, then begin updating health
        updateHealth = false;
        yield return new WaitForSeconds(updateWaitDelay);
        updateHealth = true;
    }
}
