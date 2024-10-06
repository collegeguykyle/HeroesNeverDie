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
    public bool MoveToMouse = false;
    // Dictionary to store tooltip data, keyed by unique IDs (for flexibility with different types)
    private Dictionary<string, string> dynamicTooltips = new Dictionary<string, string>();

    // The battle log text history
    private string logHistory = "";

    private string currentHoveredKeyword = null;

    private void Start()
    {
        tooltipController.HideTooltip();
        AddTestText();
    }

    private void AddTestText()
    {
        LogWithTooltip(new KW_Hit(100, "Stun", false), "Someone hit with a stun ability.");
        LogWithTooltip(new KW_Hit(20, "none", true), "Hit,");
        LogWithTooltip(new KW_Hit(73, "Blah blah blah hit", false), "another hit.");

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
        DetectKeyWords();
    }

    public void DetectKeyWords()
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

    // Helper method to add plain text to the battle log
    public void AddTextToBattleLog(string message)
    {
        battleLogText.text += message + "\n";
    }

    public void LogWithTooltip(IToolTipKeyWord tooltipData, string message)
    {
        // Generate a unique ID for this tooltip
        string uniqueID = $"tooltip_{dynamicTooltips.Count}";
        dynamicTooltips[uniqueID] = tooltipData.GetTooltipText();

        // Append the message to the battle log, replacing dynamic text with a <link>
        string logEntry = $"<link={uniqueID}><color=yellow>{message}</color></link>";

        battleLogText.text += logEntry + "\n";
    }

    private void ShowTooltip(string linkID)
    {
        if (dynamicTooltips.ContainsKey(linkID))
        {
            // Get the tooltip content from the tooltipData
            string tooltipContent = dynamicTooltips[linkID];

            // Display the tooltip using the TooltipManager
            tooltipController.ShowTooltip(tooltipContent);
        }

        if (MoveToMouse)// Move the tooltip box to follow the mouse
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(contentTransform, Input.mousePosition, null, out localPoint);
            localPoint.x = tooltipController.tooltipBox.localPosition.x;
            localPoint.y += 30;
            tooltipController.tooltipBox.localPosition = localPoint;
        }
        
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




