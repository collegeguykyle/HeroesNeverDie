using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LogGenerator
{
    private static List<BattleLogEntry> logEntries = new List<BattleLogEntry>();
    public static List<BattleLogEntry> GetLog(BattleReport battleReport, bool debugMessages)
    {
        
        foreach (ToReport report in battleReport.Reports)
        {
            ReadReport(report, debugMessages);
        }
        return logEntries;
    }

    private static void ReadReport(ToReport report, bool debugMessages)
    {
        if (report is ReportMessage && debugMessages) ReadMessage(report as ReportMessage);
        if (report is ReportStartTurn) ReadStartTurn(report as ReportStartTurn);
        if (report is ResultRollMana) ReadResultMana(report as ResultRollMana);
        if (report is ReportEndTurn) ReadEndTurn(report as ReportEndTurn);
        if (report is ReportStartRound) ReadStartRound(report as ReportStartRound);
        if (report is ReportUnitDeath) ReadUnitDeath(report as ReportUnitDeath);
        if (report is ReportEndBattle) ReadEndBattle(report as ReportEndBattle);
        if (report is Action) Debug.Log("Action in the battle Report instead of nested in an ability");
        if (report is ResultAbility) ReadResultAbility(report as ResultAbility);        
    }

    private static void ReadMessage(ReportMessage report)
    {
        logEntries.Add(new BattleLogEntry("<color=#7a97b2>" + report.message + "</color>"));
    }

    private static void ReadStartTurn(ReportStartTurn report)
    {
        logEntries.Add(new BattleLogEntry("\n<color=#ffb90f>Starting " + report.unitName + "'s Turn. </color>"));
    }

    private static void ReadResultMana(ResultRollMana result)
    {
        Mana mana = result.TotalMana(); //TODO: May want to update this with tooltips to show the dice sides in tooltips
        string text = $"{result.ownerName} rolled mana: ";
        foreach(ManaType type in mana.count.Keys)
        {
            text += $"{type} x{mana.GetCountType(type)}, ";
        }
        text += "";
        logEntries.Add(new BattleLogEntry(text));
    }

    private static void ReadEndTurn(ReportEndTurn report)
    {
        //No end of turn battle log entry required.
        //logEntries.Add("<color=#ffb90f>Ending " + report.unitName + "'s Turn. </color>\n");
    }

    private static void ReadStartRound(ReportStartRound report)
    {
        logEntries.Add(new BattleLogEntry("<color=#ffb90f>Starting Round: " + report.round + "</color>"));
    }

    private static void ReadUnitDeath(ReportUnitDeath report)
    {
        logEntries.Add(new BattleLogEntry("<color=#cd000>     " + report.unitKilled + " has been slain! </color>"));
    }

    private static void ReadEndBattle(ReportEndBattle report)
    {
        logEntries.Add(new BattleLogEntry("<color=#cd000>" + report.Victors + " TEAM WINS!!! </color>"));
    }

    private static void ReadResultAbility(ResultAbility result)
    {
        //TODO: each ability has a tooltip popup that gives info on the ability including damage dice, effects, upgrades, etc
        if (result.Ability is MoveBasic) return; //dont spam the log with basic movements

        logEntries.Add(new BattleLogEntry($"{result.CasterName} used {result.Ability.Name}."));
        foreach(Action action in result.ActionList)
        {
            string text = "    " + action.BattleLogEntry.message;
            action.BattleLogEntry.message = text;
            if(action.BattleLogEntry.showInLog) logEntries.Add(action.BattleLogEntry);
        }
    }
}

public class BattleLogEntry
{
    public bool showInLog = true;
    public string message;  // The raw log message
    public List<KeywordTooltip> keywordTooltips = new List<KeywordTooltip>();
    public BattleLogEntry(string message)
    {
        this.message = message;
    }

    // Method to add a keyword and associated tooltip
    public void AddKeywordTooltip(string keyword, IToolTipKeyWord tooltip)
    {
        keywordTooltips.Add(new KeywordTooltip(keyword, tooltip));
    }
}

public class KeywordTooltip
{
    public string keyword;  // The keyword to be replaced with a link
    public Color keywordColor;
    public IToolTipKeyWord tooltip;  // The associated tooltip object

    public KeywordTooltip(string keyword, IToolTipKeyWord tooltip)
    {
        this.keyword = keyword;
        this.tooltip = tooltip;
    }
}

public static class logHelpers
{

    public static BattleLogEntry HitForDamage(string targetName, ResultHit resultHit, ResultDamage damageResult)
    {
        BattleLogEntry entry = new BattleLogEntry("");
        if (resultHit.success)
        {
            entry.message = $"{targetName} was hit for {damageResult.TotalDamage} damage.";
            entry.AddKeywordTooltip("hit", new HitRollTooltip(resultHit));
            entry.AddKeywordTooltip($"{damageResult.TotalDamage} damage", new DamageRollTooltip(damageResult));
        }
        else
        {
            entry.message = $"{targetName} {resultHit.defenseType}ed an attack.";
            entry.AddKeywordTooltip($"{resultHit.defenseType}ed", new HitRollTooltip(resultHit));
        }
        return entry;
    }

    public static BattleLogEntry HitMiss(string targetName, ResultHit resultHit)
    {
        BattleLogEntry entry = new BattleLogEntry("");
        {
            entry.message = $"{targetName} {resultHit.defenseType}ed an attack.";
            entry.AddKeywordTooltip($"{resultHit.defenseType}ed", new HitRollTooltip(resultHit));
        }
        return entry;
    }

}