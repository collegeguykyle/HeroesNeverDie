using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultRoll : EventArgs
{
    public Unit owner;
    public List<DieSide> rolledSides = new List<DieSide>();
    public List<DieSide> modifiedSides = new List<DieSide>();

    public Mana TotalMana()
    {
        Mana rolled = new Mana();
        foreach (DieSide side in modifiedSides)
        {
            rolled.AddMana(side.Mana);
        }
        return rolled;
    }
    public ResultRoll(Unit Owner)
    {
        this.owner = Owner;
    }
}