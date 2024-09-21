using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using Newtonsoft.Json;

public class BattleReport
{
    //This class is what takes in the data needed from the Battle Calculator to create a battle replay.
    //A different class will then save this object to a JSON file for submission to a player client
    //which will de-serialize the JSON and utilize this class object to generate the replay and battle log

    //What does the replay want to know??

    //What units are going to fight on each team, what are their stats, etc.
    //Where do they start?
    //What is the turn order?

    //Any time a unit does something: does an action / reaction, when a status/reaction triggers or is applied/removed
    //      when a unit moves or gets moved, attacks, etc. Starts its turn, ends its turn, dies, etc.

    //really thats it, just need to log in order whenever something happens so the replay can then show the animations
    //for those things happening and update the UI and on screen battle log.
    public int TotalTurns;
    public Team Victors;
    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;

    public List<ToReport> Reports { get; protected set; } = new List<ToReport>();

    public void AddReport(ToReport report)
    {
        Reports.Add(report);
    }
    public void SetTeams(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
    }

    public List<Unit> GetPlayerTeam() => PlayerTeam;
    public List<Unit> GetEnemyTeam() => EnemyTeam;

}

public abstract class ToReport : EventArgs
{
    
}

public class ReportMessage : ToReport
{
    public string message;
    public ReportMessage(string message)
    {
        this.message = message;
    }
}

public class ReportStartTurn : ToReport
{
    [JsonIgnore] public Unit unit;
    public string unitName;
    public ReportStartTurn(Unit unit)
    {
        this.unit = unit;
        unitName = unit.Name;
    }
}

public class ReportTargettingData : ToReport
{
    [JsonIgnore] public Unit caster;
    public string casterName;
    public BattleSpace casterSpace;
    public List<TargetData> targetDataAll;
    public ReportTargettingData(Unit caster, BattleSpace casterSpace, List<TargetData> targets)
    {
        this.caster = caster;
        this.casterName = caster.Name;
        this.casterSpace = casterSpace;
        this.targetDataAll = targets;
    }
}

public class ReportEndTurn : ToReport
{
    [JsonIgnore] public Unit unit;
    public string unitName;
    //manaLost
    //manaRetained
    public ReportEndTurn(Unit unit)
    {
        this.unit = unit;
        unitName = unit.Name;
    }   
}

public class ReportStartRound : ToReport
{
    public int round;
    public ReportStartRound(int round)
    {
        this.round = round;
    }
}

public class ReportUnitDeath : ToReport
{
    public Unit unitKilled;
    public ReportUnitDeath(Unit unitKilled)
    {
        this.unitKilled = unitKilled;
    }
}

public class ReportEndBattle : ToReport
{
    public Team Victors;
    public ReportEndBattle(Team victors)
    {
        Victors = victors;
    }   
}