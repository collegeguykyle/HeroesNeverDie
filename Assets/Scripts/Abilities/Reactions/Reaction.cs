using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum BattleReaction { 
    UnSet, 
    StartOfCombat, 
    StartOfTurn, 
    DiceRoll, 
    RollResult, 
    Targeting, 
    HitResult, 
    AbilityComplete, 
    EndOfTurn, 
    EndOfBattle, 
    UnitDeath 
}


public abstract class Reaction 
{
    public BattleReaction ReactionType { get; set; }
    public Unit Owner { get; set; }
    public abstract void OnReaction();
    public bool TestReaction(BattleReaction reaction)
    {
        if (reaction == ReactionType) return true;
        else return false;
    }


}

public class Reactions
{

    // each type of reaction is going to need a certain set of information to evaluate
    // which means we probably need different delegate setups for most of these

    public Action OnStartCombat;
    public Action<Unit> OnStartOfTurn;
    public Action<Unit> onDiceRoll;
    public Action<Unit> onRollResult;
    public Action<Unit, Unit, Ability> onTargeting;
    public Action onHitResult;
    public Action onAbilityComplete;
    public Action<Unit> onEndOfTurn;
    public Action onEndOfBattle;
    public Action<Unit> onUnitDeath;


}
