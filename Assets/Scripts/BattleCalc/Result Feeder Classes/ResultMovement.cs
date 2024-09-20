


using Newtonsoft.Json;
using System.Collections.Generic;

public class ResultMovement : ActionResult
{
    [JsonIgnore] public Unit unitMoved;
    public string unitMovedName;
    public MoveType moveType;
    public BattleSpace startSpace;
    public BattleSpace moveToSpace;
    public int moveDist = 0;
    public bool validPath = true;
    public List<BattleSpace> _path = new List<BattleSpace>();

    public static ResultMovement MoveUnitTowards(Unit unit, BattleSpace to, MoveType type)
    {
        ResultMovement result = new ResultMovement();
        result.unitMoved = unit;
        result.unitMovedName = unit.Name;
        result.moveType = type;

        List<BattleSpace> path = unit.Battle.SpaceController.CalculatePath(unit, to);
        result._path = path;
        if (path != null && path.Count > 1)
        {
            result.startSpace = path[0];
            result.moveToSpace = path[1];
            result.moveDist = unit.Battle.SpaceController.CalculateDistance(path[0].row, path[0].col, path[1].row, path[1].col);
        }
        else
        {
            result.validPath = false;
        }
        return result;
    }
    public static ResultMovement MoveUnitTowards(Unit unit, Unit target, MoveType type)
    {
        ResultMovement result = new ResultMovement();
        result.unitMoved = unit;
        result.unitMovedName = unit.Name;
        result.moveType = type;

        BattleSpace to = target.Battle.SpaceController.GetSpaceOf(target);

        List<BattleSpace> path = unit.Battle.SpaceController.CalculatePath(unit, to);

        if (path != null && path.Count > 1)
        {
            result.startSpace = path[0];
            result.moveToSpace = path[1];
            result.moveDist = unit.Battle.SpaceController.CalculateDistance(path[0].row, path[0].col, path[1].row, path[1].col);
        }
        else
        {
            result.validPath = false;
        }
        return result;
    }

    public static bool CanMoveIntoRange(Unit caster, int abilityRange, Unit target)
    {
        List<BattleSpace> path = caster.Battle.SpaceController.CalculatePath(caster, target);
        BattleSpace targetSpace = caster.Battle.SpaceController.GetSpaceOf(target);

        for (int i = 0; i < path.Count; i++)
        {
            int range = caster.Battle.SpaceController.CalculateDistance(path[i].row, path[i].col, targetSpace.row, targetSpace.col);
            if (range <= abilityRange && i <= caster.CurrentMove)
            {
                return true;
            }
        }
        return false;
    }

}

public enum MoveType {  Walk, Fly, Teleport, Push, Pull }


public interface IMoveSelf
{
    public bool validPath { get; protected set; }
    //probably should require a MoveType enum....
    //Immune to difficult terrain types or other terrain effects?
}

public interface IMoveOthers
{

}