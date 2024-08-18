using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tactic
{
    private Unit Owner;
    private Ability Ability;
    private TCondition Condition1;
    private TCondition Condition2;

    public Tactic(Unit owner, Ability ability, TCondition condition1, TCondition condition2)
    {
        Owner = owner;
        Ability = ability;
        Condition1 = condition1;
        Condition2 = condition2;
    }

    public bool TestTactic(Mana AvailableMana, BattleSpacesController Map)
    {
        
        if (
            Ability.TestManaCost(AvailableMana)     //enough mana?
            && TestTargets(Map)                     //any targets in range?
            && TestSelfCondition(Condition1)
            && TestSelfCondition(Condition2)        //any exlusionary conditions?
            )
            return true; //then this ability should be used

            else return false; 
    }

    public void ChooseTarget()
    {
        //if ability should be used, where should it cast?
    }

    private bool TestTargets(BattleSpacesController Map) //return true if an enemy is in range of ability
    {


        return false;
    }

    private bool TestSelfCondition(TCondition condition) //return true if conditions allow use, false if not
    {
        switch (condition)
        {
            case TCondition.None: 
                return true; 
            case TCondition.HP100:
                if (Owner.CurrentHP == Owner.MaxHP) return true;
                else return false; 
            case TCondition.HPless75:
                if (Owner.CurrentHP / Owner.MaxHP <= 0.75f) return true;
                else return false;
            case TCondition.HPless50:
                if (Owner.CurrentHP / Owner.MaxHP <= 0.50f) return true;
                else return false;
            case TCondition.HPless25:
                if (Owner.CurrentHP / Owner.MaxHP <= 0.25f) return true;
                else return false;
            case TCondition.HPmore25:
                if (Owner.CurrentHP / Owner.MaxHP >= 0.25f) return true;
                else return false;
            case TCondition.HPmore50:
                if (Owner.CurrentHP / Owner.MaxHP >= 0.50f) return true;
                else return false;
            case TCondition.HPmore75:
                if (Owner.CurrentHP / Owner.MaxHP >= 0.75f) return true;
                else return false;
        }
        return false;
    
    }

}

public enum TCondition
{
    //Self Conditions
    None, HP100, HPless75, HPless50, HPless25, HPmore25, HPmore50, HPmore75,
    Flanked, OutMelee, InMelee, 

    //Target preference conditions
    HighestMaxHp, LowestMaxHP, HighestCurrentHP, LowestCurrentHP,
    Closest, BackRow, FrontRow, MidRow,
    
    //Target requirement conditions
    Hit2, Hit3, Hit4, RangeNoMove, 
    
    
}

