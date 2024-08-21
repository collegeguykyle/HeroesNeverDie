using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability
{
    public Melee1(Unit unit) : base(unit)
    {

    }

    public override string Name { get; set; }
    public override Mana cost { get; set; }
    public override Unit OwningUnit { get; set; }
    public override int Range { get; set; }
    public override bool UseEngaged { get; set; }

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
