using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    private List<ToReport> Reports = new List<ToReport>();

    private List<Unit> PlayerTeam;
    private List<Unit> EnemyTeam;

    public List<ToReport> GetBattleReport()
    {
        return Reports;
    }
    public void AddReport(ToReport report)
    {
        Reports.Add(report);
    }
    public void SetTeams(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        PlayerTeam = playerTeam;
        EnemyTeam = enemyTeam;
    }
}

public abstract class ToReport
{
    
}

public class ReportStartTurn : ToReport
{
    public Unit unit;
    public ReportStartTurn(Unit unit)
    {
        this.unit = unit;
    }
}