using System.Collections.Generic;
using System.Dynamic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance;
    public int rows = 9;
    public int columns = 5;
    public float squareWidth = 1f;
    public float squareHeight = 1f;
    public float spacing = 0.1f;
    public GameObject squarePrefab;

    // List to store information about each square in the grid
    private List<SquareInfo> squareInfos = new List<SquareInfo>();

    // This class holds information about each square
    public class SquareInfo
    {
        public int row;
        public int column;
        public RectTransform rect;

        public SquareInfo(int row, int column, RectTransform rectTransform)
        {
            this.row = row;
            this.column = column;
            this.rect = rectTransform;
        }

        public Vector3 GetWorldPosition()
        {
            return rect.position;
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    // Generates the grid of squares
    public void GenerateGrid()
    {
        ClearGrid(); // Clear existing squares and data
        squareInfos.Clear();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(squarePrefab, transform);
                cell.name = $"Cell_{row}_{col}";

                RectTransform rectTransform = cell.GetComponent<RectTransform>();

                Cell_Info info = cell.GetComponent<Cell_Info>();
                info.col = col;
                info.row = row;

                // Store the cell's row, column, and RectTransform (for world position)
                squareInfos.Add(new SquareInfo(row, col, rectTransform));
            }
        }
    }

    // Clears the grid by destroying all existing squares
    public void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
        squareInfos.Clear();
    }

    // Method to get the world-space coordinates of a square by its row and column
    public Vector3 GetWorldCoordinateOld(int row, int column)
    {
        foreach (SquareInfo squareInfo in squareInfos)
        {
            if (squareInfo.row == row && squareInfo.column == column)
            {
                return squareInfo.GetWorldPosition();
            }
        }

        Debug.LogError($"Square not found at row {row}, column {column}");
        return Vector3.zero;
    }

    public Vector3 GetWorldCoordinates(int row, int column)
    {
        foreach (Cell_Info Info in GetComponentsInChildren<Cell_Info>())
        {
            if (Info.row == row && Info.col == column)
            {
                return Info.getWorldPosition();
            }
        }

        Debug.LogError($"Square not found at row {row}, column {column}");
        return Vector3.zero;
    }

}
