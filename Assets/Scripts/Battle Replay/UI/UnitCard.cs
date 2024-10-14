using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCard : MonoBehaviour
{
    public Unit unit;

    public Image unitSprite;

    public Image healthBar; // Reference to the UI Image for the health bar
    public TextMeshProUGUI healthText; // Reference to the TextMeshPro component for displaying health

    public int maxHealth;
    public int currentHealth;

    private void Update()
    {
        UpdateHealthDisplay(); //Dosen't need to be in update, but rather just called via events
    }

    // Initialize the health display
    public void Initialize(Unit newUnit)
    {
        unit = newUnit;
        maxHealth = unit.MaxHP;
        //currentHealth = unit.currentHP;
        UpdateHealthDisplay();
        //unitSprite.sprite = unit.GetComponent<SpriteRenderer>().sprite;
    }

    // Call this method to update the unit's health
    public void UpdateHealth(int newHealth)
    {
        currentHealth = newHealth;
        UpdateHealthDisplay();
    }

    // Update the health bar UI and text
    private void UpdateHealthDisplay()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        healthBar.fillAmount = 1 - healthPercent;
        healthText.text = currentHealth + " / " + maxHealth;
    }

    public void ChangeCardColor(Color color)
    {
        unitSprite.color = color;
    }
}
