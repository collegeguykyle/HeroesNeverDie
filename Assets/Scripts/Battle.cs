using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Battle 
{

    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;
    private BattleSpacesController SpaceController = new BattleSpacesController();
    private BattleLog BattleLog = new BattleLog();
    private Unit CurrentUnit;
    private bool BattleOver = false;
    private Queue<Unit> TurnOrder = new Queue<Unit>();


    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
        StartBattle();
    }

    public void StartBattle()
    {
        SetUnitStartingPositions();
        SetTurnOrder();
        UpdateReactions();
        GetReactionsType(BattleReaction.StartOfCombat);
        StartRound();
    }
    
    private void StartRound()
    {
        while (BattleOver == false)
        {
            Unit next = GetNextUnitTurn();
            StartUnitTurn(next);
            //[ ] Add rounds or just loop initiative?
        }
        EndBattle();
    }

    private void StartUnitTurn(Unit unit)
    {
        GetReactionsType(BattleReaction.StartOfTurn);
        RollDice(unit);
        GetReactionsType(BattleReaction.DiceRoll);
        GetReactionsType(BattleReaction.RollResult);
        while (unit.PassTurn == false)
        {
            Ability ability = ChooseAbility(unit);
            UseAbility(unit, ability);
        }
        EndUnitTurn(unit);
    }

    private void EndBattle()
    {
        GetReactionsType(BattleReaction.EndOfBattle);
        //push the log somewhere via an event broadcast
    }

    private void SetUnitStartingPositions() //[ ] !!IMPORTANT FIX: Currently grid is 3x3 and both teams try to exist in it
    {
        foreach (Unit unit in EnemyTeam)
        {
            if (!SpaceController.OccupySpace(unit, unit.position))
            {
                SpaceController.FindFreeSpace(unit);
            }
        }
        foreach (Unit unit in PlayerTeam)
        {
            if (!SpaceController.OccupySpace(unit, unit.position))
            {
                SpaceController.FindFreeSpace(unit);
            }
        }
    }

    private void SetTurnOrder() 
    { 
    
    }

    private void UpdateReactions()
    {

    }
    
    private void GetReactionsType(BattleReaction ReactionType)
    {

    }

    

    private Unit GetNextUnitTurn()
    {
        return CurrentUnit;
    }

    

    private void RollDice(Unit unit)
    {

    }

    private Ability ChooseAbility(Unit unit)
    {

    }

    private void UseAbility(Unit unit, Ability ability)
    {
        GetReactionsType(BattleReaction.TargetingReaction);
        //to hit roll, do damage, apply additional ability effects
        GetReactionsType(BattleReaction.HitResult);
        GetReactionsType(BattleReaction.AbilityComplete);
    }

    private void EndUnitTurn(Unit unit)
    {
        GetReactionsType(BattleReaction.EndOfTurn);
        //reset mana
    }
}

public enum BattleReaction { StartOfCombat, StartOfTurn, DiceRoll, RollResult, TargetingReaction, HitResult, AbilityComplete, EndOfTurn, EndOfBattle, UnitDeath }
public interface IBattleReaction
{
    public BattleReaction Reaction { get; set; }
    public Unit Owner { get; set; }
    public void OnReaction();
    public bool TestReaction(BattleReaction reaction)
    {
        if (reaction == Reaction) return true;
        else return false;
    }
}


