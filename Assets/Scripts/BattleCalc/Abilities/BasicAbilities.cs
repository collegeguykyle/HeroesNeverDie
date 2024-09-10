using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability, IHit, IDealDamage
{

    public override string Name { get; protected set; } = "Test Melee 1";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 1; //should this be 10 for orthoganol and 15 for diaganol?? Check BattleSpaceController
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;

    public List<AttackType> GetAttackTypes { get; protected set; } = new List<AttackType>();

    public int NumberOfAttacks { get; protected set; } = 1;

    public List<ToHitBonus> ToHitBonus { get; protected set; } = new List<ToHitBonus>();

    public List<Damage> damageDice { get; protected set; }

    public Melee1(Unit unit) : base(unit)
    {
        cost.AddManaType(ManaType.sword);
        Damage damage = new Damage(Name, AttackType.Slashing, 2, 6);
        damageDice.Add(damage);
    }


    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        //This ability only does one attack
        ResultHit resultHit = ResultHit.TryHit(this, TargettingData.GetUnitTarget());
        if (resultHit.success)
        {
            ResultDamage damageResult = new ResultDamage(resultHit, this);
            ResultSingleTarget resultAttack = new ResultSingleTarget(new ResultAOEAction(resultHit, damageResult));
        }
        
        
        //This attack does not apply a debuff so we will skip that step

        //ResultTargetAttack is really only needed for an IAOE attack
        ResultAbility resultAbility = new ResultAbility(OwningUnit, this, resultAttack);

    }

    public int GetAttackBonus()
    {
        throw new System.NotImplementedException();
    }

}

