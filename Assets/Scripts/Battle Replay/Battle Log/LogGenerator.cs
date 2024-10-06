using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogGenerator
{

    public int totalSteps { get; private set; } = 0;
    public List<string> logEntries { get; private set; } = new List<string>();

    public LogGenerator(BattleReport battleReport)
    {
        foreach (ToReport report in battleReport.Reports)
        {
            ReadReport(report);
        }
    }

    private void ReadReport(ToReport report)
    {
        if (report is ReportMessage) ReadMessage(report as ReportMessage);
        if (report is ReportStartTurn) ReadStartTurn(report as ReportStartTurn);
        if (report is ReportEndTurn) ReadEndTurn(report as ReportEndTurn);
        if (report is ReportTargettingData) ReadTargetting(report as ReportTargettingData);
        if (report is ReportStartRound) ReadStartRound(report as ReportStartRound);
        if (report is ReportUnitDeath) ReadUnitDeath(report as ReportUnitDeath);
        if (report is ReportEndBattle) ReadEndBattle(report as ReportEndBattle);
        if (report is Action) Debug.Log("Action in the battle Report insteald of nested in an ability");
        if (report is ResultAbility) ReadResultAbility(report as ResultAbility);
        if (report is ResultTargetting) ReadResultTargetting(report as ResultTargetting);
        if (report is ResultRollMana) ReadResultMana(report as ResultRollMana);
    }

    private void ReadMessage(ReportMessage report)
    {
        logEntries.Add("<color=#7a97b2>" + report.message + "</color> \n");
    }

    private void ReadStartTurn(ReportStartTurn report)
    {
        logEntries.Add("<color=#ffb90f>Starting " + report.unitName + "'s Turn. </color>\n");
    }

    private void ReadEndTurn(ReportEndTurn report)
    {
        //No end of turn battle log entry required.
        //logEntries.Add("<color=#ffb90f>Ending " + report.unitName + "'s Turn. </color>\n");
    }

    private void ReadStartRound(ReportStartRound report)
    {
        logEntries.Add("<color=#ffb90f>Starting Round: " + report.round + "</color>\n");
    }

    private void ReadUnitDeath(ReportUnitDeath report)
    {
        logEntries.Add("<color=#cd000>     " + report.unitKilled + " has been slain! </color>\n");
    }

    private void ReadEndBattle(ReportEndBattle report)
    {
        logEntries.Add("<color=#cd000>" + report.Victors + " TEAM WINS!!! </color>\n");
    }

    private void ReadAction(ActionST action)
    {
        foreach(ActionResult result in action.actionResults)
        {
            if (result is ResultHit)
            {

            }
            if (result is ResultSave)
            {

            }
            if (result is ResultStatus)
            {

            }
            if (result is ResultDamage)
            {

            }
        }
    }

    private void ReadResultAbility(ResultAbility result)
    {
        if (result.Ability is MoveBasic) return; //Don't spam the log with basic movements

        //TODO: each ability has a tooltip popup that gives info on the ability including damage dice, effects, upgrades, etc

        logEntries.Add($"{result.CasterName} used {result.Ability.Name}. \n");
        foreach(Action action in result.ActionList)
        {
            if (action is ActionST) ReadAction(action as ActionST);
            if (action is ActionAOE)
            {
                foreach(ActionST ST in (action as ActionAOE).Targets)
                {
                    ReadAction(ST);
                }
            }

        }
    }

    private void ReadTargetting(ReportTargettingData report)
    {

    }

    private void ReadResultTargetting(ResultTargetting result)
    {

    }

    private void ReadResultMana(ResultRollMana result)
    {


    }

}
