using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ResultDamage
{



    public static ResultDamage RollDamage(ResultHit attack)
    {
        ResultDamage result = new ResultDamage();

        //take in min and max damage

        //roll the damage

        //increase damage based on weaknesses / buffs

        //mitigate the damage based on resistances / immunities

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