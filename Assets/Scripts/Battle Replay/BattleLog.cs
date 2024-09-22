using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleLogWithDynamicTooltips : MonoBehaviour
{
    public TextMeshProUGUI battleLogText;  // Reference to your battle log TextMeshProUGUI
    public GameObject tooltipBox;          // Reference to the tooltip UI panel
    public TextMeshProUGUI tooltipText;    // Reference to the TextMeshProUGUI in the tooltip box
    public RectTransform canvasRect;       // Reference to the canvas RectTransform

    // Dictionary to store tooltip data, keyed by unique IDs (for flexibility with different types)
    private Dictionary<string, object> dynamicTooltips = new Dictionary<string, object>();

    // The battle log text history
    private string logHistory = "";

    private string currentHoveredKeyword = null;

    private void Start()
    {
        tooltipBox.SetActive(false); // Initially hide the tooltip box
        AddToBattleLog( new HitResult(100, "Stun", false) );
        AddToBattleLog( new HitResult(20, "none", true) );
        AddToBattleLog(new HitResult(73, "Blah blah blah hit", false));
    }

    private void Update()
    {
        // Detect if the mouse is hovering over a specific link (keyword) in the text
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(battleLogText, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = battleLogText.textInfo.linkInfo[linkIndex];
            string hoveredKeyword = linkInfo.GetLinkID();

            // If a new keyword is hovered, show its tooltip
            if (hoveredKeyword != currentHoveredKeyword)
            {
                currentHoveredKeyword = hoveredKeyword;
                ShowTooltip(hoveredKeyword);
            }
        }
        else
        {
            HideTooltip(); // Hide tooltip when not hovering over any keyword
        }
    }

    // Helper method to add dynamic tooltip data and a generic message to the log
    public void AddToBattleLog(IBattleLogTooltip tooltipData)
    {
        // Automatically assign a unique ID based on the current number of dynamic tooltips
        string uniqueID = $"log_{dynamicTooltips.Count}";

        // Add the tooltip data to the dictionary with its unique ID
        dynamicTooltips[uniqueID] = tooltipData;

        // Get the battle log entry text from the class, with the {keyword} placeholder
        string logEntry = tooltipData.GetLogEntryText();

        // Replace the {keyword} placeholder with the actual link and color
        logEntry = logEntry.Replace(tooltipData.Keyword, 
            $"<link={uniqueID}><color={tooltipData.TextColor}>{tooltipData.Keyword}</color></link>");

        // Append the log entry to the log history
        logHistory += logEntry + "\n";

        // Update the displayed battle log
        UpdateDisplayedLog();
    }

    // Method to update the displayed battle log
    private void UpdateDisplayedLog()
    {
        battleLogText.text = logHistory; // Display the full battle log
    }

    private void ShowTooltip(string keyword)
    {
        if (dynamicTooltips.ContainsKey(keyword))
        {
            // Check if the tooltip data is a HitResult or other type and display accordingly
            if (dynamicTooltips[keyword] is HitResult hitResult)
            {
                tooltipText.text = hitResult.GetTooltipText();
            }
            else if (dynamicTooltips[keyword] is string otherData)
            {
                tooltipText.text = otherData; // For example, if it's a string or other data type
            }

            tooltipBox.SetActive(true);
        }

        // Move the tooltip box to follow the mouse
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPoint);
        tooltipBox.GetComponent<RectTransform>().localPosition = localPoint;
    }

    private void HideTooltip()
    {
        tooltipBox.SetActive(false);
        currentHoveredKeyword = null;
    }
}

public interface IBattleLogTooltip
{
    string GetTooltipText();  // Returns the text for the tooltip
    string Keyword { get; }   // Returns the keyword associated with the log entry
    string TextColor { get; } // Returns the color for the keyword in the log (as a hex code or name)
    string GetLogEntryText(); // Returns the text that should appear in the battle log
}

// Example class for HitResult, can be extended with other tooltip types
public class HitResult : IBattleLogTooltip
{
    public string Keyword => "hit";
    public string TextColor => "#FF0000";  // Red color

    public int damage;
    public string statusEffect;
    public bool isCritical;

    public HitResult(int damage, string statusEffect, bool isCritical)
    {
        this.damage = damage;
        this.statusEffect = statusEffect;
    }

    // Text that will appear in the tool tip
    public string GetTooltipText()
    {
        return $"Damage: {damage}\n" +
            $"Status: {statusEffect}\n" +
            $"Critical Hit: {isCritical}";
    }
    // The text that will appear in the battle log
    public string GetLogEntryText()
    {
        return $"Unit_Name was {Keyword} for {damage} damage!";
    }


}
