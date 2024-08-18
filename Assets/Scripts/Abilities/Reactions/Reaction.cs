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
    public Action onStartOfTurn;
    public Action onDiceRoll;
    public Action onRollResult;
    public Action onTargeting;
    public Action onHitResult;
    public Action onAbilityComplete;
    public Action onEndOfTurn;
    public Action onEndOfBattle;
    public Action onUnitDeath;

    public void AddReaction(Reaction reaction)
    {
        switch (reaction.ReactionType)
        {
            case BattleReaction.StartOfTurn:
                onStartOfTurn += reaction.OnReaction; break;
            case BattleReaction.DiceRoll:
                onDiceRoll += reaction.OnReaction; break;
            case BattleReaction.RollResult:
                onRollResult += reaction.OnReaction; break;
            case BattleReaction.Targeting:
                onTargeting += reaction.OnReaction; break;
            case BattleReaction.HitResult:
                onHitResult += reaction.OnReaction; break;
            case BattleReaction.AbilityComplete:
                onAbilityComplete += reaction.OnReaction; break;
            case BattleReaction.EndOfTurn:
                onEndOfTurn += reaction.OnReaction; break;
            case BattleReaction.EndOfBattle:
                onEndOfBattle += reaction.OnReaction; break;
            case BattleReaction.UnitDeath:
                onUnitDeath += reaction.OnReaction; break;
        }
    }

    public void RemoveReaction(Reaction reaction)
    {
        switch (reaction.ReactionType)
        {
            case BattleReaction.StartOfTurn:
                onStartOfTurn -= reaction.OnReaction; break;
            case BattleReaction.DiceRoll:
                onDiceRoll -= reaction.OnReaction; break;
            case BattleReaction.RollResult:
                onRollResult -= reaction.OnReaction; break;
            case BattleReaction.Targeting:
                onTargeting -= reaction.OnReaction; break;
            case BattleReaction.HitResult:
                onHitResult -= reaction.OnReaction; break;
            case BattleReaction.AbilityComplete:
                onAbilityComplete -= reaction.OnReaction; break;
            case BattleReaction.EndOfTurn:
                onEndOfTurn -= reaction.OnReaction; break;
            case BattleReaction.EndOfBattle:
                onEndOfTurn -= reaction.OnReaction; break;
            case BattleReaction.UnitDeath:
                onUnitDeath -= reaction.OnReaction; break;
        }
    }


}
