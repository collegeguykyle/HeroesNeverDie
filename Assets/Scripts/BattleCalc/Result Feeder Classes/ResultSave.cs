using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultSave : ActionResult
{
    //This is used if the attack uses a saving throw to determine if something hits instead of a to Hit roll
    public Ability ability;
    public Unit Target;
    public SaveType saveType;

    public int magicBonus = 0;

    public int roll = 0;
    public int statBonus = 0;
    public int totalSaveValue = 0;

    public bool success = false;
    public bool crit = false;
    public bool critMiss = false;


    public ResultSave(Ability ability, Unit target)
    {
        this.ability = ability;
        Target = target;
    }

    public static ResultSave TrySave(ITestSave ability, Unit target)
    {
        ResultSave result = new ResultSave((ability as Ability), target);

        result.magicBonus = ability.GetMagicBonus();
        result.saveType = ability.GetSaveType;
        
        result.roll = UnityEngine.Random.Range(1, 100);
        if (result.roll >= 99) result.crit = true;
        if (result.roll <= 2) result.critMiss = true;
       
        switch (ability.GetSaveType)
        {
            case SaveType.PWR: result.statBonus = target.PWR; break;
            case SaveType.AGL: result.statBonus = target.AGL; break;
            case SaveType.INT: result.statBonus = target.INT; break;
            case SaveType.ATN: result.statBonus = target.ATN; break;
            case SaveType.FTH: result.statBonus = target.FTH; break;
            case SaveType.LCK: result.statBonus = target.LCK; break;
        }

        result.totalSaveValue = result.roll + result.statBonus;
        if (result.totalSaveValue > result.magicBonus || result.crit == true) result.success = true;

        return result;

    }

}


public interface ITestSave
{
    public SaveType GetSaveType { get; }
    public List<MagicSkillBonus> MagicSkillBonus { get; }
    public int GetMagicBonus();
}

public class MagicSkillBonus
{
    String NameOfBonus;
    String SourceOfBonus;
    int value;
}

public enum SaveType { PWR, AGL, INT, ATN, FTH, LCK }