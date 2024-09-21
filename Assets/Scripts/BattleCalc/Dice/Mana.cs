using System;
using System.Collections;
using System.Collections.Generic;


public enum ManaType { sword, shield, bow, magic, light, shadow, earth, wind, water, fire, nature, shock, ice, poison, gear, blank};


public class Mana
{
    public Dictionary<ManaType, int> count { get; private set; } = new Dictionary<ManaType, int>();

    public void ReportMana(BattleReport report)
    {
        string manaReport = "ManaReport:  ";
        
        foreach(ManaType type in count.Keys)
        {
            manaReport += type + ": " + count[type] + " | ";
        }
        report.AddReport(new ReportMessage(manaReport));
    }

    public string ManaAsString()
    {
        string manaReport = "ManaReport:  ";

        foreach (ManaType type in count.Keys)
        {
            manaReport += type + ": " + count[type] + " | ";
        }
        return manaReport;
    }
    
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
            if (count[type] <= 0) 
            { 
                count.Remove(type);
            }
        }
    }
    public void RemoveManaType(ManaType type)
    {
        if (count.ContainsKey(type))
        {
            count[type] -= 1;
            if (count[type] <= 0)
            {
                count.Remove(type);
            }
        }
    }

    public void AddMana(Mana mana)
    {
        foreach (ManaType type in mana.count.Keys)
        {
            AddManaType(type, mana.count[type]);
        }
    }

    public void RemoveMana(Mana mana)
    {
        foreach(ManaType type in mana.count.Keys)
        {
            RemoveManaType(type, mana.count[type]);
        }
    }

    public static bool TryCosts(Mana manaCost, Mana unitMana)
    {
        bool result = false;
        if (manaCost.count.Count == 0) return true;
        foreach (ManaType type in manaCost.count.Keys)
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
            else return false;
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