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
            CurrentUnit.TakeTurn();
        }
        SendEndBattle();
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

}




