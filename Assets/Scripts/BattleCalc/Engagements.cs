using System;
using System.Collections.Generic;

public class Engagements
{
    private List<Engagement> Elist = new List<Engagement>();
    public void AddEngagement(Unit Attacker, Unit Victim)
    {
        foreach (Engagement E in Elist)
        {
            if (!E.Compare(Attacker, Victim)) Elist.Add(new Engagement(Attacker, Victim));
        }
    }

    public void RemoveEngagement(Unit Attacker, Unit Victim)
    {
        foreach (Engagement E in Elist)
        {
            if (E.Compare(Attacker, Victim)) Elist.Remove(E);
        }
    }

    public bool TestEngaged(Unit unit)
    {
        bool isEngaged = false;
        foreach (Engagement E in Elist)
        {
            if (E.Victim == unit) isEngaged = true;
        }
        return isEngaged;
    }
}

public class Engagement
{
    public Engagement(Unit attacker, Unit victim)
    {
        Attacker = attacker;
        Victim = victim;
    }

    public Unit Attacker;
    public Unit Victim;

    public bool Compare(Unit attacker, Unit victim)
    {
        if (attacker == Attacker && victim == Victim) return true;
        else return false;
    }
}