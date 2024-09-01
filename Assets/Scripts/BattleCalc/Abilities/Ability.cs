using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Ability
{
    public abstract string Name { get; protected set; }
    public abstract Mana cost { get; protected set; }
    public abstract Unit OwningUnit { get; protected set; }
    public abstract int Range { get; protected set; } // range 0 = movement ability
    public abstract int AOESize { get; protected set; }
    public abstract bool UseEngaged { get; protected set; }
    public abstract Team targets { get; protected set; }
    public abstract List<AttackType> GetAttackTypes { get; protected set; }
    public abstract int GetBonus { get; protected set; }

    public Ability(Unit unit)
    {
        OwningUnit = unit;
    }

    public bool TestManaCost(Mana ManaPool)
    {
        return Mana.TryCost(ManaPool, cost);

    }
    
    
    public abstract void ExecuteAbility(ResultTargetting TargettingData);
}

public enum AttackType { Ranged, Melee, Physical, Smashing, Slashing, Stabbing, 
    Elemental, Fire, Ice, Lightning, Nature, Magic, Arcane, Holy, Shadow, Force }

public enum DefenseType { Dodge, Block, Parry, Aura }

public enum AbilityType { Attack, DeBuff, Move, Buff }



