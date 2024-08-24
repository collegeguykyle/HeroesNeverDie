using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Battle 
{

    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;
    private BattleSpacesController SpaceController = new BattleSpacesController();
    private TurnOrder TurnOrder;
    private BattleLog BattleLog = new BattleLog();
    private Reactions Reactions = new Reactions();
    private Unit CurrentUnit;
    private bool BattleOver = false;
    private List<Engagement> Engagements = new List<Engagement>();

    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;

        SpaceController.PlaceEnemyTeam(EnemyTeam);
        SpaceController.PlacePlayerTeam(PlayerTeam);
        
        foreach (Unit unit in playerTeam) {unit.BattleStart(this); }
        foreach (Unit unit in enemyTeam) { unit.BattleStart(this); }

        TurnOrder = new TurnOrder(PlayerTeam, EnemyTeam);

        BattleLoop();
    }

    private void BattleLoop()
    {
        while (BattleOver == false)
        {
            CurrentUnit = TurnOrder.GetCurrentUnit();  //Turn Order advances to next unit in End Unit Turn step
            TakeUnitTurn();
        }
        SendEndBattle();
    }

    private void TakeUnitTurn()
    {
        SendStartUnitTurn(CurrentUnit);
        CurrentUnit.RollDice();
        List<Tactic> Tactics = CurrentUnit.Tactics;

        //itterate through unit's tactics in order of priority until we find one that can be used, and use it
        //if we use an abiltiy, repeat the process. Can use any number of abilities on a turn if mana remains
        //and abilities have sufficient uses available. If we itterate through full list without finding
        //an abiltiy to use, then the unit cannot do anything and passes the turn.
        bool passTurn = false;
        do 
        {
            passTurn = true;
            foreach (Tactic tactic in Tactics)
            {
                bool CanUseAction = tactic.TestTactic(this, SpaceController, CurrentUnit.Mana);
                if (CanUseAction)
                {
                    tactic.Execute();
                    passTurn = false;
                    break;
                }
            }
        } while (!passTurn);

        CurrentUnit.EndTurn();
        SendEndUnitTurn(CurrentUnit);
    }



    private void SendUseAbility(Unit target, Ability ability)
    {
        Reactions.onTargeting?.Invoke();
        //to hit roll, do damage, apply additional ability effects
        Reactions.onHitResult?.Invoke();
        Reactions.onAbilityComplete?.Invoke();
    } 
    
    public void SendEndUnitTurn(Unit unit)
    {
        Reactions.onEndOfTurn?.Invoke();
        TurnOrder.AdvanceToNextUnit();
    }
    
    private void SendEndBattle()
    {
        Reactions.onEndOfBattle?.Invoke();
        //[ ] push the log somewhere via an event broadcast
    }

    
    public void SendStartUnitTurn(Unit unit)
    {

        Reactions.onStartOfTurn?.Invoke();
    }
    
    public void SendDieRolled(DieSide side) //reactions that modify dice rolls
    {
        //careful not to allow these changes to perminantly change the dice
        Reactions.onDiceRoll?.Invoke();
    }

    public void SendRollResult(Mana rolledMana)
    {
        
        Reactions.onRollResult?.Invoke();
    }

    public void AddEngagement(Unit Attacker, Unit Victim)
    {
        Engagements.Add(new Engagement(Attacker, Victim));
    }

    public void RemoveEngagement(Unit Attacker, Unit Victim)
    {
        foreach (Engagement E in Engagements)
        {
            if (E.Compare(Attacker, Victim)) Engagements.Remove(E);
        }
    }

    public bool TestEngaged (Unit unit)
    {
        bool isEngaged = false;
        foreach(Engagement E in Engagements)
        {
            if (E.Victim == unit) isEngaged = true;
        }
        return isEngaged;
    }
}

public class Engagement
{
    public Engagement(Unit attacker, Unit victim)
    {
        Attacker = attacker;
        Victim = victim;
    }
    
    public Unit Attacker;
    public Unit Victim;

    public bool Compare (Unit attacker, Unit victim)
    {
        if (attacker == Attacker && victim == Victim) return true;
        else return false;
    }
}




