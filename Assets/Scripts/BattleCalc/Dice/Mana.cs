using System;
using System.Collections;
using System.Collections.Generic;


public enum ManaType { sword, shield, bow, magic, light, shadow, earth, wind, water, fire, nature, shock, ice, poison, gear, };


public class Mana
{
    public Dictionary<ManaType, int> count { get; private set; } = new Dictionary<ManaType, int>();

    public void AddManaType(ManaType type, int num)
    {
        if (count.ContainsKey(type))
        {
            count[type] += num;
        }
        else
        {
            count.Add(type, num);
        }
    }
    public void AddManaType(ManaType type)
    {
        if (count.ContainsKey(type))
        {
            count[type] += 1;
        }
        else
        {
            count.Add(type, 1);
        }
    }

    public void RemoveManaType(ManaType type, int num)
    {
        if (count.ContainsKey(type))
        {
            count[type] -= num;
            if (count[type] < 0) 
            { 
                count[type] = 0; 
            }
        }
    }
    public void RemoveManaType(ManaType type)
    {
        if (count.ContainsKey(type))
        {
            count[type] -= 1;
            if (count[type] < 0)
            {
                count[type] = 0;
            }
        }
    }

    public void AddMana(Mana mana)
    {
        foreach (ManaType type in mana.count.Values)
        {
            int num; 
            mana.count.TryGetValue(type, out num);
            AddManaType(type, num);
        }
    }

    public void RemoveMana(Mana mana)
    {
        foreach(ManaType type in mana.count.Values)
        {
            int num;
            mana.count.TryGetValue(type, out num);
            RemoveManaType(type, num);
        }
    }

    public static bool TryCost(Mana manaCost, Mana unitMana)
    {
        if (manaCost.count.Count == 0) return true;
        
        bool result = false;
        foreach (ManaType type in manaCost.count.Values)
        {
            if (unitMana == null)
            {
                return false;
            }
            if (unitMana.count.ContainsKey(type))
            {
                if (unitMana.count[type] >= manaCost.count[type]) result = true;
                else return false;
            }
        }
        return result;
    }

    public int GetCountType(ManaType type)
    {
        if (!count.ContainsKey(type))
        {
            return 0;
        }
        return count[type];
    }

}