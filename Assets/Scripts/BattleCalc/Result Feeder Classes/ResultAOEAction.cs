using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ResultAOEAction : ResultAction
{
    public ResultHit ResultHit;
    public ResultDamage ResultDamage;
    public ResultSave ResultSave;


#region Constructors
    public ResultAOEAction(ResultHit resultHit, ResultDamage resultDamage, ResultSave resultStatus)
    {
        ResultHit = resultHit;
        ResultDamage = resultDamage;
        ResultSave = resultStatus;
    }
    public ResultAOEAction(ResultHit resultHit, ResultDamage resultDamage)
    {
        ResultHit = resultHit;
        ResultDamage = resultDamage;
    }
    public ResultAOEAction(ResultHit resultHit, ResultSave resultSave)
    {
        ResultHit = resultHit;
        ResultSave= resultSave;
    }
    public ResultAOEAction(ResultSave resultSave, ResultDamage resultDamage)
    {
        ResultSave = resultSave;
        ResultDamage = resultDamage;
    }

#endregion

}

//ResultTargetAttack is only needed if the attack hits multiple targets:
public interface IAOE
{
    public int MaxTargets { get; }
    public int AOESize { get; }
    public int AOERange { get; }
    public AOEShape AOEShape { get; }
}
public enum AOEShape { Circle, Line, Cone }