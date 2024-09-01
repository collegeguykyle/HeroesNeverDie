using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Team { player, enemy, neutral, terrain, other }
public class Unit : IOccupyBattleSpace
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

    private Battle Battle;

    public Team Team { get; private set; }

    public void BattleStart(Battle battle)
    {
        Battle = battle;
        foreach (Reaction r in Reactions)
        {
            if (r.ReactionType == BattleReaction.StartOfCombat) r.DoReaction();
            else battle.AddReaction(r);
        }
    }


    public void RollDice()
    {
        List<DieSide> rolledSides = new List<DieSide>();
        //for each dice in the dice list, roll it and add its mana to the mana pool
        foreach(Dice dice in DiceList)
        {
            int rand = Random.Range(0, dice.Sides.Count);
            DieSide side = dice.Sides[rand];
            Battle.Reactions.SendDieRolled(side);
            rolledSides.Add(side);
        }
        Mana rolled = new Mana();
        foreach(DieSide side in rolledSides)
        {
            rolled.AddMana(side.Mana);
        }
        Battle.Reactions.SendRollResult(rolled);
        Mana.AddMana(rolled);
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


    public void EndTurn()
    {
        //reset mana
        ManaLost = Mana; //stored mana lost for reference by certain end of turn reactions that need this information
        Mana = new Mana();

    }

}
