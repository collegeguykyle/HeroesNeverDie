using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


public class JSONTest : MonoBehaviour
{
    public void Start()
    {
        List<Unit> playerTeam = new List<Unit>();
        playerTeam.Add(createRandomUnit("Bob", Team.player));
        playerTeam.Add(createRandomUnit("Billy", Team.player));
        while (playerTeam[0].StartingCol == playerTeam[1].StartingCol 
            && playerTeam[0].StartingRow == playerTeam[1].StartingRow)
        {
            playerTeam[1].StartingRow = UnityEngine.Random.Range(1, 3);
            playerTeam[1].StartingCol = UnityEngine.Random.Range(1, 5);
        }

        List<Unit> enemyTeam = new List<Unit>();
        enemyTeam.Add(createRandomUnit("Mark", Team.enemy));
        enemyTeam.Add(createRandomUnit("Terry", Team.enemy));
        while (enemyTeam[0].StartingCol == enemyTeam[1].StartingCol
            && enemyTeam[0].StartingRow == enemyTeam[1].StartingRow)
        {
            enemyTeam[1].StartingRow = UnityEngine.Random.Range(1, 3);
            enemyTeam[1].StartingCol = UnityEngine.Random.Range(1, 5);
        }

        Battle calc = new Battle(playerTeam, enemyTeam);
        JsonSave(calc.BattleReport);
        print("Done");
    }

    private Unit createRandomUnit(String Name, Team team)
    {
        Unit unit = new Unit();
        unit.Name = Name;
        unit.Team = team;
        unit.MaxHP = UnityEngine.Random.Range(20, 40);
        unit.CurrentHP = unit.MaxHP;

        unit.PWR = UnityEngine.Random.Range(10, 50);
        unit.AGL = UnityEngine.Random.Range(10, 50);
        unit.INT = UnityEngine.Random.Range(10, 50);
        unit.ATN = UnityEngine.Random.Range(10, 50);
        unit.FTH = UnityEngine.Random.Range(10, 50);
        unit.LCK = UnityEngine.Random.Range(10, 50);

        unit.StartingRow = UnityEngine.Random.Range(1, 3);
        unit.StartingCol = UnityEngine.Random.Range(1, 5);

        unit.Init = UnityEngine.Random.Range(1, 50);

        unit.AddDie(BasicDice.Melee);
        unit.AddDie(BasicDice.Melee);
        unit.AddDie(BasicDice.Melee);

        Tactic BigMelee = new Tactic(unit, new Melee2(unit), TCondition.None, TCondition.None);   
        Tactic basicMeleeTactic = new Tactic(unit, new Melee1(unit), TCondition.None, TCondition.None);
        Tactic basicMove = new Tactic(unit, new MoveBasic(unit), TCondition.None, TCondition.None);
        unit.Tactics.Add(BigMelee);
        unit.Tactics.Add(basicMeleeTactic);
        unit.Tactics.Add(basicMove);
        
        return unit;
    }

    public static JsonSerializerSettings JSONSettings()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        NamingStrategy namingStrategy = new CamelCaseNamingStrategy(true, false);
        settings.Converters.Add(new StringEnumConverter(namingStrategy));
        return settings;
    }
    public static void JsonSave(BattleReport battleReport)
    {

        string filePath = "C:\\Users\\Colle\\Desktop\\saveFile.BtlR";
        var serializedObj = JsonConvert.SerializeObject(battleReport, Formatting.Indented, JSONSettings());
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.Write(serializedObj);
        }
        print("BattleReport generated at " + filePath);
    }

    public static BattleReport LoadJsonReplay(string fileName)
    {
        string filePath = "D:\\Unity Testing\\" + fileName;
        string content;
        using (StreamReader sr = new StreamReader(filePath))
        {
            content = sr.ReadToEnd();
        }

        BattleReport report = JsonConvert.DeserializeObject<BattleReport>(content, JSONSettings());
        return report;
    }

}

