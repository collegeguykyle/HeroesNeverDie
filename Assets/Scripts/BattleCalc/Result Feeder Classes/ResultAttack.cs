using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultAttack : EventArgs
{

    public List<ResultTargetAttack> Targets = new List<ResultTargetAttack>();

    public void ExecuteAttack()
    {
        foreach (ResultTargetAttack attackTargetResult in Targets)
        {
            if (attackTargetResult.ResultHit != null && attackTargetResult.ResultHit.hit == true)
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
    public ResultAttack(List<ResultTargetAttack> targets)
    {
        Targets = targets;
    }
    public ResultAttack(ResultTargetAttack target)
    {
        Targets.Add(target);
    }
    public ResultAttack(ResultTargetAttack target1, ResultTargetAttack target2)
    {
        Targets[0] = target1;
        Targets[1] = target2;
    }
    public ResultAttack(ResultTargetAttack target1, ResultTargetAttack target2, ResultTargetAttack target3)
    {
        Targets[0] = target1;
        Targets[1] = target2;
        Targets[2] = target3;
    }
    public ResultAttack(ResultTargetAttack[] targets)
    {
        foreach (ResultTargetAttack target in targets)
        {
            Targets.Add(target);
        }
    }

    public ResultAttack() { }
#endregion

    //consider: What if I want an ability to do X damage on hit, then if fail a save take additional damage?
    //this shows how abilities need to own execution with aid from interfaces, results stored in result containers


}

public interface IAOE
{
    public int MaxTargets { get; }
    public int AOESize { get; }
    public int AOERange { get; }
    public AOEShape AOEShape { get; }
}
public enum AOEShape { Circle, Line, Cone }
