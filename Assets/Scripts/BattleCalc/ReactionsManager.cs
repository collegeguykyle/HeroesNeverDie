using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Rendering.CoreUtils;
using System.Linq.Expressions;



public class ReactionsManager
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
                onStartOfTurn += reaction.DoReaction; break;
            case BattleReaction.DiceRoll:
                onDiceRoll += reaction.DoReaction; break;
            case BattleReaction.RollResult:
                onRollResult += reaction.DoReaction; break;
            case BattleReaction.Targeting:
                onTargeting += reaction.DoReaction; break;
            case BattleReaction.HitResult:
                onHitResult += reaction.DoReaction; break;
            case BattleReaction.AbilityComplete:
                onAbilityComplete += reaction.DoReaction; break;
            case BattleReaction.EndOfTurn:
                onEndOfTurn += reaction.DoReaction; break;
            case BattleReaction.EndOfBattle:
                onEndOfBattle += reaction.DoReaction; break;
            case BattleReaction.UnitDeath:
                onUnitDeath += reaction.DoReaction; break;
        }
    }

    public void RemoveReaction(Reaction reaction)
    {
        switch (reaction.ReactionType)
        {
            case BattleReaction.StartOfTurn:
                onStartOfTurn -= reaction.DoReaction; break;
            case BattleReaction.DiceRoll:
                onDiceRoll -= reaction.DoReaction; break;
            case BattleReaction.RollResult:
                onRollResult -= reaction.DoReaction; break;
            case BattleReaction.Targeting:
                onTargeting -= reaction.DoReaction; break;
            case BattleReaction.HitResult:
                onHitResult -= reaction.DoReaction; break;
            case BattleReaction.AbilityComplete:
                onAbilityComplete -= reaction.DoReaction; break;
            case BattleReaction.EndOfTurn:
                onEndOfTurn -= reaction.DoReaction; break;
            case BattleReaction.EndOfBattle:
                onEndOfTurn -= reaction.DoReaction; break;
            case BattleReaction.UnitDeath:
                onUnitDeath -= reaction.DoReaction; break;
        }
    }

    public void SendStartUnitTurn(Unit unit)
    {
        //BattleReport.AddReport(new ReportStartTurn(unit));
        if (onStartOfTurn != null)
        {
            foreach (Action handler in onStartOfTurn.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Start of Turn Event Exception:  " + ex.Message); }
            }
        }
   
    }

    public void SendDieRolled(DieSide side) //reactions that modify dice rolls
    {
        //careful not to allow these changes to perminantly change the dice
        if (onDiceRoll != null)
        {
            foreach (Action handler in onDiceRoll.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Dice Rolled Event Exception:  " + ex.Message); }
            }
        }

    }

    public void SendRollResult(Mana rolledMana) //reactions that occur based on the final outcome of a unit's dice roll
    {
        if (onRollResult != null)
        {
            foreach (Action handler in onRollResult.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Dice Roll Result Event Exception:  " + ex.Message); }
            }
        }

    }

    public void SendTargetting(ResultTargetting result) 
    {
        if (onTargeting != null)
        {
            foreach (Action handler in onTargeting.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Targeting Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendHitResult(ResultAttack result)
    {
        if (onHitResult != null)
        {
            foreach (Action handler in onHitResult.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Hit Result Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendAbilityComplete(Ability ability)
    {
        if (onAbilityComplete != null)
        {
            foreach (Action handler in onAbilityComplete.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Ability Complete Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendEndUnitTurn(Unit unit)
    {
        if (onEndOfTurn != null)
        {
            foreach (Action handler in onEndOfTurn.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"On End of Turn Event Exception:  " + ex.Message); }
            }
        }

    }

    public void SendEndBattle()
    {
        if (onEndOfBattle != null)
        {
            foreach (Action handler in onEndOfBattle.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"End of Battle Event Exception:  " + ex.Message); }
            }
        }
        
        //[ ] push the Battle Report somewhere via an event broadcast?
    }

    public void SendUnitDeath(Unit unit)
    {
        if (onUnitDeath != null)
        {
            foreach (Action handler in onUnitDeath.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"Unit Death Event Exception:  " + ex.Message); }
            }
        }

    }


}