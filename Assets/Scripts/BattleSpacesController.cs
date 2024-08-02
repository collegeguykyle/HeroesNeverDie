using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BattleSpacesController
{

    private BattleSpace[,] battleSpaces = new BattleSpace[3,3];

    public BattleSpacesController()
        {
            CreateBattleSpaces();
        }

    public bool OccupySpace(IOccupyBattleSpace entity, BattleSpace space)
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

    public void FindFreeSpace(IOccupyBattleSpace entity)
    {
        //[ ] cycle through all the spaces to find one that is free for the unit to go into based on its Team
    }

    public bool IsSpaceOccupied(int Row, int Col)
    {
        if (battleSpaces[Row, Col].isInSpace == null) return false;
        else return true;
    }

    public IOccupyBattleSpace WhoOccupiesSpace(int Row, int Col)
    {
        if (battleSpaces[Row, Col].isInSpace == null) return null;
        else return battleSpaces[Row, Col].isInSpace;
    }

    private void CreateBattleSpaces()
    {
        for (int i = 1; i < 4; i++)
        {
            for(int j = 1; j < 4; j++)
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
