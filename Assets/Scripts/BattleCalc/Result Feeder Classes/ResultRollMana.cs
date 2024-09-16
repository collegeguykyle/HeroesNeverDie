using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class ResultRollMana : ToReport
{
    [JsonIgnore] public Unit owner;
    public string ownerName;
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
    public ResultRollMana(Unit Owner)
    {
        this.owner = Owner;
        this.ownerName = Owner.Name;
    }
}