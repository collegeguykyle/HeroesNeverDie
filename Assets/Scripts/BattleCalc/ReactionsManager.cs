using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class ReactionsManager : IDisposable
{
    public Battle battle;

    public event EventHandler<Unit> StartOfTurn;
    public event EventHandler<ResultRollMana> ManaDieRoll;
    public event EventHandler<ResultRollMana> ManaRollResult;
    public event EventHandler<ResultTargetting> Targeting;
    public event EventHandler<Action> ActionResult;
    public event EventHandler<ResultAbility> AbilityComplete;
    public event EventHandler<Unit> EndOfTurn;
    public event EventHandler EndOfBattle;
    public event EventHandler<Unit> UnitDeath;

    public ReactionsManager(Battle battle)
    {
        this.battle = battle;
    }

    public void SendStartUnitTurn(Unit unit)
    {
        //BattleReport.AddReport(new ReportStartTurn(unit));
        if (StartOfTurn != null)
        {
            foreach (EventHandler<Unit> handler in StartOfTurn.GetInvocationList())
            {
                try { handler(this, unit); }
                catch (Exception ex) { Debug.Log($"Start of Turn Event Exception:  " + ex.Message); }
            }
        }
    }
    public void SubscribeStartUnitTurn(EventHandler<Unit> handler)
    {
        if (!AmISubscribed(StartOfTurn, handler)) StartOfTurn += handler;
    }

    public void SendManaDieRolled(ResultRollMana result) //reactions that modify dice rolls
    {
        //careful not to allow these changes to perminantly change the dice
        if (ManaDieRoll != null)
        {
            foreach (EventHandler<ResultRollMana> handler in ManaDieRoll.GetInvocationList())
            {
                try { handler(this, result); }
                catch (Exception ex) { Debug.Log($"Dice Rolled Event Exception:  " + ex.Message); }
            }
        }
    }
    public void SubscribeManaDieRolled(EventHandler<ResultRollMana> handler)
    {
        if (!AmISubscribed(ManaDieRoll, handler)) ManaDieRoll += handler;
    }

    public void SendRollResult(ResultRollMana roll) //reactions that occur based on the final outcome of a unit's dice roll
    {
        if (ManaRollResult != null)
        {
            foreach (EventHandler<ResultRollMana> handler in ManaRollResult.GetInvocationList())
            {
                try { handler(this, roll); }
                catch (Exception ex) { Debug.Log($"Dice Roll Result Event Exception:  " + ex.Message); }
            }
        }

    }
    public void SubscribeManaRollResult(EventHandler<ResultRollMana> handler)
    {
        if (!AmISubscribed(ManaRollResult, handler)) ManaRollResult += handler;
    }

    public void SendTargeting(ResultTargetting result) 
    {
        if (Targeting != null)
        {
            foreach (EventHandler<ResultTargetting> handler in Targeting.GetInvocationList())
            {
                try { handler(this, result); }
                catch (Exception ex) { Debug.Log($"Targeting Event Exception:  " + ex.Message); }
            }
        }
    }
    public void SubscribeTargeting(EventHandler<ResultTargetting> handler)
    {
        if (!AmISubscribed(Targeting, handler)) Targeting += handler;
    }

    public void SendActionResult(Action result)
    {
        //FIREST: Send the attack to Battle? to apply the damage / status / move
        //THEN let reactions react to what happened
        if (ActionResult != null)
        {
            foreach (EventHandler<Action> handler in ActionResult.GetInvocationList())
            {
                try { handler(this, result); }
                catch (Exception ex) { Debug.Log($"Action Result Event Exception:  " + ex.Message); }
            }
            //TODO: After all reactions have modified the action or done their reaction, execute the action
            //IE Deal damage, add status, move actors
        }
        
    }
    public void SubscribeActionResult(EventHandler<Action> handler)
    {
        if (!AmISubscribed(ActionResult, handler)) ActionResult += handler;
    }

    //TODO: Add SendActionComplete?  For reactions that don't trigger before the action resolves, but after, but before the next action

    public void SendAbilityComplete(ResultAbility ability)
    {
        if (AbilityComplete != null)
        {
            foreach (EventHandler<ResultAbility> handler in AbilityComplete.GetInvocationList())
            {
                try { handler(this, ability); }
                catch (Exception ex) { Debug.Log($"Ability Complete Event Exception:  " + ex.Message); }
            }
        }
    }
    public void SubscribeAbilityComplete(EventHandler<ResultAbility> handler)
    {
        if (!AmISubscribed(AbilityComplete, handler)) AbilityComplete += handler;
    }

    public void SendEndUnitTurn(Unit unit)
    {
        if (EndOfTurn != null)
        {
            foreach (EventHandler<Unit> handler in EndOfTurn.GetInvocationList())
            {
                try { handler(this, unit); }
                catch (Exception ex) { Debug.Log($"On End of Turn Event Exception:  " + ex.Message); }
            }
        }

    }
    public void SubscribeEndUnitTurn(EventHandler<Unit> handler)
    {
        if (!AmISubscribed(EndOfTurn, handler)) EndOfTurn += handler;
    }

    public void SendEndBattle()
    {
        if (EndOfBattle != null)
        {
            foreach (System.Action handler in EndOfBattle.GetInvocationList())
            {
                try { handler(); }
                catch (Exception ex) { Debug.Log($"End of Battle Event Exception:  " + ex.Message); }
            }
        }
        //[ ] push the Battle Report somewhere via an event broadcast?

        Dispose();
    }
    public void SubscribeEndBattle(EventHandler handler)
    {
        if (!AmISubscribed(EndOfBattle, handler)) EndOfBattle += handler;
    }

    public void SendUnitDeath(Unit unit)
    {
        if (UnitDeath != null)
        {
            foreach (EventHandler<Unit> handler in UnitDeath.GetInvocationList())
            {
                try { handler(this, unit); }
                catch (Exception ex) { Debug.Log($"Unit Death Event Exception:  " + ex.Message); }
            }
        }
        
    }
    public void SubscribeUnitDeath(EventHandler<Unit> handler)
    {
        if (!AmISubscribed(UnitDeath, handler)) UnitDeath += handler;
    }


    public static bool AmISubscribed(Delegate eventDelegate, Delegate handler)
    {
        // Check if the event has any subscribers
        if (eventDelegate != null)
        {
            // Iterate through the invocation list and check for the given handler
            foreach (var existingHandler in eventDelegate.GetInvocationList())
            {
                if (existingHandler == handler)
                {
                    return true; // Handler is already subscribed
                }
            }
        }
        return false; // Handler is not subscribed
    }

    public void Dispose() //unsubscribe all events to prevent memory leaks
    {
        StartOfTurn = null;
        ManaDieRoll = null;
        ManaRollResult = null;
        Targeting = null;
        ActionResult = null;
        AbilityComplete = null;
        EndOfTurn = null;
        EndOfBattle = null;
        UnitDeath = null;
    }
}
