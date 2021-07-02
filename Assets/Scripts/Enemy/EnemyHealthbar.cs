using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthbar : MonoBehaviour
{
    [SerializeField]
    private Transform healthBar;

    private int maxHealth;
    private int currentHealth;

    private void Start()
    {
        if (healthBar != null)
            healthBar.localScale = new Vector3(1f, healthBar.localScale.y);
    }

    private void Update()
    {
/*        if (healthBar != null)
            if (healthBar.parent.localScale.x < 0)
                healthBar.localScale = new Vector3(-healthBar.localScale.x, healthBar.localScale.y);
            else
                healthBar.localScale = new Vector3(healthBar.localScale.x, healthBar.localScale.y);*/
    }

    public void SetMaxHealth(int newValue) {
        maxHealth = currentHealth = newValue;
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        healthBar.localScale = new Vector3(1f, healthBar.localScale.y);
    }

    public void SetCurrentHealth(int newValue) {
        currentHealth -= newValue;
        if (currentHealth < 0)
            currentHealth = 0;

        float tmpCurrent = currentHealth;
        float tmpMax = maxHealth;
        healthBar.localScale = new Vector3(tmpCurrent / tmpMax, healthBar.localScale.y);
    }
}
