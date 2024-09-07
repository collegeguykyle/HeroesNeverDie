using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultHit
{
    public Ability Ability;
    public Unit Target;

    public int roll = 0;

    public bool hit = false;
    public bool crit = false;
    public bool critMiss = false;
    
    public int attackBonus = 0;

    public DefenseType defenseType;
    public int defenseValue = 0;

    public ResultHit(Ability ability, Unit target)
    {
        Ability = ability;
        Target = target;
    }

    public static ResultHit TryHit(IHit ability, Unit target)
    {
        ResultHit result = new ResultHit((ability as Ability), target);

        result.attackBonus = ability.GetAttackBonus();

        //Choose which defense value is the best against the attack
        int dodge = target.GetDodge();
        int block = target.GetBlock();
        int parry = target.GetParry();
        int aura = target.GetAura();

        if (ability.GetAttackTypes.Contains(AttackType.Ranged)) { parry = 0; }
        if (!ability.GetAttackTypes.Contains(AttackType.Physical)) { parry = 0; }
        if (ability.GetAttackTypes.Contains(AttackType.Melee)) { aura = 0; }
        if (!ability.GetAttackTypes.Contains(AttackType.Magic)) { aura = 0; }

        List<int> defenses = new List<int> { parry, dodge, block, aura };
        defenses = defenses.OrderByDescending(x => x).ToList();
        int BestDefense = defenses[0];

        if (BestDefense == dodge) result.defenseType = DefenseType.Dodge;
        else if (BestDefense == block) result.defenseType = DefenseType.Block;
        else if (BestDefense == parry) result.defenseType = DefenseType.Parry;
        else if (BestDefense == aura) result.defenseType = DefenseType.Aura;

        result.roll = Random.Range(1, 100);
        if (result.roll >= 99) result.crit = true;
        if (result.roll <= 2) result.critMiss = true;
        if (result.crit || result.roll + result.attackBonus > BestDefense) result.hit = true;

        return result;

    }
}

public interface IHit
{
    public abstract List<AttackType> GetAttackTypes { get; }
    public abstract int NumberOfAttacks { get; }
    public abstract List<ToHitBonus> ToHitBonus { get; }
    public abstract int GetAttackBonus();
}

// TO HIT: attack bonus: each ability has multipliers tied to base stats, total up for attack bonus
// DEFENSE: Best option of:
//      Dodge = melee and ranged, phys and magic
//      Block = melee and ranged, phys and magic
//      Parry = melee only, phys only
//      Aura = ranged only, magic only

//IF HIT: Check for damage reductions / immunities and thresholds
//DAMAGE TYPES: 
//      Physical: Smashing, Slashing, Stabbing
//      Elemental: Nature, Fire, Ice, Lightning
//      Magical: Arcane, Holy, Shadow, Force
