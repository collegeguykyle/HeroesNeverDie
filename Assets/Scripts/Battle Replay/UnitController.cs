using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public int startingRow;
    public int startingColumn;

    public float moveSpeed = 1.0f; // Speed at which to lerp
    private Vector3 targetPosition; // The target position to lerp towards
    private bool isLerping = false; // Whether the object is currently moving
    private bool startLerp = true;

    void Update()
    {
        if (startLerp)
        {
            startLerp = false;
            StartLerpToCell(startingRow, startingColumn);
        }
        // If lerping is active, move the object
        if (isLerping)
        {
            // Lerp the position towards the target
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if the object is close enough to the target (to stop the lerp)
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition; // Snap to the target
                isLerping = false; // Stop lerping
            }
        }
    }

    // Method to start lerping to a specific cell (row and column)
    public void StartLerpToCell(int row, int column)
    {
        // Get the world coordinates of the target cell
        targetPosition = GridGenerator.Instance.GetWorldCoordinates(row, column);

        // If a valid position is returned (not Vector3.zero), start lerping
        if (targetPosition != Vector3.zero)
        {
            isLerping = true; // Begin lerping
        }
        else
        {
            Debug.LogError("Invalid row or column provided.");
        }
    }
}
