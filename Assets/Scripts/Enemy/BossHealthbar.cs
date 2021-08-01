using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthbar : MonoBehaviour
{
    [SerializeField]
    private BossHealthUI bossHealthUI;

    // Start is called before the first frame update
    void Start()
    {
        SetHealthbar(false);
    }

    public void SetHealthbar(bool flag) {
        bossHealthUI.gameObject.SetActive(flag);
    }

    public void SetMaxHealth(int maxHealth) {
        bossHealthUI.SetMaxHealth(maxHealth);
    }

    public void UpdateHealth(int damagedHealth) {
        bossHealthUI.UpdateHealth(damagedHealth);
    }
}
