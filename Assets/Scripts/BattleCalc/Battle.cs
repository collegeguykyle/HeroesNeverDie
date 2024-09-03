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

    public List<Unit> PlayerTeam { get; private set; }
    public  List<Unit> EnemyTeam { get; private set; }
    public BattleSpacesController SpaceController { get; private set; }
    public TurnOrder TurnOrder { get; private set; }
    public BattleReport BattleReport = new BattleReport();
    public ReactionsManager Reactions { get; private set; } 
    public Unit CurrentUnit { get; private set; }
    private bool BattleOver = false;
    private List<Engagement> Engagements = new List<Engagement>();

    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        Reactions = new ReactionsManager(this);

        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;

        BattleReport.SetTeams(playerTeam, enemyTeam);
        TurnOrder = new TurnOrder(PlayerTeam, EnemyTeam);

        SpaceController = new BattleSpacesController(playerTeam, enemyTeam);
        SpaceController.PlaceEnemyTeam(EnemyTeam);
        SpaceController.PlacePlayerTeam(PlayerTeam);
        
        foreach (Unit unit in playerTeam) {unit.BattleStart(this); }
        foreach (Unit unit in enemyTeam) { unit.BattleStart(this); } 

        Reactions.onUnitDeath += TestBattleOver;
        BattleLoop();
    }

#region Core Game Play Loop

    private void BattleLoop()
    {
        while (BattleOver == false)
        {
            CurrentUnit = TurnOrder.GetCurrentUnit();  //Turn Order advances to next unit in End Unit Turn step
            TakeUnitTurn();
        }
    }

    private void TakeUnitTurn()
    {
        Reactions.SendStartUnitTurn(CurrentUnit); 
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

        CurrentUnit.ClearMana();
        Reactions.SendEndUnitTurn(CurrentUnit);
        TurnOrder.AdvanceToNextUnit();
    }

    public void TestBattleOver(object sender, Unit unit)
    {
        bool allDead = true;

        foreach (Unit u in PlayerTeam)
        {
            if (u.CurrentHP > 0) allDead = false;
        }
        if (allDead) 
        {
            Reactions.SendEndBattle();
            BattleOver = true;
        }
            
        foreach (Unit u in EnemyTeam)
        {
            if (u.CurrentHP > 0) allDead = false;
        }
        if (allDead) 
        { 
            Reactions.SendEndBattle();
            BattleOver = true;
            Reactions.Dispose();
        }

    }

    #endregion

#region Engagements

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

#region Executing Abilities

    public void ExecuteAttackResult(ResultAttack attackResult)
    {
        //TODO:  I AM HERE
        foreach(ResultTargetAttack attack in attackResult.Targets)
        {
            if (attack.ResultHit.hit)
            {

            }


        }
        //Do the damage, apply the buffs / debuffs, move things around
        //Check to see if this killed anyone or changes the battlefield in some way that affects pathfinding, etc

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




