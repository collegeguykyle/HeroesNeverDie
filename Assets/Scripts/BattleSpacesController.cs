using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattleSpacesController
{

    private BattleSpace[,] battleSpaces = new BattleSpace[9,3];  //[row, col]
    public BattleSpacesController()
    {
        CreateBattleSpaces();
    }

    public bool TryOccupySpace(IOccupyBattleSpace entity, BattleSpace space)
    {
        if (IsSpaceOccupied(space.row, space.col))
        {
            Debug.Log($"BattleSpaceController attempted to place {entity} at row: {space.row}, col: {space.col}, but {WhoOccupiesSpace(space.row, space.col)} was already there");
            return false;
        }
        else
        {
            battleSpaces[space.row, space.col].isInSpace = entity;
            return true;
        }
    }
    public bool TryOccupySpace(IOccupyBattleSpace entity, int row, int col)
    {
        return TryOccupySpace(entity, GetBattleSpaceAt(row, col));
    }

    public void PlaceEnemyTeam(List<Unit> enemyTeam)
    {
        //the enemy team needs to be rotated around and then placed in the top 3 rows
        foreach (Unit unit in enemyTeam)
        {
            if (unit.StartingCol == 1) unit.StartingCol = 3;
            else if (unit.StartingCol == 3) unit.StartingCol = 1;
            if (unit.StartingRow == 1) unit.StartingRow = 3;
            else if (unit.StartingRow == 3) unit.StartingRow = 1;

            TryOccupySpace(unit, unit.StartingRow, unit.StartingCol);
        }
    }

    public void PlacePlayerTeam(List<Unit> playerTeam)
    {
        foreach(Unit unit in playerTeam)
        {
            if (unit.StartingRow == 1) unit.StartingRow = 7;
            else if (unit.StartingRow == 2) unit.StartingRow = 8;
            else if (unit.StartingRow == 3) unit.StartingRow = 9;

            TryOccupySpace(unit, unit.StartingRow, unit.StartingCol);
        }

    }

    public void FindFreeSpace(IOccupyBattleSpace entity)
    {
        //[ ] cycle through all the spaces to find one that is free for the unit to go into based on its Team
    }

    public bool IsSpaceOccupied(int Row, int Col)
    {
        if (battleSpaces[Row-1, Col-1].isInSpace == null) return false;
        else return true;
    }

    public IOccupyBattleSpace WhoOccupiesSpace(int Row, int Col)
    {
        if (battleSpaces[Row-1, Col-1].isInSpace == null) return null;
        else return battleSpaces[Row-1, Col-1].isInSpace;
    }
    public BattleSpace GetBattleSpaceAt(int Row, int Col)
    {
        if (battleSpaces[Row - 1, Col - 1] == null) return null;
        else return battleSpaces[Row - 1, Col - 1];
    }

    private void CreateBattleSpaces()
    {
        int rows = battleSpaces.GetLength(0);
        int cols = battleSpaces.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for(int j = 0; j < cols; j++)
            {
                battleSpaces[i,j] = new BattleSpace(i,j);
            }
        }
    }

}

public class BattleSpace
{
    public float x;
    public int y; //x and y used to get center point of the battle space in worldspace for moving characters around

    public int row;
    public int col; // 1,1  1,2  1,3 
                    // 2,1  2,2  2,3
                    // 3,1  3,2  3,3

    public IOccupyBattleSpace isInSpace;

    public BattleSpace(int Row, int Col)
    {
        this.row = Row;
        this.col = Col;
    }

}


public interface IOccupyBattleSpace
{
    public Team Team { get; }
    
}
