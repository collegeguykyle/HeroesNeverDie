using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Melee1 : Ability, IHit, IDealDamage
{

    public override string Name { get; protected set; } = "Test Melee 1";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 15; //should this be 10 for orthoganol and 15 for diaganol?? Check BattleSpaceController
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;

    public List<ToHitBonus> ToHitBonus { get; protected set; } = new List<ToHitBonus>();

    public List<Damage> damageDice { get; protected set; } = new List<Damage>();

    public AttackType GetAttackType { get; } = AttackType.Physical; 

    public AttackRange GetAttackRange { get; } = AttackRange.Melee;

    public Melee1(Unit owner) : base(owner)
    {
        cost.AddManaType(ManaType.sword);
        cost.AddManaType(ManaType.sword);
        Damage damage = new Damage(Name, DamageType.Slashing, 2, 6);
        damageDice.Add(damage);
    }


    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        Debug.Log("Using Basic Test Melee");
        //This ability only does one attack
        ActionST resultAttack = new ActionST(this, TargettingData.SelectedTarget);
        ResultHit resultHit = ResultHit.TryHit(this, TargettingData.SelectedTarget as Unit);
        resultAttack.actionResults.Add(resultHit);
        if (resultHit.success)
        {
            ResultDamage damageResult = new ResultDamage(resultHit, this);
            resultAttack.actionResults.Add(damageResult);
        }
        OwningUnit.Battle.AddToActionStack(resultAttack);
    }

    public int GetAttackBonus()
    {
        return OwningUnit.PWR / 2;
    }

}

public class Melee2 : Ability, IHit, IDealDamage
{

    public override string Name { get; protected set; } = "Big Melee";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 15; //should this be 10 for orthoganol and 15 for diaganol?? Check BattleSpaceController
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;

    public List<ToHitBonus> ToHitBonus { get; protected set; } = new List<ToHitBonus>();

    public List<Damage> damageDice { get; protected set; } = new List<Damage> ();

    public AttackType GetAttackType { get; } = AttackType.Physical;

    public AttackRange GetAttackRange { get; } = AttackRange.Melee;

    public Melee2(Unit owner) : base(owner)
    {
        cost.AddManaType(ManaType.sword);
        cost.AddManaType(ManaType.sword);
        cost.AddManaType(ManaType.sword);
        Damage damage = new Damage(Name, DamageType.Slashing, 3, 6);
        damage.DamageBonus = 5;
        damageDice.Add(damage);
    }


    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        Debug.Log("Using Big Melee");
        //This ability only does one attack
        ActionST resultAttack = new ActionST(this, TargettingData.SelectedTarget);
        ResultHit resultHit = ResultHit.TryHit(this, TargettingData.SelectedTarget as Unit);
        resultAttack.actionResults.Add(resultHit);
        if (resultHit.success)
        {
            ResultDamage damageResult = new ResultDamage(resultHit, this);
            resultAttack.actionResults.Add(damageResult);
        }
        OwningUnit.Battle.AddToActionStack(resultAttack);
    }

    public int GetAttackBonus()
    {
        return OwningUnit.PWR / 2;
    }

}

public class MoveBasic : Ability, IMoveSelf
{
    //I AM HERE:  WHY IS THIS MOVE ABILITY NOT BEING SELECTED FOR EXECUTION BY THE TACTIC??
    public MoveBasic(Unit OwningUnit) : base(OwningUnit)
    {
    }

    public override string Name { get; protected set; } = "Basic Move";
    public override Mana cost { get; protected set; } = new Mana();
    public override int Range { get; protected set; } = 0;
    public override bool UseEngaged { get; protected set; } = false;
    public override Team targets { get; protected set; } = Team.enemy;

    public MoveType moveType { get; protected set; } = MoveType.Walk;

    public bool validPath { get;  } 

    public override void ExecuteAbility(ResultTargetting TargettingData)
    {
        Debug.Log("Using Basic Move");
        ResultMovement result = ResultMovement.MoveUnitTowards(OwningUnit, TargettingData.getTargetData().BattleSpace, MoveType.Walk);
        OwningUnit.Battle.AddToActionStack(result, this, TargettingData.SelectedTarget);
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