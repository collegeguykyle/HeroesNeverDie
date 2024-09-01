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
        //3-5: Are basic requirements to use the tactic met? Unit alive, enough mana, etc
        if (!BasicConditionsMet(battleController, AvailableMana)) return false; 

        //6: Can unit target an enemy with it from where it is?
        ResultTargetting TargettingData = map.GetTargets(Ability); 
        if (!TargettingData.TargetInRange()) return false;

        //7: If a mandatory target condition, ensure options meets requirement
        foreach (TargetData target in TargettingData.Targets) 
        {
            if (TestTargetRequirements(Condition1, target.targetType, map)) target.TacticRequirement = true; else target.TacticRequirement = false;
            if (TestTargetRequirements(Condition2, target.targetType, map)) target.TacticRequirement = true; else target.TacticRequirement = false;
        }

        //8: Filter out options based on condition priorities
        TargettingData = TestTargetPrefernce(Condition1, TargettingData, battleController);
        TargettingData = TestTargetPrefernce(Condition2, TargettingData, battleController);

        //9.1: If one of the conditions is a selector, use it to select the final target
        TargettingData = TestTargetSelector(Condition1, TargettingData, map);        
        TargettingData = TestTargetSelector(Condition2, TargettingData, map);

        //9.2: If no final selector condition, choose closest, then randomly --OR-- ability has prefered Selector if player does not input one
        if (TargettingData.SelectedTarget == null) TargettingData = TestTargetSelector(TCondition.Closest, TargettingData, map);

        //*****10: Execute the ability against the chosen target*****
        Ability.ExecuteAbility(TargettingData);
        return true;
    }


    private bool BasicConditionsMet(Battle battleController,  Mana AvailableMana)
    {
        if(Owner.CurrentHP <= 0) return false;                  //Unit still alive?
        if (!Ability.TestManaCost(AvailableMana)) return false; //enough mana?

        if (!TestSelfCondition(Condition1)) return false;
        if (!TestSelfCondition(Condition2)) return false;        //any exlusionary conditions?

        bool engaged = battleController.TestEngaged(Owner);
        if (engaged && !Ability.UseEngaged) return false;        //if unit is engaged can this ability be used?

        if (Ability.Range == 0 && Owner.CurrentMove < 1) return false; //if move ability and no movement points

        return true;
    }

    private Team TargetConversion(Unit unit, Team target)
    {
        if (unit.Team == Team.player) return target;
        if (unit.Team == Team.enemy)
        {
            if (target == Team.enemy) return Team.player;
            if (target == Team.player) return Team.enemy;
        }
        return target;
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
        return true;
    
    }

    private bool TestTargetRequirements(TCondition condition, IOccupyBattleSpace target, BattleSpacesController map) //return true if conditions allow use, false if not
    {
        BattleSpace center;
        switch (condition)
        {
            case TCondition.None:
                return true;
            case TCondition.Hit2:
                center = map.GetSpaceOf(target);
                if (map.GetTargetsInRange(center.row, center.col, Ability.Range, target.Team, false).Count >= 2) return true;
                else return false;
            case TCondition.Hit3:
                center = map.GetSpaceOf(target);
                if (map.GetTargetsInRange(center.row, center.col, Ability.Range, target.Team, false).Count >= 3) return true;
                else return false;
            case TCondition.Hit4:
                center = map.GetSpaceOf(target);
                if (map.GetTargetsInRange(center.row, center.col, Ability.Range, target.Team, false).Count >= 4) return true;
                else return false;
        }
        return true;
    }

    private ResultTargetting TestTargetPrefernce(TCondition condition, ResultTargetting targets, Battle BC)
    {
        switch (condition)
        {
            case TCondition.Engaged:
                foreach(TargetData target in targets.Targets)
                {
                    if (target.targetType is Unit)
                    {
                        if (BC.TestEngaged(target.targetType as Unit))
                        {
                            target.TacticPreference = true;
                            target.PriorityScore++;
                            if (!targets.PriorityList.Contains(target)) targets.PriorityList.Add(target);
                        }
                    }
                }
                break;
            case TCondition.NotEngaged:
                foreach (TargetData target in targets.Targets)
                {
                    if (target.targetType is Unit)
                    {
                        if (!BC.TestEngaged(target.targetType as Unit))
                        {
                            target.TacticPreference = true;
                            target.PriorityScore++;
                            if (!targets.PriorityList.Contains(target)) targets.PriorityList.Add(target);
                        }
                    }
                }
                break;
        }
        return targets;
    }

    private ResultTargetting TestTargetSelector(TCondition condition1, ResultTargetting targets, BattleSpacesController map) //player can only choose one selector as they can only ever return one choice from a list of options
    {
        List<TargetData> targetsList = new List<TargetData>();
        if (targets.PriorityList.Count > 0) targetsList = targets.PriorityList;
        else targetsList = targets.Targets;
        TargetData choice = new TargetData();
        switch (condition1)
        {
            case TCondition.HighestMaxHP:
                choice = null;
                int HighHP = 0;
                foreach (TargetData target in targets.Targets)
                {
                    int HP = 0;
                    if (target.targetType is Unit) HP = (target.targetType as Unit).MaxHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                targets.SetSelectedTarget(choice);
                break;
            case TCondition.LowestMaxHP:
                choice = null;
                int LowHP = 0;
                foreach (TargetData target in targets.Targets)
                {
                    int HP = 9999;
                    if (target.targetType is Unit) HP = (target.targetType as Unit).MaxHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                targets.SetSelectedTarget(choice);
                break;
            case TCondition.HighestCurrentHP: 
                choice = null;
                HighHP = 0;
                foreach (TargetData target in targets.Targets)
                {
                    int HP = 0;
                    if (target.targetType is Unit) HP = (target.targetType as Unit).CurrentHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                targets.SetSelectedTarget(choice);
                break;
            case TCondition.LowestCurrentHP: 
                choice = null;
                LowHP = 9999;
                foreach (TargetData target in targets.Targets)
                {
                    int HP = 9999;
                    if (target.targetType is Unit) HP = (target.targetType as Unit).CurrentHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                targets.SetSelectedTarget(choice);
                break;
            case TCondition.Closest:
                choice = null;
                int lowDist = 999;
                foreach (TargetData target in targets.Targets)
                {
                    if (target.rangeTo < lowDist)
                    {
                        lowDist = target.rangeTo;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                targets.SetSelectedTarget(choice);
                break;
        }
        return targets;
    }
}

public enum TCondition
{
    //Self Conditions
    None, HP100, HPless75, HPless50, HPless25, HPmore25, HPmore50, HPmore75,
    Flanked, OutMelee, InMelee, 

    //Target requirement conditions: one or more potential targets MUST meet the condition or move to next tactic.
    //these then ALSO need to filter the potential targets as a preference
    Hit2, Hit3, Hit4,

    //Target preference conditions
    Engaged, NotEngaged, //tank, mage, starting row, current row, has or does not have X condition, etc

    //Target selection conditions
    HighestMaxHP, LowestMaxHP, HighestCurrentHP, LowestCurrentHP,
    Closest, 


}

