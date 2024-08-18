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

    public bool PassTurn = false;

    public Mana Mana = new Mana();

    public List<Ability> Abilities = new List<Ability>();
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

    public void TakeTurn()
    {
        Battle.SendStartUnitTurn(this);
        RollDice();
        while (PassTurn == false)
        {
            UseAbility(ChooseAbility());
        }
        EndTurn();
    }

    private Ability ChooseAbility()
    {

        return new Melee1(this); //temp
    }

    private void UseAbility(Ability ability)
    {

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


    private void EndTurn()
    {
        PassTurn = false;
        //reset mana
        Battle.SendEndUnitTurn(this);
    }

}
