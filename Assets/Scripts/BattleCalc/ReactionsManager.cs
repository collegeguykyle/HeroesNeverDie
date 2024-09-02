using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Rendering.CoreUtils;
using System.Linq.Expressions;



public class ReactionsManager : IDisposable
{

    // ***TODO: Create data classes and delegates for each event type as needed

    public event EventHandler<Unit> onStartOfTurn;
    public event EventHandler<DieSide> onDiceRoll;
    public event EventHandler<ResultRoll> onRollResult;
    public event EventHandler<ResultTargetting> onTargeting;
    public event EventHandler<ResultAttack> onHitResult;
    public event EventHandler<ResultAbility> onAbilityComplete;
    public event EventHandler<Unit> onEndOfTurn;
    public event EventHandler onEndOfBattle;
    public event EventHandler<Unit> onUnitDeath;


    public void SendStartUnitTurn(Unit unit)
    {
        //BattleReport.AddReport(new ReportStartTurn(unit));
        if (onStartOfTurn != null)
        {
            foreach (EventHandler<Unit> handler in onStartOfTurn.GetInvocationList())
            {
                try { handler(this, unit); }
                catch (Exception ex) { Debug.Log($"Start of Turn Event Exception:  " + ex.Message); }
            }
        }
   
    }

    public void SendDieRolled(DieSide side) //reactions that modify dice rolls
    {
        //careful not to allow these changes to perminantly change the dice
        if (onDiceRoll != null)
        {
            DieSide clone = new DieSide(side); //create a copy of the dieSide so all modifications to the result are temporary
            foreach (EventHandler<DieSide> handler in onDiceRoll.GetInvocationList())
            {
                try { handler(this, clone); }
                catch (Exception ex) { Debug.Log($"Dice Rolled Event Exception:  " + ex.Message); }
            }
        }

    }

    public void SendRollResult(ResultRoll roll) //reactions that occur based on the final outcome of a unit's dice roll
    {
        if (onRollResult != null)
        {
            foreach (EventHandler<ResultRoll> handler in onRollResult.GetInvocationList())
            {
                try { handler(this, roll); }
                catch (Exception ex) { Debug.Log($"Dice Roll Result Event Exception:  " + ex.Message); }
            }
        }

    }

    public void SendTargetting(ResultTargetting result) 
    {
        if (onTargeting != null)
        {
            foreach (EventHandler<ResultTargetting> handler in onTargeting.GetInvocationList())
            {
                try { handler(this, result); }
                catch (Exception ex) { Debug.Log($"Targeting Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendHitResult(ResultAttack result)
    {
        if (onHitResult != null)
        {
            foreach (EventHandler<ResultAttack> handler in onHitResult.GetInvocationList())
            {
                try { handler(this, result); }
                catch (Exception ex) { Debug.Log($"Hit Result Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendAbilityComplete(Ability ability)
    {
        if (onAbilityComplete != null)
        {
            foreach (EventHandler<Ability> handler in onAbilityComplete.GetInvocationList())
            {
                try { handler(this, ability); }
                catch (Exception ex) { Debug.Log($"Ability Complete Event Exception:  " + ex.Message); }
            }
        }
    }

    public void SendEndUnitTurn(Unit unit)
    {
        if (onEndOfTurn != null)
        {
            foreach (EventHandler<Unit> handler in onEndOfTurn.GetInvocationList())
            {
                try { handler(this, unit); }
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

        Dispose();
    }

    public void SendUnitDeath(Unit unit)
    {
        if (onUnitDeath != null)
        {
            foreach (EventHandler<Unit> handler in onUnitDeath.GetInvocationList())
            {
                try { handler(this, unit); }
                catch (Exception ex) { Debug.Log($"Unit Death Event Exception:  " + ex.Message); }
            }
        }
        
    }

    public void Dispose() //unsubscribe all events to prevent memory leaks
    {
        onStartOfTurn = null;
        onDiceRoll = null;
        onRollResult = null;
        onTargeting = null;
        onHitResult = null;
        onAbilityComplete = null;
        onEndOfTurn = null;
        onEndOfBattle = null;
        onUnitDeath = null;
    }
}
