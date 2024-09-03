using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    public string Source;
    public AttackType attackType;
    public int NumberDice;
    public int SizeDice;
    public int DamageBonus;
    public List<int> RollResult = new List<int>();

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

    public Damage RollDamage()
    {
        for (int i = 0; i< NumberDice; i++)
        {
            int num = Random.Range(0, SizeDice);
            RollResult.Add(num);
        }
        return this;
    }

}