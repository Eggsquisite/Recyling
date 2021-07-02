using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthbar : MonoBehaviour
{
    private Slider healthBar;
    public Vector3 offset;

    private void Start()
    {
        if (healthBar == null)
            healthBar = GetComponentInChildren<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (healthBar != null)
            healthBar.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + offset);
    }

    public void SetMaxHealth(int newValue) {
        if (healthBar != null) { 
            healthBar.maxValue = healthBar.value = newValue;
            healthBar.minValue = 0;
        }
    }

    public void SetCurrentHealth(int newValue) {
        healthBar.value -= newValue;
    }
}
