using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    public string Name { get; private set; } = "Blank Die";
    public List<DieSide> Sides { get; private set; } = new List<DieSide>();
    public void AddSide(DieSide side)
    {
        DieSide clone = new DieSide(side);
        Sides.Add(clone);
        clone.OwningDie = this;
    }
    public Dice() { }
    public Dice(string name, DieSide side1, DieSide side2, DieSide side3, DieSide side4, DieSide side5, DieSide side6)
    {
        Name = name;
        Sides.Add(side1);
        Sides.Add(side2);
        Sides.Add(side3);
        Sides.Add(side4);
        Sides.Add(side5);
        Sides.Add(side6);
    }
}

public static class BasicDice
{
    public static Dice Melee = new Dice("Basic Melee", 
        BasicDieSides.Riposte1_1,
        BasicDieSides.Melee1,
        BasicDieSides.Melee1,
        BasicDieSides.Defend1,
        BasicDieSides.Defend1,
        BasicDieSides.Blank);

    public static Dice Ranged = new Dice("Basic Ranged",
        BasicDieSides.Ranged2,
        BasicDieSides.Ranged1,
        BasicDieSides.Ranged1,
        BasicDieSides.Defend1,
        BasicDieSides.Defend1,
        BasicDieSides.Blank);
}

public static class BasicDieSides
{
    public static DieSide Blank {  get; private set; } = new DieSide("Blank", ManaType.blank);

    public static DieSide Melee1 { get; private set; } = new DieSide("Melee 1", ManaType.sword);
    public static DieSide Melee2 { get; private set; } = new DieSide("Melee 2", ManaType.sword, 2);
    public static DieSide Melee3 { get; private set; } = new DieSide("Melee 3", ManaType.sword, 3);

    public static DieSide Defend1 { get; private set; } = new DieSide("Defend 1", ManaType.shield);
    public static DieSide Defend2 { get; private set; } = new DieSide("Defend 2", ManaType.shield, 2);
    public static DieSide Defend3 { get; private set; } = new DieSide("Defend 3", ManaType.shield, 3);

    public static DieSide Riposte1_1 { get; private set; } = new DieSide("Riposte 1/1", ManaType.sword, ManaType.shield);
    public static DieSide Riposte2_1 { get; private set; } = new DieSide("Riposte 2/1", ManaType.sword, 2, ManaType.shield, 1);
    public static DieSide Riposte1_2 { get; private set; } = new DieSide("Riposte 1/2", ManaType.sword, 1, ManaType.shield, 2);

    public static DieSide Ranged1 { get; private set; } = new DieSide("Ranged 1", ManaType.bow);
    public static DieSide Ranged2 { get; private set; } = new DieSide("Ranged 2", ManaType.bow, 2);
    public static DieSide Ranged3 { get; private set; } = new DieSide("Ranged 3", ManaType.bow, 3);

    public static DieSide Magic1 { get; private set; } = new DieSide("Magic 1", ManaType.magic);
    public static DieSide Magic2 { get; private set; } = new DieSide("Magic 2", ManaType.magic, 2);
    public static DieSide Magic3 { get; private set; } = new DieSide("Magic 3", ManaType.magic, 3);

}



