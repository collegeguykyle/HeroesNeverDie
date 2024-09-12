using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class Status
{
    public abstract string Name { get; }
    public virtual int Stacks { get; protected set; } = 0;
    public IOccupyBattleSpace Owner { get; protected set; }
    public abstract void SubscribeEvents(Battle battle);
    public Battle BattleController { get; }
    public Status (Battle battle, IOccupyBattleSpace owner)
    {
        BattleController = battle;
        Owner = owner;
        SubscribeEvents(battle);
    }
    public void addStacks(int stacks)
    {
        Stacks += stacks;
    }
}

#region Interfaces

public interface IReactStartTurn
{
    public abstract void onStartTurn(object sender, Unit unit);
    public void UnsubscribeStartTurn(Battle battle)
    {
        battle.Reactions.StartOfTurn -= onStartTurn;
    }
    public void SubscribeStartTurn(Battle battle)
    {
        battle.Reactions.SubscribeStartUnitTurn(onStartTurn);
    }

}

public interface IReactManaDieRoll
{
    public abstract void onManaDieRoll(object sender, ResultRollMana result);
    public void UnsubscribeManaDieRoll(Battle battle)
    {
        battle.Reactions.ManaDieRoll -= onManaDieRoll;
    }
    public void SubscribeManaDieRoll(Battle battle)
    {
        battle.Reactions.SubscribeManaDieRolled(onManaDieRoll);
    }
}

public interface IReactManaRollResult
{
    public abstract void onManaRollResult(object sender, ResultRollMana result);
    public void UnsubscribeManaRollResult(Battle battle)
    {
        battle.Reactions.ManaRollResult -= onManaRollResult;
    }
    public void SubscribeManaRollResult(Battle battle)
    {
        battle.Reactions.SubscribeManaRollResult(onManaRollResult);
    }
}

public interface IReactTargeting
{
    public abstract void onTargeting(object sender, ResultTargetting result);
    public void UnsubscribeTargeting(Battle battle)
    {
        battle.Reactions.Targeting -= onTargeting;
    }
    public void SubscribeTargeting(Battle battle)
    {
        battle.Reactions.SubscribeTargeting(onTargeting);
    }
}

public interface IReactActionResult
{
    public abstract void onActionResult(object sender, Action result);
    public void UnsubscribeActionResult(Battle battle)
    {
        battle.Reactions.ActionResult -= onActionResult;
    }
    public void SubscribeActionResult(Battle battle)
    {
        battle.Reactions.SubscribeActionResult(onActionResult);
    }
}

public interface IReactAbilityComplete
{
    public abstract void onAbilityComplete(object sender, ResultAbility result);
    public void UnsubscribeAbilityComplete(Battle battle)
    {
        battle.Reactions.AbilityComplete -= onAbilityComplete;
    }
    public void SubscribeAbilityComplete(Battle battle)
    {
        battle.Reactions.SubscribeAbilityComplete(onAbilityComplete);
    }
}

public interface IReactEndOfTurn
{
    public abstract void onEndOfTurn(object sender, Unit unit);
    public void UnsubscribeEndOfTurn(Battle battle)
    {
        battle.Reactions.EndOfTurn -= onEndOfTurn;
    }
    public void SubscribeEndOfTurn(Battle battle)
    {
        battle.Reactions.SubscribeEndUnitTurn(onEndOfTurn);
    }
}

public interface IReactUnitDeath
{
    public abstract void onUnitDeath(object sender, Unit unit);
    public void UnsubscribeUnitDeath(Battle battle)
    {
        battle.Reactions.UnitDeath -= onUnitDeath;
    }
    public void SubscribeUnitDeath(Battle battle)
    {
        battle.Reactions.SubscribeUnitDeath(onUnitDeath);
    }
}


#endregion


//Example status class
public class S_Poison : Status, IReactStartTurn
{
    public S_Poison(Battle battle, IOccupyBattleSpace owner) : base(battle, owner)
    {
    }

    public override string Name { get; } = "Poison";

    public override void SubscribeEvents(Battle battle)
    {
        battle.Reactions.SubscribeStartUnitTurn(onStartTurn);
    }
    public void onStartTurn(object sender, Unit unit)
    {
        Reaction reaction = new Reaction(this);
        Damage damage = new Damage("Poison", DamageType.Nature, Stacks);
        ResultDamage resultDamage = new ResultDamage(damage);
        //TODO: send it
    }
}






