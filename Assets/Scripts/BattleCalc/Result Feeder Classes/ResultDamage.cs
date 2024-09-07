using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ResultDamage
{
    public int TotalDamage;
    public List<Damage> Damages;

    public static ResultDamage RollDamage(ResultHit attack, IDealDamage ability)
    {
        ResultDamage result = new ResultDamage();

        foreach (Damage damage in ability.damageDice)
        {
            Damage clone = Damage.Clone(damage);
            result.Damages.Add(clone);
            clone.RollDamage();
            clone.ModifyDamage(attack);
            result.TotalDamage += clone.totalDamage;
        }
        return result;
    }


}

public interface IDealDamage
{
    public abstract List<Damage> damageDice { get; }

    public abstract ResultDamage SendDamage(List<Damage> damage);


}


// TO HIT: attack bonus: each ability has multipliers tied to base stats, total up for attack bonus
// DEFENSE: Best option of:
//      Dodge = melee and ranged, phys and magic
//      Block = melee and ranged, phys and magic
//      Parry = melee only, phys only
//      Aura = ranged only, magic only

//IF HIT: Check for damage reductions / immunities and thresholds
//DAMAGE TYPES: 
//      Physical: Smashing, Slashing, Stabbing, Striking
//      Elemental: Nature, Fire, Ice, Lightning
//      Magical: Arcane, Holy, Shadow, Force