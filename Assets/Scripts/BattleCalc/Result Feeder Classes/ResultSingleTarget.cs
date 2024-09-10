using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultSingleTarget : ResultAction
{

    public List<ResultAOEAction> Targets = new List<ResultAOEAction>();

    //TODO: This method needs to die and become smaller helper methods
    //to make it easier for individual abilities to do common tasks
    public void ExecuteAttack()
    {
        foreach (ResultAOEAction attackTargetResult in Targets)
        {
            if (attackTargetResult.ResultHit != null && attackTargetResult.ResultHit.success == true)
            {
                Unit target = attackTargetResult.ResultHit.Target;
                if (attackTargetResult.ResultDamage != null)
                    target.TakeDamage(attackTargetResult.ResultDamage.TotalDamage);
            }
            //if an attack is BOTH IHit and ISave then the save is conditional on the hit
            if (attackTargetResult.ResultSave != null)
            {
                //TODO: impliment code for checking if a save was successful or not, then applying the buff / debuff
            }
            //TODO: Add implimentation for moving the user of the ability or others hit by it

        }

        //Check to see if changes the battlefield in some way that affects pathfinding, etc
    }

    

    #region Constructors
    public ResultSingleTarget(List<ResultAOEAction> targets)
    {
        Targets = targets;
    }
    public ResultSingleTarget(ResultAOEAction target)
    {
        Targets.Add(target);
    }
    public ResultSingleTarget(ResultAOEAction target1, ResultAOEAction target2)
    {
        Targets[0] = target1;
        Targets[1] = target2;
    }
    public ResultSingleTarget(ResultAOEAction target1, ResultAOEAction target2, ResultAOEAction target3)
    {
        Targets[0] = target1;
        Targets[1] = target2;
        Targets[2] = target3;
    }
    public ResultSingleTarget(ResultAOEAction[] targets)
    {
        foreach (ResultAOEAction target in targets)
        {
            Targets.Add(target);
        }
    }

    public ResultSingleTarget() { }
#endregion

    //consider: What if I want an ability to do X damage on hit, then if fail a save take additional damage?
    //this shows how abilities need to own execution with aid from interfaces, results stored in result containers


}


