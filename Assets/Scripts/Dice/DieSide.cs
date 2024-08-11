using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieSide
{
    public string Name { get; protected set; } = "Blank";
    public Mana Mana { get; protected set; } = new Mana(); //Amount of mana you get if the side is rolled

    // [ ] Should we have unique sprites for dice sides, or create a modular system??

    //Dice Status Effects
    // bool pain = false; //When rolled take damage and remove this effect !!NOT IMPLIMENTED
    // bool agony = false; //When rolled take damage but effect is not removed !!NOT IMPLIMENTED
    // bool phased = false; //When rolled side does nothing but this effect is removed !!NOT IMPLIMENTED

    #region Constructors

    public DieSide(DieSide clone)
    {
        this.Name = clone.Name;
        this.Mana = clone.Mana;
    }
    public DieSide(string name, Mana mana)
    {
        Name = name;
        Mana = mana;
    }
    public DieSide(string name, ManaType mana1)
    {
        this.Name = name;
        Mana.AddManaType(mana1, 1);
    }
    public DieSide(string name, ManaType mana1, ManaType mana2)
    {
        this.Name = name;
        Mana.AddManaType(mana1, 1);
        Mana.AddManaType(mana2, 1);
    }
    public DieSide(string name, ManaType mana1, ManaType mana2, ManaType mana3)
    {
        this.Name = name;
        Mana.AddManaType(mana1, 1);
        Mana.AddManaType(mana2, 1);
        Mana.AddManaType(mana3, 1);
    }
    public DieSide(string name, ManaType mana1, int amount1)
    {
        this.Name = name;
        Mana.AddManaType(mana1, amount1);
    }
    public DieSide(string name, ManaType mana1, int amount1, ManaType mana2, int amount2)
    {
        this.Name = name;
        Mana.AddManaType(mana1, amount1);
        Mana.AddManaType(mana2, amount2);
    }
    public DieSide(string name, ManaType mana1, int amount1, ManaType mana2, int amount2, ManaType mana3, int amount3)
    {
        this.Name = name;
        Mana.AddManaType(mana1, amount1);
        Mana.AddManaType(mana2, amount2);
        Mana.AddManaType(mana3, amount3);
    }

    #endregion

}

public enum DieSides { Melee1, Melee2, Shield1, Shield2, Ranged1, Ranged2, Magic1, Magic2 }
//This enum must be in the EXACT same order as the Scriptable Object Side Prefab List