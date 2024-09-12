using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    //This class has a dual purpose.
    //Purpose ONE: Holding data on how much damage an ability does and of what type
    public string Source;
    public DamageType damageType;
    public int NumberDice;
    public int SizeDice;
    public int DamageBonus;
    public List<DamageModifier> DamageModifierList = new List<DamageModifier>();
    //Purpose TWO: Storing data in clones of the result of the damage rolled and modified for Logging
    public List<int> RollResult = new List<int>();
    public int totalDamage = 0;
    public bool crit = false;
    public bool damageResist = false;
    public bool damageVulnerable = false;

    public Damage(string source, DamageType Type, int numberDice, int sizeDice)
    {
        Source = source;
        this.damageType = Type;
        NumberDice = numberDice;
        SizeDice = sizeDice;
    }
    public Damage(string source, DamageType Type, int numberDice, int sizeDice, int bonus)
    {
        Source = source;
        this.damageType = Type;
        NumberDice = numberDice;
        SizeDice = sizeDice;
        DamageBonus = bonus;
    }
    public Damage(string source, DamageType Type, int flatDamage)
    {
        Source = source;
        this.damageType = Type;
        NumberDice = 1;
        SizeDice = flatDamage;
        RollResult.Add(flatDamage);
        totalDamage = flatDamage;
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
    public int RollDamage(ResultHit attack)
    {
        RollResult.Clear();
        for (int i = 0; i < NumberDice; i++)
        {
            int num = Random.Range(0, SizeDice);
            RollResult.Add(num);
            totalDamage += num;
        }
        if (attack != null && attack.crit)
        {
            totalDamage = totalDamage * 2;
            crit = true;
        }
        totalDamage += DamageBonus;
        return totalDamage;
    }

    public static Damage Clone(Damage toClone)
    {
        Damage clone = new Damage(toClone.Source, toClone.damageType, toClone.NumberDice, toClone.SizeDice);
        return clone;
    }

}

public class DamageModifier
{
    int Amount;
    Reaction Source;
    public DamageModifier(int amount, Reaction source)
    {
        Amount = amount;
        Source = source;
    }

}

public enum DamageType
{
    Smashing, Slashing, Stabbing, Striking,
    Elemental, Fire, Ice, Lightning, Nature,
    Arcane, Holy, Shadow, Psychic
}