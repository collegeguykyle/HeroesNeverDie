using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Team { player, enemy, neutral, terrain, other }
public class Unit : IOccupyBattleSpace
{
    public string Name = "Hero Name";

    public int MaxHP = 10;
    public int CurrentHP = 10;

    List<Dice> DiceList = new List<Dice>();

    public int Actions = 1;
    public int SwiftActions = 1;

    public Mana Mana = new Mana();

    public List<Ability> Abilities = new List<Ability>();
    public List<Tactic> Tactics = new List<Tactic>();
    public List<Reaction> Reactions = new List<Reaction>();

    public int StartingRow = 0;
    public int StartingCol = 0;
    public int Init = 0;

    public Battle Battle;

    public Team Team { get; private set; }

    public void BattleStart(Battle battle)
    {
        Battle = battle;
        // [ ] Do start of battle conditons and actions
    }


    private Ability ChooseAbility()
    {
        // [ ] cylce through abilities in the list until we find one that works
        foreach (Ability ability in Abilities) 
        {
            bool manaToCast = ability.TestManaCost(Mana);
            
        
        }


        // [ ] Step 2: melee or ranged?


        // [ ] Step 3: cast locations in move range?


        // [ ] use the ability

        
        // [ ] if no abilities can be used then pass the turn

        return new Melee1(this); //temp
    }


    public void RollDice()
    {
        List<DieSide> rolledSides = new List<DieSide>();
        //for each dice in the dice list, roll it and add its mana to the mana pool
        foreach(Dice dice in DiceList)
        {
            int rand = Random.Range(0, dice.Sides.Count);
            DieSide side = dice.Sides[rand];
            Battle.SendDieRolled(side);
            rolledSides.Add(side);
        }
        Mana rolled = new Mana();
        foreach(DieSide side in rolledSides)
        {
            rolled.AddMana(side.Mana);
        }
        Battle.SendRollResult(rolled);
        Mana.AddMana(rolled);
    }


    public void EndTurn()
    {

        //reset mana

    }

}
