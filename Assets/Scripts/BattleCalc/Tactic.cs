using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;


public class Tactic : ToReport
{
    public bool TacticSelected = false;
    [JsonIgnore] private Unit Owner;
    public Ability Ability { get; private set; }
    public TCondition Condition1 { get; private set; }
    public TCondition Condition2 { get; private set; }
    public BasicConditions ConditionsCheck { get; private set; }
    public ResultTargetting resultTargetting { get; set; }

    public Tactic(Unit owner, Ability ability, TCondition condition1, TCondition condition2)
    {
        Owner = owner;
        Ability = ability;
        Condition1 = condition1;
        Condition2 = condition2;
    }

    public ResultTargetting TestTactic(Battle battleController, BattleSpacesController map, Mana AvailableMana)
    {
        //6: Can unit target an enemy with it from where it is?
        resultTargetting = map.GetTargets(Ability); 
        if (!resultTargetting.TargetInRange()) return resultTargetting;

        //7: If a mandatory target condition, ensure options meets requirement
        foreach (TargetData target in resultTargetting.Targets) 
        {
            if (TestTargetRequirements(Condition1, target.targetType, map)) target.TacticRequirement = true; else target.TacticRequirement = false;
            if (TestTargetRequirements(Condition2, target.targetType, map)) target.TacticRequirement = true; else target.TacticRequirement = false;
        }
        if (resultTargetting.TargetInRange()) return resultTargetting;

        //8: Filter out options based on condition priorities
        resultTargetting = TestTargetPrefernce(Condition1, resultTargetting, battleController);
        resultTargetting = TestTargetPrefernce(Condition2, resultTargetting, battleController);

        //9.1: If one of the conditions is a selector, use it to select the final target
        resultTargetting = TestTargetSelector(Condition1, resultTargetting, map);        
        resultTargetting = TestTargetSelector(Condition2, resultTargetting, map);

        //9.2: If no final selector condition, choose closest, then randomly --OR-- ability has prefered Selector if player does not input one
        if (resultTargetting.SelectedTargetData == null) resultTargetting = TestTargetSelector(TCondition.Closest, resultTargetting, map);

        //10.1: If still no target selection then nothing to target, so return false, else broadcast target data to reactions
        if (resultTargetting.SelectedTargetData == null) return resultTargetting;

        //*****10: Execute the ability against the chosen target*****
        TacticSelected = true;
        return resultTargetting;

    }


    public bool BasicConditionsMet(Battle battleController,  Mana AvailableMana)
    {
        ConditionsCheck = new BasicConditions();
        if (Owner.CurrentHP <= 0) ConditionsCheck.UnitAlive = false;                 //Unit still alive?
        if (!Mana.TryCosts(Ability.cost, AvailableMana)) ConditionsCheck.EnoughMana = false;
        //if (!Ability.TestManaCost(AvailableMana)) ConditionsCheck.EnoughMana = false; //enough mana?

        if (!TestSelfCondition(Condition1)) ConditionsCheck.Condition1Exclusionary = false;
        if (!TestSelfCondition(Condition2)) ConditionsCheck.Condition2Exclusionary = false;        //any exlusionary conditions?

        bool engaged = battleController.TestEngaged(Owner);
        if (engaged && !Ability.UseEngaged) ConditionsCheck.EngagedLimited = false;        //if unit is engaged can this ability be used?

        if (Ability is IMoveSelf && Owner.CurrentMove < 1) ConditionsCheck.MovePoints = false; //if move ability and no movement points
        
        return ConditionsCheck.AllConditionsMet();
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
        if (target is Unit && (target as Unit).CurrentHP <= 0) return false;
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

public class BasicConditions
{
    public bool UnitAlive = true;
    public bool EnoughMana = true;
    public bool Condition1Exclusionary = true;
    public bool Condition2Exclusionary = true;
    public bool EngagedLimited = true;
    public bool MovePoints = true;

    public bool AllConditionsMet()
    {
        if(UnitAlive && EnoughMana && Condition1Exclusionary && Condition2Exclusionary && EngagedLimited && MovePoints) return true;
        else return false;
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

