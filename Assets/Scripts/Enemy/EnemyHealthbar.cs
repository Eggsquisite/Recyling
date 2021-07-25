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

    private float totalHealthPercent;
    private int maxHealth;
    private int currentHealth;
    private bool updateHealth;
    private bool facingLeft;

    private Coroutine waitTimeRoutine;

    private void Awake()
    {
        if (healthFill != null)
            totalHealthPercent = healthFill.localScale.x;

        if (transform.localScale.x >= 0)
            facingLeft = false;
        else
            facingLeft = true;
    }

    private void Start()
    {
        if (healthFill != null && healthDestroy != null) { 
            healthFill.localScale = new Vector3(healthFill.localScale.x, healthFill.localScale.y);
            healthDestroy.localScale = new Vector3(healthDestroy.localScale.x, healthDestroy.localScale.y);
        }
    }

    private void Update()
    {
        // change direction of health bar
        if (healthFill != null) { 
            if (transform.localScale.x > 0 && facingLeft) {
                facingLeft = false;
                Debug.Log(name + " is facing right");
                healthBarParent.localScale = new Vector3(Mathf.Abs(healthBarParent.localScale.x), healthBarParent.localScale.y);
            }
            else if (transform.localScale.x < 0 && !facingLeft) {
                facingLeft = true;
                Debug.Log(name + " is facing left");
                healthBarParent.localScale = new Vector3(-Mathf.Abs(healthBarParent.localScale.x), healthBarParent.localScale.y);
            }
        }

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

    public void SetMaxHealth(int newValue) {
        maxHealth = currentHealth = newValue;
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        healthFill.localScale = new Vector3(totalHealthPercent, healthFill.localScale.y);
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

    public void UpdateHealthbarDirection() { 
        if (healthFill != null)
            if (transform.localScale.x > 0) {
                facingLeft = true;
                healthBarParent.localScale = new Vector3(Mathf.Abs(healthBarParent.localScale.x), healthBarParent.localScale.y);
            }
            else if (transform.localScale.x < 0) {
                facingLeft = false;
                healthBarParent.localScale = new Vector3(-Mathf.Abs(healthBarParent.localScale.x), healthBarParent.localScale.y);
            }
    }
}
