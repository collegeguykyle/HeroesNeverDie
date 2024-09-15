using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability, IHit, IDealDamage
{

    public override string Name { get; protected set; } = "Test Melee 1";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 10; //should this be 10 for orthoganol and 15 for diaganol?? Check BattleSpaceController
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;

    public List<ToHitBonus> ToHitBonus { get; protected set; } = new List<ToHitBonus>();

    public List<Damage> damageDice { get; protected set; }

    public AttackType GetAttackType { get; } = AttackType.Physical; 

    public AttackRange GetAttackRange { get; } = AttackRange.Melee;

    public Melee1(Unit unit) : base(unit)
    {
        cost.AddManaType(ManaType.sword);
        Damage damage = new Damage(Name, DamageType.Slashing, 2, 6);
        damageDice.Add(damage);
    }


    public override void ExecuteAbility(ResultTargetting TargettingData)
    {

        //This ability only does one attack
        ActionST resultAttack = new ActionST(this);
        ResultHit resultHit = ResultHit.TryHit(this, TargettingData.GetUnitTarget());
        resultAttack.actionResults.Add(resultHit);
        if (resultHit.success)
        {
            ResultDamage damageResult = new ResultDamage(resultHit, this);
            resultAttack.actionResults.Add(damageResult);
        }

    }

    public int GetAttackBonus()
    {
        throw new System.NotImplementedException();
    }

}

public class MoveBasic : Ability, IMoveSelf
{
    public MoveBasic(Unit OwningUnit) : base(OwningUnit)
    {
    }

    public override string Name { get; protected set; } = "Basic Move";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 0;
    public override bool UseEngaged { get; protected set; } = false;
    public override Team targets { get; protected set; } = Team.enemy;
    bool IMoveSelf.validPath { get ; set ; }

    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        ResultMovement result = ResultMovement.MoveUnitTowards(OwningUnit, TargettingData.SelectedTarget.BattleSpace, MoveType.Walk);
        OwningUnit.Battle.AddToActionStack(result, this);
    }
}

public class ApplyPoison : Ability, IApplyStatus
{
    public override string Name { get ; protected set ; }
    public override Mana cost { get ; protected set ; }
    public override int Range { get ; protected set ; }
    public override bool UseEngaged { get ; protected set ; }
    public override Team targets { get ; protected set ; }

    public List<Status> StatusToApply { get; protected set; } = new List<Status>();

    public ApplyPoison(Unit OwningUnit) : base(OwningUnit)
    {
    }

    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        
    }
}