using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleLogWithDynamicTooltips : MonoBehaviour
{
    public TextMeshProUGUI battleLogText;  // Reference to your battle log TextMeshProUGUI
    public TooltipController tooltipController;
    public RectTransform canvasRect;       // Reference to the canvas RectTransform
    public RectTransform scrollRectTransform; // Reference to the ScrollRect's RectTransform for visible area checking
    public RectTransform contentTransform;    // Reference to the content RectTransform that holds the battle log text

    // Dictionary to store tooltip data, keyed by unique IDs (for flexibility with different types)
    private Dictionary<string, IBattleLogTooltip> dynamicTooltips = new Dictionary<string, IBattleLogTooltip>();

    // The battle log text history
    private string logHistory = "";

    private string currentHoveredKeyword = null;

    private void Start()
    {
        tooltipController.HideTooltip();
        AddToBattleLog( new HitResult(100, "Stun", false) );
        AddToBattleLog( new HitResult(20, "none", true) );
        AddToBattleLog(new HitResult(73, "Blah blah blah hit", false));

        AddTextToBattleLog("Changes to Integrate TooltipManager:\r\n\r\n    " +
            "Reference to TooltipManager:\r\n        Added a reference to TooltipManager in the " +
            "BattleLogWithDynamicTooltips class to handle displaying tooltips and adjusting their size " +
            "dynamically.\r\n\r\n    ShowTooltip() Method:\r\n        Instead of directly manipulating " +
            "the tooltip in the battle log class, we now call tooltipManager.ShowTooltip(tooltipContent) " +
            "to display the tooltip. The TooltipManager automatically adjusts the size of the tooltip box " +
            "based on the content.\r\n\r\n    HideTooltip():\r\n        The HideTooltip() method of " +
            "TooltipManager is called to hide the tooltip when the mouse is not hovering over any link.\r\n\r\n    " +
            "Tooltip Positioning:\r\n        The tooltip box position is still updated to follow the mouse " +
            "using localPosition, but the actual display of the tooltip is now handled by TooltipManager." +
            "\r\n\r\nExample TooltipManager Class:\r\n\r\nHere’s the TooltipManager class, as we discussed " +
            "earlier, for handling the tooltip display and dynamic sizing:");
    }

    private void Update()
    {
        // Detect if the mouse is hovering over a specific link (keyword) in the text
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(battleLogText, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = battleLogText.textInfo.linkInfo[linkIndex];
            string hoveredKeyword = linkInfo.GetLinkID();

            // Check if the hovered text is visible within the ScrollRect's viewport
            if (IsTextVisible(linkInfo))
            {
                // If a new keyword is hovered, show its tooltip
                if (hoveredKeyword != currentHoveredKeyword)
                {
                    currentHoveredKeyword = hoveredKeyword;
                    ShowTooltip(hoveredKeyword);
                }
            }
            else
            {
                tooltipController.HideTooltip();  // Hide the tooltip if text is not visible
                currentHoveredKeyword = null;
            }
        }
        else
        {
            tooltipController.HideTooltip();  // Hide tooltip when not hovering over any keyword
            currentHoveredKeyword = null;
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

    // Helper method to add plain text to the battle log
    public void AddTextToBattleLog(string message)
    {
        // Append the plain text to the log history
        logHistory += message + "\n";

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
            // Get the tooltip content from the tooltipData
            string tooltipContent = dynamicTooltips[keyword].GetTooltipText();

            // Display the tooltip using the TooltipManager
            tooltipController.ShowTooltip(tooltipContent);
        }

        // Move the tooltip box to follow the mouse
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(contentTransform, Input.mousePosition, null, out localPoint);
        localPoint.x = tooltipController.tooltipBox.localPosition.x;
        localPoint.y += 30;
        tooltipController.tooltipBox.localPosition = localPoint;
    }

    // Check if the hovered text is visible inside the ScrollRect
    private bool IsTextVisible(TMP_LinkInfo linkInfo)
    {
        // Get the first and last character info for the link
        int firstCharacterIndex = linkInfo.linkTextfirstCharacterIndex;
        int lastCharacterIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength - 1;

        // Get the bottom left position of the first character and the top right of the last character
        TMP_CharacterInfo firstCharInfo = battleLogText.textInfo.characterInfo[firstCharacterIndex];
        TMP_CharacterInfo lastCharInfo = battleLogText.textInfo.characterInfo[lastCharacterIndex];

        // Transform the character positions to local space
        Vector3 bottomLeft = battleLogText.transform.TransformPoint(firstCharInfo.bottomLeft);
        Vector3 topRight = battleLogText.transform.TransformPoint(lastCharInfo.topRight);

        // Get the local midpoint of the link
        Vector3 linkMidPoint = (bottomLeft + topRight) / 2;

        // Convert the midpoint to local space of the viewport
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRectTransform, RectTransformUtility.WorldToScreenPoint(null, linkMidPoint), null, out localPosition);

        // Get the local bounds of the viewport (use yMin and yMax for vertical scrolling)
        float viewportTop = scrollRectTransform.rect.yMax;
        float viewportBottom = scrollRectTransform.rect.yMin;

        // Check if the link's midpoint is between the top and bottom of the ScrollRect's viewport
        return (localPosition.y <= viewportTop && localPosition.y >= viewportBottom);
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
