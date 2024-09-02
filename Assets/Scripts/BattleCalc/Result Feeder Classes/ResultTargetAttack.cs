using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ResultTargetAttack
{
    public ResultHit ResultHit;
    public ResultDamage ResultDamage;
    public ResultSave ResultStatus;

    public ResultTargetAttack(ResultHit resultHit, ResultDamage resultDamage, ResultSave resultStatus)
    {
        ResultHit = resultHit;
        ResultDamage = resultDamage;
        ResultStatus = resultStatus;
    }
}