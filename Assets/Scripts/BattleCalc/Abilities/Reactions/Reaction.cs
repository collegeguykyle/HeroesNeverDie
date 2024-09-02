using System.Collections;
using System.Collections.Generic;
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
    public abstract void DoReaction();
    public bool TestReaction(BattleReaction reaction)
    {
        if (reaction == ReactionType) return true;
        else return false;
    }
}


