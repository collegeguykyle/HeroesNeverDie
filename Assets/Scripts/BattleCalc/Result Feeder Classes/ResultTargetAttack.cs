using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ResultTargetAttack
{
    public ResultHit ResultHit;
    public ResultDamage ResultDamage;
    public ResultSave ResultSave;

    public ResultTargetAttack(ResultHit resultHit, ResultDamage resultDamage, ResultSave resultStatus)
    {
        ResultHit = resultHit;
        ResultDamage = resultDamage;
        ResultSave = resultStatus;
    }
    public ResultTargetAttack(ResultHit resultHit, ResultDamage resultDamage)
    {
        ResultHit = resultHit;
        ResultDamage = resultDamage;
    }
    public ResultTargetAttack(ResultHit resultHit, ResultSave resultSave)
    {
        ResultHit = resultHit;
        ResultSave= resultSave;
    }
    public ResultTargetAttack(ResultSave resultSave, ResultDamage resultDamage)
    {
        ResultSave = resultSave;
        ResultDamage = resultDamage;
    }
}