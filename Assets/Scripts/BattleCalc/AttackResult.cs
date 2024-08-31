using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackResult
{
    public int roll = 0;

    public bool hit = false;
    public bool crit = false;
    public bool critMiss = false;
    
    public int attackBonus = 0;

    public DefenseType defenseType;
    public int defenseValue = 0;



    public static AttackResult TryHit(Ability ability, Unit target)
    {
        AttackResult result = new AttackResult();

        result.attackBonus = ability.GetBonus;

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
