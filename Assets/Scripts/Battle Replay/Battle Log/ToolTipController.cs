using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;  // The TextMeshPro component for the tooltip content
    public RectTransform tooltipBox;     // The RectTransform of the tooltip box

    public float topBuffer = 10f;  // Padding at the top
    public float bottomBuffer = 10f;  // Padding at the bottom

    private void UpdateTooltipHeight()
    {
        // Calculate the required height based on the text content
        float textHeight = tooltipText.GetPreferredValues().y;  // Get the preferred height of the text

        // Adjust the height of the tooltip box based on text height and buffers
        tooltipBox.sizeDelta = new Vector2(tooltipBox.sizeDelta.x, textHeight + topBuffer + bottomBuffer);
    }

    public void ShowTooltip(string content)
    {
        // Set the text content
        tooltipText.text = content;

        // Update the height of the tooltip box based on the content
        UpdateTooltipHeight();

        // Make sure the tooltip box is active and displayed
        tooltipBox.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        // Hide the tooltip box
        tooltipBox.gameObject.SetActive(false);
    }
}
