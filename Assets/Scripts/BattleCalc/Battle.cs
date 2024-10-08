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
    int battleSafeGuard = 0;
    public Engagements engagements { get; private set; } = new Engagements();
    private Stack<Action> ActionsStack = new Stack<Action>();
    private ResultAbility currentAbility;

#region Constructors

    public Battle(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        Reactions = new ReactionsManager(this);

        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;

        BattleReport.SetTeams(playerTeam, enemyTeam);
        TurnOrder = new TurnOrder(PlayerTeam, EnemyTeam);

        SpaceController = new BattleSpacesController(playerTeam, enemyTeam);
        
        foreach (Unit unit in playerTeam) {unit.BattleStart(this); }
        foreach (Unit unit in enemyTeam) { unit.BattleStart(this); } 

        BattleLoop();
    }
    

    #endregion 

#region Core Game Play Loop

    private void BattleLoop()
    {
        
        while (BattleOver == false)
        {
            CurrentUnit = TurnOrder.GetCurrentUnit();  //Turn Order advances to next unit in End Unit Turn step
            TakeUnitTurn();
            battleSafeGuard++;
            if (battleSafeGuard > 100)
            {
                BattleReport.AddReport(new ReportMessage($"BATTLE ENDED via battelSafeGuard, over 999 turns executed with no victor"));
                Debug.Log($"BATTLE ENDED via battelSafeGuard, over 999 turns executed with no victor");
                EndBattle(Team.neutral);

            }
        }
    }

    private void TakeUnitTurn()
    {
        BattleReport.AddReport(new ReportStartTurn(CurrentUnit));
        Reactions.SendStartUnitTurn(CurrentUnit);

        CurrentUnit.CurrentMove = CurrentUnit.MaxMove;

        ResultRollMana result = CurrentUnit.RollManaDice(); //1: Unit rolls mana
        Reactions.SendManaDieRolled(result);
        BattleReport.AddReport(result);

        CurrentUnit.Mana.AddMana(result.TotalMana());
        CurrentUnit.Mana.ReportMana(BattleReport);

        List<Tactic> Tactics = CurrentUnit.Tactics;
        //2: itterate through unit's tactics in order of priority until we find one that can be used, and use it
        //if we use an abiltiy, repeat the process. Can use any number of abilities on a turn if mana remains
        //and abilities have sufficient uses available. If we itterate through full list without finding
        //an abiltiy to use, then the unit cannot do anything and passes the turn.
        bool passTurn = false;
        int turnSafeGuard = 1; //pass the turn after X iterations through the tactic to avoid accidental infinite loops 
        do 
        {
            if (currentAbility != null) AbilityClose(currentAbility);
            BattleReport.AddReport(new ReportMessage("Starting Tactics Eval Loop: " + turnSafeGuard));
            passTurn = true;
            List<TargetData> targetsData = SpaceController.GetTargets(CurrentUnit);
            BattleSpace space = SpaceController.GetSpaceOf(CurrentUnit);
            BattleReport.AddReport(new ReportTargettingData(CurrentUnit, space, targetsData));

            foreach (Tactic tactic in Tactics)
            {
                BattleReport.AddReport(new ReportMessage("Testing Tactic:  " + tactic.Ability.Name ));

                //3-5: Are basic requirements to use the tactic met? Unit alive, enough mana, etc
                BasicConditions conditions = tactic.BasicConditionsMet(this, CurrentUnit.Mana, engagements);
                if (!conditions.AllConditionsMet()) 
                { 
                    BattleReport.AddReport(new ResultTargetting(tactic.Ability, conditions));
                    continue;
                }

                //6-10: Do tactic conditions allow for use?
                ResultTargetting resultT = tactic.TestTactic(this, targetsData, CurrentUnit.Mana, engagements);
                resultT.conditions = conditions;
                //*****10: Execute the ability against the chosen target*****
                if (resultT.TacticSelected && resultT.getTargetData() != null) 
                {
                    passTurn = false;
                    Reactions.SendTargeting(resultT);
                    BattleReport.AddReport(resultT);
                    AbilitySelected(tactic.Ability, resultT);
                    tactic.Ability.ExecuteAbility(resultT);
                    break;
                }
                else
                {
                    BattleReport.AddReport(resultT);
                }
            }
            turnSafeGuard++;
            if (turnSafeGuard > 20)
            {
                passTurn = true;
                BattleReport.AddReport(new ReportMessage($"{CurrentUnit} itterated over its tactics pool 20 times but did not pass turn. Safeguard activated"));
                Debug.Log($"{CurrentUnit} itterated over its tactics pool 20 times but did not pass turn. Safeguard activated");
            }
        } while (!passTurn);

        if (currentAbility != null)
        {
            BattleReport.AddReport(currentAbility);
            currentAbility = null;
        }
        CurrentUnit.ClearMana();
        BattleReport.AddReport(new ReportEndTurn(CurrentUnit));
        Reactions.SendEndUnitTurn(CurrentUnit);
        TurnOrder.AdvanceToNextUnit();
    }

    public void AbilitySelected(Ability ability, ResultTargetting result)
    {
        if (currentAbility != null) BattleReport.AddReport(currentAbility);
        currentAbility = new ResultAbility(CurrentUnit, ability);
        CurrentUnit.Mana.RemoveMana(ability.cost);
    }
    public void AbilityComplete(ResultAbility result)
    {
        Reactions.SendAbilityComplete(result);
        BattleReport.AddReport(currentAbility);
        currentAbility = null;
    }
    public void AbilityClose(ResultAbility result)
    {
        BattleReport.AddReport(currentAbility);
        currentAbility = null;
    }

    public void EndBattle(Team victors)
    {
        Reactions.SendEndBattle();
        BattleOver = true;
        Reactions.Dispose();
        BattleReport.AddReport(currentAbility);
        BattleReport.TotalTurns = battleSafeGuard;
        BattleReport.Victors = victors;
        BattleReport.AddReport(new ReportEndBattle(victors));
    }

    public void UnitKilled(Unit unit)
    {
        TurnOrder.RemoveFromTurnOrder(unit);
        Reactions.SendUnitDeath(unit);
        TestBattleOver();
    }

    public void TestBattleOver()
    {
        bool allPlayerDead = true;
        bool allEnemyDead = true;

        foreach (Unit u in PlayerTeam)
        {
            if (u.CurrentHP > 0) allPlayerDead = false;
        }

        foreach (Unit u in EnemyTeam)
        {
            if (u.CurrentHP > 0) allEnemyDead = false;
        }

        if (allPlayerDead && allEnemyDead) EndBattle(Team.neutral); //This is a tie
        else if (allPlayerDead) EndBattle(Team.enemy);
        else if (allEnemyDead) EndBattle(Team.player);
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
            Debug.Log("resolving Action stack: " + ActionsStack.Count);
            ResolveAction(ActionsStack.Pop());
        }
    }
    public void AddToActionStack(ActionResult result, Ability ability, IOccupyBattleSpace target)
    {
        ActionST action = new ActionST(ability, target);
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
        currentAbility.AddAction(action);
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
        }
    }
    private void ResolveMove(ResultMovement result)
    {
        if (result.validPath && result.moveDist <= result.unitMoved.CurrentMove)
        {
            SpaceController.MoveUnitTo(result.unitMoved, result.moveToSpace);
            result.unitMoved.CurrentMove -= result.moveDist;
        }
        else BattleReport.AddReport(new ReportMessage("Movement tried to resolve but something went wrong"));
    }


#endregion

}






