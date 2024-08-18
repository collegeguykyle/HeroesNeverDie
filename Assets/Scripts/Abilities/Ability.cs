using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Ability
{
    public string Name;
    public Mana cost;
    public Unit OwningUnit;
    public int Range;

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







