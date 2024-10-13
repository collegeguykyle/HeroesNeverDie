using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToolTipKeyWord
{
    string GetTooltipText();  // Returns the text for the tooltip

}

//See ResultHit class for information that feeds this tooltip from IHit interface
public class HitRollTooltip : IToolTipKeyWord
{
    public ResultHit result;
    
    public HitRollTooltip(ResultHit result)
    {
        this.result = result;
    }

    public string GetTooltipText()
    {
        return $"To Hit Roll: {result.roll}{bonus()} = {result.roll + result.attackBonus} \n"
            + $"Defense Roll ({result.defenseType}): {result.defenseValue} \n"
            + $"{success()}";
    }

    private string bonus()
    {
        string text = "";
        foreach(ToHitBonus item in result.HitBonusList)
        {
            text += $" + {item.value} ({item.NameOfBonus})";
        }
        return text ;
    }

    private string success()
    {
        string text = "";
        if (result.success) text = "HIT";
        else text = "MISS";
        if (result.crit) text = "CRITICAL HIT!";
        if (result.critMiss) text = "Critcal MISS!";
        return text ;
    }
}

public class DamageRollTooltip : IToolTipKeyWord
{
    public ResultDamage result;

    public DamageRollTooltip(ResultDamage result)
    {
        this.result = result;
    }

    public string GetTooltipText()
    {
        string text = "";

        foreach(Damage Dam in result.DamageRolls)
        {
            text += $"{Dam.Source} ({Dam.NumberDice}D{Dam.SizeDice}) " +
                $"{mods(Dam)}" +
                $"= {Dam.totalDamage} {Dam.damageType} Damage \n";
        }

        return text;
    }

    private string mods(Damage dam)
    {
        string text = "";
        foreach(DamageModifier mod in dam.DamageModifierList)
        {
            text += $"+ {mod.Amount} ({mod.Source}) ";
        }
        if (dam.crit) text += "x2 (Critical) ";
        if (dam.damageResist) text += "x0.5 (Resistant) ";
        if (dam.damageVulnerable) text += "x1.5 (Vulnerable)";
        return text;
    }

}

public class SaveRollTooltip : IToolTipKeyWord
{
    public ResultSave result;

    public SaveRollTooltip(ResultSave result)
    {
        this.result = result;
    }

    public string GetTooltipText()
    {
        return $"Caster Bonus: {bonus()} = {result.magicBonus} \n"
            + $"Save Roll: {result.roll} + {result.statBonus} ({result.saveType}) = {result.totalSaveValue}\n"
            + $"{success()}";
    }

    private string bonus()
    {
        bool first = true;
        string text = "";
        foreach (MagicSkillBonus item in result.bonusList)
        {
            if(first) text += $"{item.value} ({item.NameOfBonus})";
            else text += $" + {item.value} ({item.NameOfBonus})";
        }
        return text;
    }

    private string success()
    {
        string text = "";
        if (result.success) text = "Save Passed";
        else text = "Save Failed";
        if (result.crit) text = "Save CRITICAL Success!";
        if (result.critMiss) text = "Save CRITICAL Failure!";
        return text;
    }
}
