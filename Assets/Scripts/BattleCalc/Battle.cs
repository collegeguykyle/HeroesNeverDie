using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Battle 
{
    //This class runs the core battle loop!!  It should be run on the server with results (Battle Report)
    //return back to the player client.
    //It takes two teams as inputs and fully calculates the result of the fight progmatically.
    //As it calculates the fight, it records everything in a Battle Report, which is what  
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
    private Stack<Action> ActionsStack = new Stack<Action>();

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

        Reactions.UnitDeath += TestBattleOver;
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
        CurrentUnit.RollManaDice(); //1: Unit rolls mana
        List<Tactic> Tactics = CurrentUnit.Tactics;
        //2: itterate through unit's tactics in order of priority until we find one that can be used, and use it
        //if we use an abiltiy, repeat the process. Can use any number of abilities on a turn if mana remains
        //and abilities have sufficient uses available. If we itterate through full list without finding
        //an abiltiy to use, then the unit cannot do anything and passes the turn.
        bool passTurn = false;
        int safeGuard = 0; //pass the turn after X iterations through the tactic to avoid accidental infinite loops 
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
            safeGuard++;
            if (safeGuard > 20)
            {
                passTurn = true;
                Debug.Log($"{CurrentUnit} turn passed via 20 tactics passes safeguard");
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

#region Stack Resolution

    public void AddToActionStack(Action action)
    {
        //This is where the DOING happens. When an ability or reaction wants to do something, it sends itself to
        //the reaction manager, which gives all reactions an oppertunity to either modify the results of action
        //in some way, or to execute an action of their own by being placed on the stack. 
        ActionsStack.Push(action);
        Reactions.SendActionResult(action);

        while (ActionsStack.Count > 0)
        {
            ResolveAction(ActionsStack.Pop());
        }
    }
    public void AddToActionStack(ActionResult result, Ability ability)
    {
        ActionST action = new ActionST(ability);
        action.AddResult(result);
        AddToActionStack(action);
    }

    private void ResolveAction(Action action)
    {
        if (action is ActionST) ResolveSingleTarget(action as ActionST);
        if (action is ActionAOE) ResolveMultiTarget(action as ActionAOE);
        if (action is Reaction) ResolveReaction(action as Reaction);
    }

    private void ResolveSingleTarget(ActionST action)
    {
        // Ok, all adjustments have been made and reactions stacked!
        // Now to actually DO the THINGS, and LOG the things!
        foreach (ActionResult result in action.actionResults)
        {
            if (result is ResultHit) { } //then just log it
            if (result is ResultSave) { } //then just log it
            if (result is ResultDamage) ResolveDamage(result as ResultDamage, action.target as Unit);
            if (result is ResultStatus) ResolveStatus(result as ResultStatus, action.target as Unit);
            if (result is ResultMovement) ResolveMove(result as ResultMovement);
        }
    }
    private void ResolveMultiTarget(ActionAOE action)
    {

    }
    private void ResolveReaction(Reaction action)
    {

    }

    private void ResolveDamage(ResultDamage result, Unit target)
    {
        //Put code here to have damage that is going to be done altered by NON-REACTING Status, such as damage immunity
        if (target != null && target is Unit)
        {
            //log it
            target.TakeDamage(result.TotalDamage);
        }
    }
    private void ResolveStatus(ResultStatus result, Unit target)
    {
        Status statusToEffect = result.status;
        if (target != null && target is Unit)
        {
            //if unit already has a copy of this status, just alter its stacks, don't add a new instance to the list
            bool hasAStack = false;
            foreach (Status status in target.statusList)
            {
                if (status.Name == result.status.Name)
                {
                    hasAStack = true;
                    statusToEffect = status;
                }
            }
            if (!hasAStack) target.statusList.Add(statusToEffect);
   
            result.newStacksTotal = statusToEffect.ChangeStacks(result.stacksChange);
            result.newStatus = true;
            if (result.newStacksTotal <= 0) result.removeStatus = true;

            //log it
        }
    }
    private void ResolveMove(ResultMovement result)
    {
        if (result.validPath && result.moveDist <= result.unitMoved.CurrentMove)
        {
            SpaceController.MoveUnitTo(result.unitMoved, result.moveToSpace);
            //log it
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




