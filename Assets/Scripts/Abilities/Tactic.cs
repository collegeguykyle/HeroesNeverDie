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

    public bool TestTactic(Battle battleController, BattleSpacesController map, Mana AvailableMana)
    {
        if (!ConditionsMet(battleController, AvailableMana)) return false; //Can we use the tactic?
        if (!TestTargets(map)) return false;  //Can we target an enemy from where we currently are?

        // [ ] Do any of the target options meet TConditions?
        return true;
    }

    //movement abilities will also want to know if a tactic can be used to determine if we should
    //consider it when setting move to location options
    private bool ConditionsMet(Battle battleController,  Mana AvailableMana)
    {
        if (!Ability.TestManaCost(AvailableMana)) return false; //enough mana?

        if (!TestSelfCondition(Condition1)) return false;
        if (!TestSelfCondition(Condition2)) return false;        //any exlusionary conditions?

        bool engaged = battleController.TestEngaged(Owner);
        if (engaged && !Ability.UseEngaged) return false;        //if unit is engaged can this ability be used?

        if (Ability.Range == 0 && Owner.CurrentMove < 1) return false; //if move ability and no movement points

        return true;
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

