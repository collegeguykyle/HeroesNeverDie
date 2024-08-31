using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Battle 
{
    //This class is the core of the game!!  It should be run on the server with results (Battle Report)
    //return back to the player client.
    //It takes two teams as inputs and fully calculates the result of the fight progmatically.
    //As it calculates the fight, it records everything in a Battle Report, which is what is 
    //the player client uses to create a replay of the battle which they watch to see the results.

    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;
    private BattleSpacesController SpaceController = new BattleSpacesController();
    private TurnOrder TurnOrder;
    private BattleReport BattleReport = new BattleReport();
    private Reactions Reactions = new Reactions();
    private Unit CurrentUnit;
    private bool BattleOver = false;
    private List<Engagement> Engagements = new List<Engagement>();

    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
        BattleReport.SetTeams(playerTeam, enemyTeam);

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
        CurrentUnit.RollDice(); //1: Unit rolls mana
        List<Tactic> Tactics = CurrentUnit.Tactics;

        //2: itterate through unit's tactics in order of priority until we find one that can be used, and use it
        //if we use an abiltiy, repeat the process. Can use any number of abilities on a turn if mana remains
        //and abilities have sufficient uses available. If we itterate through full list without finding
        //an abiltiy to use, then the unit cannot do anything and passes the turn.
        bool passTurn = false;
        do 
        {
            passTurn = true;
            foreach (Tactic tactic in Tactics)
            {
                bool TacticUsed = tactic.TestTactic(this, SpaceController, CurrentUnit.Mana);
                if (TacticUsed)
                {
                    passTurn = false;
                    break;
                }
            }
        } while (!passTurn);

        CurrentUnit.EndTurn();
        SendEndUnitTurn(CurrentUnit);
    }

    private void rollAttack()
    {

    }

#region Reaction Event Invokers

    public void SendStartUnitTurn(Unit unit)
    {
        BattleReport.AddReport(new ReportStartTurn(unit));
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
        BattleOver = true;
        //[ ] push the log somewhere via an event broadcast
    }

    public void SendUnitDeath(Unit unit)
    {
        if(unit.Team == Team.player)
        {
            bool allDead = true;
            foreach (Unit u in PlayerTeam)
            {
                if (u.CurrentHP > 0) allDead = false;
            }
            if (allDead) SendEndBattle();
        }
        if (unit.Team == Team.enemy)
        {
            bool allDead = true;
            foreach (Unit u in EnemyTeam)
            {
                if (u.CurrentHP > 0) allDead = false;
            }
            if (allDead) SendEndBattle();
        }
    }
 
#endregion


#region Engagements
    public void AddReaction(Reaction reaction)
    {
        Reactions.AddReaction(reaction);
    }
    public void RemoveReaction(Reaction reaction)
    {
        Reactions.RemoveReaction(reaction);
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

 #endregion

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




