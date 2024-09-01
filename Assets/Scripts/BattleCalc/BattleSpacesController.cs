using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSpacesController
{
    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;
    private BattleSpace[,] battleSpaces = new BattleSpace[9,3];  //[row, col]
    public BattleSpacesController(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
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

    public BattleSpace GetSpaceOf(IOccupyBattleSpace actor)
    {
        foreach (BattleSpace space in battleSpaces)
        {
            if (space.isInSpace == actor) return space;
        }
        return null;
    }

    public List<IOccupyBattleSpace> GetTargetsInRange(int Row, int Col, int range, Team targetsTeam, bool excludeStart)
    {
        // *****TODO***** should return a list of all units within X range of the given row/col
        //probably create a few different input versions of this method
        //maybe one that also just returns a bool / asks for which team you are searching for
        List<BattleSpace> occupiedSpaces = new List<BattleSpace>();
        foreach (BattleSpace space in battleSpaces)
        {
            if (space.isInSpace != null && space.isInSpace.Team == targetsTeam) occupiedSpaces.Add(space);
        }
        List<IOccupyBattleSpace> targets = new List<IOccupyBattleSpace>();
        foreach (BattleSpace space in occupiedSpaces)
        {
            if (space == GetBattleSpaceAt(Row, Col) && excludeStart) break;
            if (CalculateDistance(space.row, space.col, Row, Col) > range) break;
            targets.Add(space.isInSpace);
        }
        return targets;
    }

    public ResultTargetting GetTargets(Ability ability)
    {
        BattleSpace castFrom = GetSpaceOf(ability.OwningUnit);
        ResultTargetting result = new ResultTargetting(ability, castFrom);
        
        List<Unit> targetOptions; 
        Team t = TargetConversion(ability.OwningUnit, ability.targets);
        if (t == Team.player) targetOptions = PlayerTeam;
        else targetOptions = EnemyTeam;                     //***TODO***Currently no way implimented to target terrain or nuetrals
        
        foreach (Unit target in targetOptions)
        {
            TargetData data = new TargetData();
            data.targetType = target;
            data.BattleSpace = GetSpaceOf(target);
            data.rangeTo = CalculateDistance(castFrom.row, castFrom.col, data.BattleSpace.row, data.BattleSpace.col);
            if (data.rangeTo <= ability.Range) data.inRange = true;
            data.OthersAOE = GetTargetsInRange(data.BattleSpace.row, data.BattleSpace.col, ability.AOESize, t, false);
            result.AddTargetData(data);
        }
        return result;
    }
    private Team TargetConversion(Unit unit, Team target)
    {
        if (unit.Team == Team.player) return target;
        if (unit.Team == Team.enemy)
        {
            if (target == Team.enemy) return Team.player;
            if (target == Team.player) return Team.enemy;
        }
        return target;
    }

    public int CalculateDistance(int row1, int col1, int row2, int col2)
    {
        // Calculate differences in row and column indices
        int deltaRow = Mathf.Abs(row2 - row1);
        int deltaCol = Mathf.Abs(col2 - col1);

        // Calculate the number of diagonal moves (minimum of deltaRow and deltaCol)
        int diagonalMoves = Mathf.Min(deltaRow, deltaCol);

        // Calculate the number of remaining orthogonal moves
        int orthogonalMoves = Mathf.Abs(deltaRow - deltaCol);

        // Calculate the total distance
        float distance = (diagonalMoves * 1.5f) + orthogonalMoves;

        return Mathf.RoundToInt(distance * 10);
    }

    public List<BattleSpace> CalculatePath(int rowStart, int colStart, int rowGoal, int colGoal)
    {
        List<BattleSpace> openList = new List<BattleSpace>();
        List<BattleSpace> closedList = new List<BattleSpace>();

        BattleSpace StartNode = GetBattleSpaceAt(rowStart, colStart);
        BattleSpace GoalNode = GetBattleSpaceAt(rowGoal, colGoal);
        if (StartNode == null || GoalNode == null) return null;

        ResetPathfindingScores();
        StartNode.G = 0;
        StartNode.H = 0;
        openList.Add(StartNode);

        while (openList.Count > 0)
        {
            openList.Sort();
            BattleSpace openNode = openList[0];
            openList.RemoveAt(0);
            closedList.Add(openNode);

            if (openNode == GoalNode) //Found the end, calculate the path
            {
                List<BattleSpace> nodePath = new List<BattleSpace>();
                nodePath.Add(openNode);
                while (openNode != StartNode)
                {
                    openNode = openNode.PathParent;
                    nodePath.Add(openNode);
                }
                nodePath.Sort(); //path was backwards, this should reverse the path to be start to goal
                return nodePath;
            }
            
            List<BattleSpace> children = GetNeighbors(openNode);
            
            foreach(BattleSpace child in children)
            {
                if (closedList.Contains(child)) break;
                if (child.isInSpace != null) //add a space to the closed list if there is something in it
                {
                    closedList.Add(child);
                    break;
                }
                int G = openNode.G + 10;
                    if (child.row != openNode.row && child.col != openNode.col) G += 5; //diaganols cost 15
                int H = CalculateDistance(child.row, child.col, GoalNode.row, GoalNode.col); //distance between child and goal

                if (openList.Contains(child) && G > child.G) break;
                else
                {
                    child.G = G;
                    child.H = H;
                    if (!openList.Contains(child)) openList.Add(child);
                }
            }
            
        }
        return null; //if you get here then the algo searched every space and never found a path;
    }
    private void ResetPathfindingScores()
    {
        foreach (BattleSpace space in battleSpaces)
        {
            space.G = 999;
            space.H = 999;
        }
    }
    public List<BattleSpace> GetNeighbors(BattleSpace space)
    {
        List<BattleSpace> neighors = new List<BattleSpace>();
        BattleSpace next = GetBattleSpaceAt(space.row -1, space.col -1);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row - 1, space.col);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row - 1, space.col + 1);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row, space.col - 1);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row, space.col + 1);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row + 1, space.col - 1);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row + 1, space.col);
            if (next != null) neighors.Add(next);
        next = GetBattleSpaceAt(space.row + 1, space.col + 1);
            if (next != null) neighors.Add(next);
        return neighors;
    }
    public List<BattleSpace> GetNeighbors(int row, int col)
    {
        return GetNeighbors(GetBattleSpaceAt(row, col));
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

public class BattleSpace : IComparable<BattleSpace>
{
    public float x;
    public float y; //x and y used to get center point of the battle space in worldspace for moving characters around

    public int row;
    public int col; // 1,1  1,2  1,3 
                    // 2,1  2,2  2,3
                    // 3,1  3,2  3,3

    public BattleSpace PathParent;
    public int F { get { return G + H; } } //F is the total cost of the node. F = G + H
    public int G = 999; //G is the distance between the current node and the start node.
    public int H = 999; //H is the heuristic — estimated distance from the current node to the end node.

    public IOccupyBattleSpace isInSpace;

    public BattleSpace(int Row, int Col)
    {
        this.row = Row;
        this.col = Col;
    }

    public int CompareTo(BattleSpace other)
    {
        if (other == null) throw new ArgumentNullException("Node compared to was null");
        return this.F.CompareTo(other.F);
    }

}


public interface IOccupyBattleSpace
{
    public Team Team { get; }
    
}
