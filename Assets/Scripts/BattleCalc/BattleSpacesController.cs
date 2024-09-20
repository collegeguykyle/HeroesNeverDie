using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSpacesController
{
    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;
    private BattleSpace[,] battleSpaces = new BattleSpace[9,5];  //[row, col]
    public BattleSpacesController(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
        CreateBattleSpaces();
        PlaceEnemyTeam(enemyTeam);
        PlacePlayerTeam(playerTeam);
    }

#region Controller Setup

    private void CreateBattleSpaces()
    {
        int rows = battleSpaces.GetLength(0);
        int cols = battleSpaces.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                battleSpaces[i, j] = new BattleSpace(i+1, j+1);

            }
        }
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
            battleSpaces[space.row -1, space.col -1].isInSpace = entity;
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
            if (unit.StartingCol == 1) unit.StartingCol = 5;
            else if (unit.StartingCol == 2) unit.StartingCol = 4;
            else if (unit.StartingCol == 4) unit.StartingCol = 2;
            else if (unit.StartingCol == 5) unit.StartingCol = 1;
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

#endregion

#region Info Pulls on location of units or battle spaces

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
        if(Row < 1 || Row > 9 || Col < 1 || Col > 5) return null;
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

#endregion

#region Ability Targeting Methods

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

    public List<TargetData> GetTargets(Unit unit)
    {
        BattleSpace castFrom = GetSpaceOf(unit);
        List<TargetData> result = new List<TargetData>();

        List<Unit> targetOptions = new List<Unit>();
        foreach(Unit u in PlayerTeam) targetOptions.Add(u);
        foreach(Unit u in EnemyTeam) targetOptions.Add(u);
        //***TODO***Currently no way implimented to target terrain or nuetrals
        
        foreach (Unit target in targetOptions)
        {
            BattleSpace space = GetSpaceOf(target);
            int rangeTo = CalculateDistance(castFrom.row, castFrom.col, space.row, space.col);
            List<BattleSpace> path = CalculatePath(unit, target);
            
            TargetData data = new TargetData(target, space, rangeTo, path.Count);
            result.Add(data);
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

    #endregion

#region Path Finding

    public List<BattleSpace> CalculatePath(int rowStart, int colStart, int rowGoal, int colGoal)
    {
        List<BattleSpace> openList = new List<BattleSpace>();
        List<BattleSpace> closedList = new List<BattleSpace>();

        BattleSpace StartNode = GetBattleSpaceAt(rowStart, colStart);
        BattleSpace GoalNode = GetBattleSpaceAt(rowGoal, colGoal);
        if (StartNode == null)
        {
            Debug.Log("PathFind failed due to no BattleSpace found at Row: " + rowStart + ", Col: " + colStart);
            return null;
        }
        if (GoalNode == null)
        {
            Debug.Log("PathFind failed due to no BattleSpace found at Row: " + rowStart + ", Col: " + colStart);
            return null;
        }

        ResetPathfindingScores();
        StartNode.G = 0;
        StartNode.H = 0;
        openList.Add(StartNode);
        int failsafe = 0;

        while (openList.Count > 0)
        {
            failsafe++;
            if (failsafe > 75) //infinite loop protection
            {
                Debug.Log("Pathfind failsafe activated");
                return null;
            }

            openList.Sort();
            BattleSpace openNode = openList[0];
            closedList.Add(openNode);
            openList.RemoveAt(0);

            if (openNode == GoalNode) //Found the end, calculate the path
            {
                List<BattleSpace> nodePath = new List<BattleSpace>();
                nodePath.Add(openNode);
                int pathFailSafe = 0;
                while (openNode != StartNode)
                {
                    pathFailSafe++;
                    if (pathFailSafe > 25)
                    {
                        Debug.Log("Pathfind Failsafe activated at the path found stage");
                        return null;
                    }
                    openNode = openNode.PathParent;
                    nodePath.Add(openNode);
                }
                nodePath.Reverse(); //path was backwards, this should reverse the path to be start to goal
                
                
                return nodePath;
            }
            
            List<BattleSpace> children = GetNeighbors(openNode);
            
            foreach(BattleSpace child in children)
            {
                if (closedList.Contains(child)) continue;
                if (child.isInSpace != null && child != GoalNode) //add a space to the closed list if there is something in it
                {
                    closedList.Add(child);
                    continue;
                }
                int G = openNode.G + 10;
                    if (child.row != openNode.row && child.col != openNode.col) G += 5; //diaganols cost 15
                int H = CalculateDistance(child.row, child.col, GoalNode.row, GoalNode.col); //distance between child and goal

                if (openList.Contains(child) && G > child.G) continue;
                else
                {
                    child.G = G;
                    child.H = H;
                    child.PathParent = openNode;
                    if (!openList.Contains(child)) openList.Add(child);
                }
            }
            
        }
        Debug.Log("No path found");
        return null; //if you get here then the algo searched every space and never found a path;
    }
    public List<BattleSpace> CalculatePath(Unit mover, IOccupyBattleSpace target)
    {
        BattleSpace moverSpace = GetSpaceOf(mover);
        BattleSpace targetSpace = GetSpaceOf(target);
        List<BattleSpace> path = CalculatePath(moverSpace.row, moverSpace.col, targetSpace.row, targetSpace.col);
        return path;
    }
    public List<BattleSpace> CalculatePath(Unit mover, BattleSpace target)
    {
        BattleSpace moverSpace = GetSpaceOf(mover);
        List<BattleSpace> path = CalculatePath(moverSpace.row, moverSpace.col, target.row, target.col);
        return path;
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

    #endregion
   
    public bool MoveUnitTo(Unit unit, BattleSpace space)
    {
        if (IsSpaceOccupied(space.row, space.col))
        {
            IOccupyBattleSpace who = WhoOccupiesSpace(space.row, space.col);
            if (who is Unit) return false;
        }

        BattleSpace start = GetSpaceOf(unit);
        start.isInSpace = null;
        space.isInSpace = unit;
        return true;
        
    }   


}

public class BattleSpace : IComparable<BattleSpace>
{
    [JsonIgnore] public float x;
    [JsonIgnore] public float y; //x and y used to get center point of the battle space in worldspace for moving characters around

    public int row;
    public int col; // 1,1  1,2  1,3  1,4  1,5
                    // 2,1  2,2  2,3
                    // 3,1  3,2  3,3

    [JsonIgnore] public BattleSpace PathParent;
    [JsonIgnore] public int F { get { return G + H; } } //F is the total cost of the node. F = G + H
    [JsonIgnore] public int G = 999; //G is the distance between the current node and the start node.
    [JsonIgnore] public int H = 999; //H is the heuristic — estimated distance from the current node to the end node.

    [JsonIgnore] public IOccupyBattleSpace isInSpace;

    //Add stuff here for if the battle space is in zone of control for a Unit?

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
public class TargetData
{
    [JsonIgnore] public IOccupyBattleSpace target;
    public string targetName;
    public Team targetTeam;
    public BattleSpace BattleSpace;
    public int rangeTo;
    public int pathDist;


    public TargetData(IOccupyBattleSpace target, BattleSpace targetSpace, int rangeTo, int pathDist)
    {
        this.target = target;
        this.targetName = target.Name;
        this.targetTeam = target.Team;
        BattleSpace = targetSpace;
        this.rangeTo = rangeTo;
        this.pathDist = pathDist;
    }
}

public interface IOccupyBattleSpace
{
    public abstract string Name { get; }
    public abstract Team Team { get; }
    public abstract List<Status> statusList { get; }
    
}
