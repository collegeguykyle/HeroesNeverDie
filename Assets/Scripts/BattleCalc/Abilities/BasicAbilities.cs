using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability, IDealDamage
{


    public override string Name { get; protected set; } = "Test Melee 1";
    public override Mana cost { get; protected set; } = new Mana();
    public override Unit OwningUnit { get; protected set; }
    public override int Range { get; protected set; } = 1; //should this be 10 for orthoganol and 15 for diaganol?? Check BattleSpaceController
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;
    public override List<AttackType> GetAttackTypes { get; protected set; }
    public override int GetBonus { get; protected set; }
    public override int AOESize { get; protected set; }
    public Damage damage { get; protected set; }

    public Damage Test1;

    public Melee1(Unit unit) : base(unit)
    {
        cost.AddManaType(ManaType.sword);
    }


    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        //I've got targetting data. Now I need to try to hit something.
        ResultHit resultHit = ResultHit.TryHit(this, TargettingData.GetUnitTarget());
        //If I hit it then I need to deal damage to it, by passing in the ResultHit
        ResultDamage damageResult = SendDamage(damage);
        //If the attack also applies a debuff then I need to get a ResultStatus
        
        //this is where we would put in our logic of all the possible upgrades for this ability
    }

    public ResultDamage SendDamage(Damage damage)
    {
        throw new System.NotImplementedException();
    }
}

