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
    private float updateDamageDelay;
    [SerializeField]
    private float healthVisualSpeed;

    private int tmpDmg;
    private bool updateHealth;
    private Coroutine updateHealthRoutine, updateDamageTextRoutine;

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

        if (updateDamageTextRoutine != null)
            StopCoroutine(updateDamageTextRoutine);
        updateDamageTextRoutine = StartCoroutine(UpdateDamageTextRoutine(damagedHealth));
    }

    IEnumerator UpdateHealthRoutine() {
        // wait for a delay, then begin updating health
        updateHealth = false;

        yield return new WaitForSeconds(updateWaitDelay);

        updateHealth = true;
    }

    IEnumerator UpdateDamageTextRoutine(int damage) {
        tmpDmg += damage;
        var tmp = "-";
        damagedText.text = tmp + tmpDmg.ToString();

        yield return new WaitForSeconds(updateDamageDelay);

        tmpDmg = 0;
        damagedText.text = "";
    }

    private void OnEnable()
    {
        damagedText.text = "";
    }

    private void OnDisable()
    {
        damagedText.text = "";
    }
}
