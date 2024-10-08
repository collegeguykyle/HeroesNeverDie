using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;



public enum Team { player, enemy, neutral, terrain, other }
public class Unit : EventArgs, IOccupyBattleSpace
{
    [JsonIgnore] public Battle Battle { get; protected set; }
    public string Name { get; set; } = "Hero Name";
    public Team Team { get; set; }

    public int MaxHP = 10;
    public int CurrentHP = 10;
    
    public int MaxMove = 30; //Moving 1 square costs 10, 15 for diaganol
    public int CurrentMove = 30;

    public int PWR = 1;
    public int AGL = 1;
    public int INT = 1;
    public int ATN = 1;
    public int FTH = 1;
    public int LCK = 1;

    [JsonIgnore] public int StartingRow = 0;
    [JsonIgnore] public int StartingCol = 0;
    public int Init = 0;    

    public List<Dice> DiceList = new List<Dice>();
    [JsonIgnore] public List<Tactic> Tactics = new List<Tactic>();
    public List<Status> statusList { get; protected set; } = new List<Status>();

    public Mana Mana = new Mana();
    public Mana ManaLost = new Mana(); 

    public void BattleStart(Battle battle)
    {
        Battle = battle;
        foreach (Status status in statusList)
        {
            if (status is IReactStartBattle) (status as IReactStartBattle).onStartBattle();
            else status.Subscribe(battle);
        }
    }

    public void AddDie(Dice dice)
    {
        Dice die = new Dice();
        foreach(DieSide side in dice.Sides)
        {
            die.AddSide(side);
        }
        DiceList.Add(die);
    }

    public ResultRollMana RollManaDice()
    {
        ResultRollMana result = new ResultRollMana(this);
        //for each dice in the dice list, roll it and add its mana to the mana pool
        foreach(Dice dice in DiceList)
        {
            int rand = UnityEngine.Random.Range(0, dice.Sides.Count);
            DieSide side = dice.Sides[rand];
            result.rolledSides.Add(side);
        }
        return result;
    }

    public int GetDodge()
    {
        return AGL; //TODO: Return class that contains a full list of all buffs that add to this stat for logging
    }
    public int GetBlock()
    {
        return PWR; //TODO: Return class that contains a full list of all buffs that add to this stat for logging
    }
    public int GetParry()
    {
        return AGL; //TODO: Return class that contains a full list of all buffs that add to this stat for logging
    }
    public int GetAura()
    {
        return ATN; //TODO: Return class that contains a full list of all buffs that add to this stat for logging
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP <= 0) Battle.UnitKilled(this);
    }

    public void ClearMana()
    {
        //reset mana
        ManaLost = Mana; //stored mana lost for reference by certain end of turn reactions that need this information
        Mana = new Mana();

    }

}
