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


    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;

        StartBattle();
    }

    public void StartBattle()
    {
        SetUnitStartingPositions();
        TurnOrder = new TurnOrder(PlayerTeam, EnemyTeam);
        // [ ] Call start of battle function on all units, this will do their start of combat stuff
        //     and hook up all relevant stuff to their events as needed for future reactions
        BattleLoop();
    }
    private void SetUnitStartingPositions()
    {
        SpaceController.PlaceEnemyTeam(EnemyTeam);
        SpaceController.PlacePlayerTeam(PlayerTeam);
    }

    private void BattleLoop()
    {
        while (BattleOver == false)
        {
            Unit next = TurnOrder.GetCurrentUnit();
            StartUnitTurn(next);
        }
        EndBattle();
    }

    private void StartUnitTurn(Unit unit)
    {
        Reactions.OnStartOfTurn?.Invoke(unit);
        RollDice(unit);
        Reactions.onDiceRoll?.Invoke(unit);
        Reactions.onRollResult?.Invoke(unit);
        while (unit.PassTurn == false)
        {
            Ability ability = ChooseAbility(unit);
            UseAbility(unit, ability);
        }
        EndUnitTurn(unit);
    }

    private void EndUnitTurn(Unit unit)
    {
        Reactions.onEndOfTurn?.Invoke(unit);
        // [ ] reset mana
        TurnOrder.AdvanceToNextUnit();
    }


    private void RollDice(Unit unit)
    {

    }

    private Ability ChooseAbility(Unit unit) //add that this returns an ability
    {
        Melee1 a = new Melee1(unit);
        return a;
    }

    private void UseAbility(Unit target, Ability ability)
    {
        Reactions.onTargeting?.Invoke(CurrentUnit, target, ability);
        //to hit roll, do damage, apply additional ability effects
        Reactions.onHitResult?.Invoke();
        Reactions.onAbilityComplete?.Invoke();
    }
    private void EndBattle()
    {
        Reactions.onEndOfBattle?.Invoke();
        //push the log somewhere via an event broadcast
    }

}




