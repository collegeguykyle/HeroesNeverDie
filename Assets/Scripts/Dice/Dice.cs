using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    public string Name { get; private set; } = "Blank Die";
    public List<DieSides> Sides {  get; private set; } = new List<DieSides>();


    #region Basic Die Sides
    // **** NOTE: I changed to using prefabs for DieSides to enable setting sprites for them in the inspector
    public static DieSide Melee1 { get; private set; } = new DieSide("Melee 1", ManaType.sword);
    public static DieSide Melee2 { get; private set; } = new DieSide("Melee 2", ManaType.sword, 2);
    public static DieSide Melee3 { get; private set; } = new DieSide("Melee 3", ManaType.sword, 3);

    public static DieSide Riposte1_1 { get; private set; } = new DieSide("Riposte 1/1", ManaType.sword, ManaType.shield);
    public static DieSide Riposte2_1 { get; private set; } = new DieSide("Riposte 2/1", ManaType.sword, 2, ManaType.shield, 1);
    public static DieSide Riposte1_2 { get; private set; } = new DieSide("Riposte 1/2", ManaType.sword, 1, ManaType.shield, 2);

    #endregion



}

