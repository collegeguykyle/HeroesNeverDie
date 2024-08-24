using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Ability
{
    public abstract string Name { get; set; }
    public abstract Mana cost { get; set; }
    public abstract Unit OwningUnit { get; set; }
    public abstract int Range { get; set; } // range 0 = movement ability
    public abstract bool UseEngaged { get; set; }
    public abstract Team targets { get; set; }

    public Ability(Unit unit)
    {
        OwningUnit = unit;
    }

    public bool TestManaCost(Mana ManaPool)
    {
        return Mana.TryCost(ManaPool, cost);

    }

    public abstract List<IOccupyBattleSpace> TestValidTargets();
    public abstract void GetTargets(); //probably needs to return a list of units or targetables
    public abstract void ExecuteAbility();
}







