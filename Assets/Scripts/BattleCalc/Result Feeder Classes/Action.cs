using System;
using System.Collections.Generic;


public abstract class Action : EventArgs
{
    //This class groups ActionST, ActionAOE, and Reactions as options for sending into the reaction manager
    //and for ResultAbility to log

}

public abstract class ActionResult
{
    //used to clump up the Result classes into a single list
}

public class ActionST : Action
{
    public IOccupyBattleSpace target;
    public List<ActionResult> actionResults;
    public Ability owningAbility;

    public void AddResult(ActionResult actionResult)
    {
        actionResults.Add(actionResult);
    }

    public ActionST(Ability ability)
    {
        this.owningAbility = ability;
    }

}

public class ActionAOE : Action
{
    public Ability owningAbility;
    public List<ActionST> Targets = new List<ActionST>();

    public static List<Unit> GetAOETargets(ResultTargetting TargetingData, IAOE ability)
    {
        List<Unit> result = new List<Unit>(); //TODO: Add AOE targeting here based on type of AOE and AOErange
        return result;
    }

    public ActionAOE(Ability ability)
    {
        owningAbility = ability;
    }
}

public interface IAOE
{
    public int MaxTargets { get; }
    public int AOESize { get; }
    public int AOERange { get; }
    public AOEShape AOEShape { get; }
}
public enum AOEShape { Circle, Line, Laser, Cone, Box }



public class Reaction : Action
{
    public Status owningStatus;
    public IOccupyBattleSpace target;
    public List<ActionResult> actionResults; //couldn't a reaction have an AOE effect? may need to rethink this, really I just want to log it as a reactoin but resolve it the same as the other actions


    public void AddResult(ActionResult actionResult)
    {
        actionResults.Add(actionResult);
    }

    public Reaction(Status status)
    {
        owningStatus = status;
    }


}

