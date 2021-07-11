using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthbar : MonoBehaviour
{
    [SerializeField]
    private Transform healthBarParent;
    [SerializeField]
    private Transform healthFill;
    [SerializeField]
    private Transform healthDestroy;

    [SerializeField]
    private float waitTime;
    [SerializeField]
    private float visualDestroySpeed;
    [SerializeField]
    private bool isBoss;

    private int maxHealth;
    private int currentHealth;
    private bool updateHealth;

    private Coroutine waitTimeRoutine;

    private void Start()
    {
        if (healthFill != null && healthDestroy != null) { 
            healthFill.localScale = new Vector3(1f, healthFill.localScale.y);
            healthDestroy.localScale = new Vector3(1f, healthDestroy.localScale.y);
        }

        if (isBoss) {
            //healthBarParent.parent = null;
        }
    }

    private void Update()
    {
/*        if (healthFill != null)
            if (healthFill.parent.localScale.x < 0)
                healthFill.localScale = new Vector3(-healthFill.localScale.x, healthFill.localScale.y);
            else
                healthFill.localScale = new Vector3(healthFill.localScale.x, healthFill.localScale.y);*/

        if (updateHealth) { 
            if (healthDestroy.localScale.x > healthFill.localScale.x) {
                healthDestroy.localScale = new Vector3(healthDestroy.localScale.x - visualDestroySpeed * Time.deltaTime, 
                                                        healthDestroy.localScale.y);
            } else { 
                healthDestroy.localScale = new Vector3(healthFill.localScale.x, healthDestroy.localScale.y);
                updateHealth = false;
            }
        }
    }

    public bool GetIsBoss() {
        return isBoss;
    }

    public void SetMaxHealth(int newValue) {
        maxHealth = currentHealth = newValue;
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        healthFill.localScale = new Vector3(1f, healthFill.localScale.y);
    }

    public void SetCurrentHealth(int newValue) {
        currentHealth -= newValue;
        if (currentHealth < 0)
            currentHealth = 0;

        float tmpCurrent = currentHealth;
        float tmpMax = maxHealth;
        healthFill.localScale = new Vector3(tmpCurrent / tmpMax, healthFill.localScale.y);

        if (waitTimeRoutine != null)
            StopCoroutine(waitTimeRoutine);
        waitTimeRoutine = StartCoroutine(UpdateHealthVisual());
    }

    private IEnumerator UpdateHealthVisual() {
        updateHealth = false;
        yield return new WaitForSeconds(waitTime);
        updateHealth = true;
    }
}
