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
        if (!BasicConditionsMet(battleController, AvailableMana)) return false; //3-5: Can we use the tactic?
        
        List<IOccupyBattleSpace> targets = TargetsInRangeOfOwner(map); //6: Can unit target an enemy with it from where it is?
        if (targets.Count == 0) return false;  
        
        foreach (IOccupyBattleSpace target in targets) //7: If a mandatory target condition, ensure an options meets requirement
        {
            if (!TestTargetRequirements(Condition1, target, map)) return false;
            if (!TestTargetRequirements(Condition2, target, map)) return false;
        }

        //8: Filter out options based on condition priorities
        targets = TestTargetPrefernce(Condition1, targets, battleController);
        targets = TestTargetPrefernce(Condition2, targets, battleController);

        //9: Choose an option based on what options remain
        IOccupyBattleSpace FinalTarget = TargetSelector(Condition1, targets, map);        //returns null if condition is not a final selector
        if (FinalTarget == null) FinalTarget = TargetSelector(Condition2, targets, map);  //returns null if condition is not a final selector
        if (FinalTarget == null)                                                          //if no final selector condition, choose a random target from filtered options
        {
            int rand = Random.Range(0, targets.Count);
            FinalTarget = targets[rand];
        }
        Ability.ExecuteAbility(FinalTarget); //*****10: Execute the ability against the chosen target*****
        return true;
    }


    private bool BasicConditionsMet(Battle battleController,  Mana AvailableMana)
    {
        if (!Ability.TestManaCost(AvailableMana)) return false; //enough mana?

        if (!TestSelfCondition(Condition1)) return false;
        if (!TestSelfCondition(Condition2)) return false;        //any exlusionary conditions?

        bool engaged = battleController.TestEngaged(Owner);
        if (engaged && !Ability.UseEngaged) return false;        //if unit is engaged can this ability be used?

        if (Ability.Range == 0 && Owner.CurrentMove < 1) return false; //if move ability and no movement points

        return true;
    }

    private List<IOccupyBattleSpace> TargetsInRangeOfOwner(BattleSpacesController Map) //returns list of targets in range of ability
    {
        BattleSpace space = Map.GetSpaceOf(Owner);
        Team targetTeam = TargetConversion(Owner, Ability.targets); 
        List<IOccupyBattleSpace> targets = Map.GetTargetsInRange(space.row, space.col, Ability.Range, targetTeam, true);
        return targets;
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

    private List<IOccupyBattleSpace> TestTargetPrefernce(TCondition condition, List<IOccupyBattleSpace> targets, Battle BC)
    {
        if (targets.Count == 1) return targets;
        List<IOccupyBattleSpace> yesList = new List<IOccupyBattleSpace>();
        switch (condition)
        {
            case TCondition.Engaged:
                foreach(IOccupyBattleSpace target in targets)
                {
                    if (target is Unit)
                    {
                        if (BC.TestEngaged(target as Unit)) yesList.Add(target);
                    }
                }
                if (yesList.Count > 0) return yesList;
                else return targets;
            case TCondition.NotEngaged:
                foreach (IOccupyBattleSpace target in targets)
                {
                    if (target is Unit)
                    {
                        if (!BC.TestEngaged(target as Unit)) yesList.Add(target);
                    }
                }
                if (yesList.Count > 0) return yesList;
                else return targets;
        }
        return targets;
    }

    private IOccupyBattleSpace TargetSelector(TCondition condition1, List<IOccupyBattleSpace> targets, BattleSpacesController map) //returns which target to preference from the list based on the given TCondition
    {
        if (targets.Count == 1) return targets[0];
        switch (condition1)
        {
            case TCondition.HighestMaxHP: 
                IOccupyBattleSpace choice = null;
                int HighHP = 0;
                foreach (IOccupyBattleSpace target in targets)
                {
                    int HP = 0;
                    if (target is Unit) HP = (target as Unit).MaxHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        choice = target;
                    }
                }
                return choice;
            case TCondition.LowestMaxHP: 
                choice = null;
                int LowHP = 0;
                foreach (IOccupyBattleSpace target in targets)
                {
                    int HP = 9999;
                    if (target is Unit) HP = (target as Unit).MaxHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        choice = target;
                    }
                }
                return choice;
            case TCondition.HighestCurrentHP: 
                choice = null;
                HighHP = 0;
                foreach (IOccupyBattleSpace target in targets)
                {
                    int HP = 0;
                    if (target is Unit) HP = (target as Unit).CurrentHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        choice = target;
                    }
                }
                return choice;
            case TCondition.LowestCurrentHP: 
                choice = null;
                LowHP = 9999;
                foreach (IOccupyBattleSpace target in targets)
                {
                    int HP = 9999;
                    if (target is Unit) HP = (target as Unit).CurrentHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        choice = target;
                    }
                }
                return choice;
            case TCondition.Closest:
                choice = null;
                int lowDist = 999;
                foreach (IOccupyBattleSpace target in targets)
                {
                    BattleSpace targetSpace = map.GetSpaceOf(target);
                    BattleSpace ownerSpace = map.GetSpaceOf(Owner);
                    int dist = map.CalculateDistance(ownerSpace.row, ownerSpace.col, targetSpace.row, targetSpace.col);
                    if (dist < lowDist)
                    {
                        lowDist = dist;
                        choice = target;
                    }
                }
                return choice;
        }
        return null;
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

