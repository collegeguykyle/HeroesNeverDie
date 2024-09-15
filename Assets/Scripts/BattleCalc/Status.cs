using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;


public abstract class Status
{
    public abstract string Name { get; }
    public virtual int Stacks { get; protected set; } = 0;
    public IOccupyBattleSpace Owner { get; protected set; }
    public Battle BattleController { get; protected set; }
    public Status (Battle battle, IOccupyBattleSpace owner)
    {
        BattleController = battle;
        Owner = owner;
        Subscribe(battle);
    }
    public void addStacks(int stacks)
    {
        Stacks += stacks;
    }

    public void Subscribe(Battle battle)
    {
        Status status = this;
        BattleController = battle;
        if (status is IReactStartTurn) battle.Reactions.SubscribeStartUnitTurn((status as IReactStartTurn).onStartTurn);
        if (status is IReactManaDieRoll) battle.Reactions.SubscribeManaDieRolled((status as IReactManaDieRoll).onManaDieRoll);
        if (status is IReactManaRollResult) battle.Reactions.SubscribeManaRollResult((status as IReactManaRollResult).onManaRollResult);
        if (status is IReactTargeting) battle.Reactions.SubscribeTargeting((status as IReactTargeting).onTargeting); ;
        if (status is IReactActionResult) battle.Reactions.SubscribeActionResult((status as IReactActionResult).onActionResult);
        if (status is IReactAbilityComplete) battle.Reactions.SubscribeAbilityComplete((status as IReactAbilityComplete).onAbilityComplete); ;
        if (status is IReactEndOfTurn) battle.Reactions.SubscribeEndUnitTurn((status as IReactEndOfTurn).onEndOfTurn); ;
        if (status is IReactUnitDeath) battle.Reactions.SubscribeUnitDeath((status as IReactUnitDeath).onUnitDeath); ;
    }

    public void UnsubscribeAll()
    {
        if (this is IReactStartTurn) BattleController.Reactions.StartOfTurn -= (this as IReactStartTurn).onStartTurn;
        if (this is IReactManaDieRoll) BattleController.Reactions.ManaDieRoll -= (this as IReactManaDieRoll).onManaDieRoll;
        if (this is IReactManaRollResult) BattleController.Reactions.ManaRollResult -= (this as IReactManaRollResult).onManaRollResult;
        if (this is IReactTargeting) BattleController.Reactions.Targeting -= (this as IReactTargeting).onTargeting;
        if (this is IReactActionResult) BattleController.Reactions.ActionResult -= (this as IReactActionResult).onActionResult;
        if (this is IReactAbilityComplete) BattleController.Reactions.AbilityComplete -= (this as IReactAbilityComplete).onAbilityComplete;
        if (this is IReactEndOfTurn) BattleController.Reactions.EndOfTurn -= (this as IReactEndOfTurn).onEndOfTurn;
        if (this is IReactUnitDeath) BattleController.Reactions.UnitDeath -= (this as IReactUnitDeath).onUnitDeath;
    }

    public bool UnitHasStatus()
    {
        foreach (Status status in Owner.statusList)
        {
            if (status.Name == this.Name) return true;
        }
        return false;
    }

    public int ChangeStacks(int stacks)
    {
        Stacks += stacks;
        if (Stacks <= 0)
        {
            UnsubscribeAll();
            Owner.statusList.Remove(this);
            //log removal here or in resultStatus?
        }
        return stacks;
    }
}

#region Interfaces

public interface IReactStartBattle
{
    public abstract void onStartBattle();
}
public interface IReactStartTurn
{
    public abstract void onStartTurn(object sender, Unit unit);
}

public interface IReactManaDieRoll
{
    public abstract void onManaDieRoll(object sender, ResultRollMana result);
}

public interface IReactManaRollResult
{
    public abstract void onManaRollResult(object sender, ResultRollMana result);
}

public interface IReactTargeting
{
    public abstract void onTargeting(object sender, ResultTargetting result);
}

public interface IReactActionResult
{
    public abstract void onActionResult(object sender, Action result);
}

public interface IReactAbilityComplete
{
    public abstract void onAbilityComplete(object sender, ResultAbility result);
}

public interface IReactEndOfTurn
{
    public abstract void onEndOfTurn(object sender, Unit unit);
}

public interface IReactUnitDeath
{
    public abstract void onUnitDeath(object sender, Unit unit);
}


#endregion


//Example status class
public class S_Poison : Status, IReactStartTurn
{
    public S_Poison(Battle battle, IOccupyBattleSpace owner) : base(battle, owner)
    {
    }

    public override string Name { get; } = "Poison";

    public void onStartTurn(object sender, Unit unit)
    {
        if (unit != null && unit == Owner)
        {
            Reaction reaction = new Reaction(this);

            ResultDamage resultDamage = new ResultDamage(new Damage("Poison", DamageType.Nature, Stacks)); //deal 1 damage per stack
            reaction.AddResult(resultDamage);
            
            ResultStatus resultStatus = new ResultStatus(this, -1);  //lose 1 stack at the start of affected unit's turn after dealing damage
            reaction.AddResult(resultStatus);

            BattleController.PushAction(reaction);
        }
        
    }
}






