using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability
{
    public Melee1(Unit unit) : base(unit)
    {
    }

    public override void ExecuteAbility()
    {
        
    }

    public override void GetTargets()
    {
        
    }

    public override List<IOccupyBattleSpace> TestValidTargets()
    {
        List <IOccupyBattleSpace> options = new List < IOccupyBattleSpace >();
        return options;
    }
}
