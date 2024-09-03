using System;
using System.Collections;
using System.Collections.Generic;



public enum Team { player, enemy, neutral, terrain, other }
public class Unit : EventArgs, IOccupyBattleSpace
{
    public string Name = "Hero Name";

    public int MaxHP = 10;
    public int CurrentHP = 10;

    public int MaxActions = 1;
    public int CurrentActions = 1;

    public int MaxSwift = 1;
    public int CurrentSwift = 1;
    
    public int MaxMove = 3;
    public int CurrentMove = 3;

    public int PWR = 1;
    public int AGL = 1;
    public int INT = 1;
    public int ATN = 1;
    public int FTH = 1;
    public int LCK = 1;

    public Mana Mana = new Mana();
    public Mana ManaLost = new Mana();

    List<Dice> DiceList = new List<Dice>();
    public List<Ability> Abilities = new List<Ability>(); //Delete this? Only used in shop phase, which will have a seperate unit class
    public List<Tactic> Tactics = new List<Tactic>();
    public List<Reaction> Reactions = new List<Reaction>();

    public int StartingRow = 0;
    public int StartingCol = 0;
    public int Init = 0;

    public Battle Battle { get; protected set; }

    public Team Team { get; private set; }

    public void BattleStart(Battle battle)
    {
        Battle = battle;
        foreach (Reaction r in Reactions)
        {
            if (r is ReactionStartBattle) (r as ReactionStartBattle).onEvent();
            else r.Subscribe(battle);
        }
    }


    public void RollDice()
    {
        ResultRoll result = new ResultRoll(this);
        //for each dice in the dice list, roll it and add its mana to the mana pool
        foreach(Dice dice in DiceList)
        {
            int rand = UnityEngine.Random.Range(0, dice.Sides.Count);
            DieSide side = dice.Sides[rand];
            result.rolledSides.Add(side);
        }
        Battle.Reactions.SendDieRolled(result); //reactions that modify the faces of the dice after they are rolled
        Battle.Reactions.SendRollResult(result); //reactions that do things based on final roll result
        Mana.AddMana(result.TotalMana());
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
        if (CurrentHP <= 0) Battle.Reactions.SendUnitDeath(this);
    }

    public void ClearMana()
    {
        //reset mana
        ManaLost = Mana; //stored mana lost for reference by certain end of turn reactions that need this information
        Mana = new Mana();

    }

}
