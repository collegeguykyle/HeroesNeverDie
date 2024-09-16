using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability
{
    public abstract string Name { get; protected set; }
    public abstract Mana cost { get; protected set; }
    public abstract int Range { get; protected set; } 
    public abstract bool UseEngaged { get; protected set; }
    public abstract Team targets { get; protected set; }

    [JsonIgnore] public Unit OwningUnit { get; protected set; }
    public string OwningUnitName;

    //TODO: How send Attack Result and Ability Complete result??

    public Ability(Unit OwningUnit)
    {
        this.OwningUnit = OwningUnit;
        OwningUnitName = OwningUnit.Name;
    }

    public bool TestManaCost(Mana ManaPool)
    {
        return Mana.TryCost(ManaPool, cost);

    }

    public abstract void ExecuteAbility(ResultTargetting TargettingData);




}









