using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultAttack
{

    public List<ResultTargetAttack> Targets = new List<ResultTargetAttack>();


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
#endregion

}
