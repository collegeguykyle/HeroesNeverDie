using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    //This class has a dual purpose.
    //Purpose ONE: Holding data on how much damage an ability does and of what type
    public string Source;
    public AttackType attackType;
    public int NumberDice;
    public int SizeDice;
    public int DamageBonus;
    //Purpose TWO: Storing data in clones of the result of the damage rolled and modified for Logging
    public List<int> RollResult = new List<int>();
    public int totalDamage = 0;
    public bool crit = false;
    public bool damageResist = false;
    public bool damageVulnerable = false;

    public Damage(string source, AttackType attackType, int numberDice, int sizeDice)
    {
        Source = source;
        this.attackType = attackType;
        NumberDice = numberDice;
        SizeDice = sizeDice;
    }

    public int GetAverageDamage()
    {
        int num = SizeDice + 1;
        float dieAvg = num / 2;
        int final = Mathf.RoundToInt(dieAvg * NumberDice);
        return final;
    }

    public int RollDamage()
    {
        RollResult.Clear();
        for (int i = 0; i< NumberDice; i++)
        {
            int num = Random.Range(0, SizeDice);
            RollResult.Add(num);
            totalDamage += num;
        }
        totalDamage += DamageBonus;
        return totalDamage;
    }

    public int ModifyDamage(ResultHit attack)
    {
        if (attack.crit)
        {
            totalDamage = totalDamage * 2;
            crit = true;
        }
        //TODO: Add Modifications for buffs / debuffs / resistences etc for Units and check for them here to modify damage

        return totalDamage;
    }

    public static Damage Clone(Damage toClone)
    {
        Damage clone = new Damage(toClone.Source, toClone.attackType, toClone.NumberDice, toClone.SizeDice);
        return clone;
    }

}