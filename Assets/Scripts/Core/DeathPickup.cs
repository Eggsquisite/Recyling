using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPickup : MonoBehaviour
{
    private int currencyStored;

    public void InteractActivated() {
        Player.instance.GetComponent<PlayerUI>().SetCurrency(currencyStored);
    }

    public void SetCurrencyStored(int newValue) {
        currencyStored = newValue;
    }

    public int GetCurrencyStored() {
        return currencyStored;
    }
}
