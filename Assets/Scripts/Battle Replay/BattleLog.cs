using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLog 
{
    public List<ILoggable> Log;




}

public interface ILoggable
{
    public void Log(string message);
}



public enum HitResult { hit, crit, miss, dodge, blocked,  }

public class Attack : ILoggable
{
    public Unit Attacker;
    public Unit Target;

    public int ToHitRoll;
    public List<ToHitBonus> ToHitBonuses;
    
    public void Log(string message)
    {
        throw new System.NotImplementedException();
    }
}

public struct ToHitBonus
{
    string BonusSource;
    int Bonus;
}
