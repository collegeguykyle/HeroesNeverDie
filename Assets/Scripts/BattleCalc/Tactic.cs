using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;


public class Tactic : ToReport
{
    
    [JsonIgnore] public Unit Owner { get; private set; }
    public Ability Ability { get; private set; }
    public TCondition Condition1 { get; private set; }
    public TCondition Condition2 { get; private set; }
    public BasicConditions ConditionsCheck { get; private set; }
    public List<TargetEval> targetEvals { get; private set; }
    public bool TacticSelected { get; private set; } = false;

    public Tactic(Unit owner, Ability ability, TCondition condition1, TCondition condition2)
    {
        Owner = owner;
        Ability = ability;
        Condition1 = condition1;
        Condition2 = condition2;
    }

    public void TacticReset()
    {
        TacticSelected = false;
        targetEvals = new List<TargetEval>();
        TacticSelected = false;
    }

    public ResultTargetting TestTactic(Battle battleController, List<TargetData> dataList, Mana AvailableMana, Engagements engages)
    {
        targetEvals = new List<TargetEval>();
        Dictionary<TargetEval, TargetData> dataPairs = new Dictionary<TargetEval, TargetData>();
        //Filter target list down to those this ability can target
        foreach (TargetData data in dataList)
        {
            if (data == null) continue;
            if (data.targetTeam == TargetConversion(Owner, Ability.targets)) 
            {
                TargetEval newEval = new TargetEval(data.target);
                targetEvals.Add(newEval);
                dataPairs.Add(newEval, data);
            }
        }

        //6: Can unit target an enemy with it from where it is?
        foreach (TargetEval eval in targetEvals) 
        {
            TargetData data = dataPairs[eval];
            if (data.rangeTo <= Ability.Range || Ability is IMoveSelf) eval.inRange = true;
            else eval.inRange = false;
        }
        if (targetEvals.Count == 0) return null;

        //7: If a mandatory target condition, ensure options meets requirement
        foreach (TargetEval eval in targetEvals) 
        {
            if (TestTargetRequirements(Condition1, eval.target, dataList)) eval.TacticRequirement = true; else eval.TacticRequirement = false;
            if (TestTargetRequirements(Condition2, eval.target, dataList)) eval.TacticRequirement = true; else eval.TacticRequirement = false;
        }
        if (targetEvals.Count == 0) return null;

        //8: Mark targets as priority based on conditional preferences
        TestTargetPrefernce(Condition1, targetEvals, engages);
        TestTargetPrefernce(Condition2, targetEvals, engages);

        //9.1: If one of the conditions is a selector, use it to select the final target
        TestTargetSelector(Condition1, targetEvals, dataPairs);        
        TestTargetSelector(Condition2, targetEvals, dataPairs);

        //9.2: Evaluate targets to see if one has highest priority
        int highP = 0;
        TargetEval choice = targetEvals[0];
        foreach (TargetEval eval in targetEvals)
        {
            if (eval.PriorityScore > highP && eval.inRange && eval.TacticRequirement)
            {
                choice = eval;
                highP = eval.PriorityScore;
            }
            choice.TacticSelected = true;
        }

        //9.2: If still a tie between two targets, choose closest, then randomly --OR-- ability has prefered Selector if player does not input one
        //TestTargetSelector(TCondition.Closest, evals, dataPairs);

        //*****10: Execute the ability against the chosen target*****
        TacticSelected = true;
        return new ResultTargetting(Ability, choice, dataPairs);

    }


    public bool BasicConditionsMet(Battle battleController,  Mana AvailableMana, Engagements engages)
    {
        ConditionsCheck = new BasicConditions();
        if (Owner.CurrentHP <= 0) ConditionsCheck.UnitAlive = false;                 //Unit still alive?
        if (!Mana.TryCosts(Ability.cost, AvailableMana)) ConditionsCheck.EnoughMana = false;
        //if (!Ability.TestManaCost(AvailableMana)) ConditionsCheck.EnoughMana = false; //enough mana?

        if (!TestSelfCondition(Condition1)) ConditionsCheck.Condition1Exclusionary = false;
        if (!TestSelfCondition(Condition2)) ConditionsCheck.Condition2Exclusionary = false;        //any exlusionary conditions?

        bool engaged = engages.TestEngaged(Owner);
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

    private bool TestTargetRequirements(TCondition condition, IOccupyBattleSpace target, List<TargetData> dataList) //return true if conditions allow use, false if not
    {
        if (target is Unit && (target as Unit).CurrentHP <= 0) return false; //target must be alive (may need to edit later if want to target corpses)
        //BattleSpace center;
        switch (condition)
        {
            case TCondition.None:
                return true;
                /* REFACTOR THE AOE CONDITIONS WITH AOE SHAPE TYPES, ETC
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
                */
        }
        
        return true;
    }

    private void TestTargetPrefernce(TCondition condition, List<TargetEval> targets, Engagements engages)
    {
        switch (condition)
        {
            case TCondition.Engaged:
                foreach(TargetEval target in targets)
                {
                    if (target.target is Unit)
                    {
                        if (engages.TestEngaged(target.target as Unit))
                        {
                            target.TacticPreference = true;
                            target.PriorityScore++;
                        }
                    }
                }
                break;
            case TCondition.NotEngaged:
                foreach (TargetEval target in targets)
                {
                    if (target.target is Unit)
                    {
                        if (!engages.TestEngaged(target.target as Unit))
                        {
                            target.TacticPreference = true;
                            target.PriorityScore++;
                        }
                    }
                }
                break;
        }
    }

    private void TestTargetSelector(TCondition condition1, List<TargetEval> targets, Dictionary<TargetEval, TargetData> dataDict) //player can only choose one selector as they can only ever return one choice from a list of options
    {
        if (targets.Count == 1)
        {
            targets[0].TacticSelected = true;
            return;
        }
        TargetEval choice;
        switch (condition1)
        {
            case TCondition.HighestMaxHP:
                int HighHP = 0;
                foreach (TargetEval target in targets)
                {
                    int HP = 0;
                    if (target.target is Unit) HP = (target.target as Unit).MaxHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                break;
            case TCondition.LowestMaxHP:
                int LowHP = 0;
                foreach (TargetEval target in targets)
                    {
                    int HP = 9999;
                    if (target.target is Unit) HP = (target.target as Unit).MaxHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                break;
            case TCondition.HighestCurrentHP: 
                HighHP = 0;
                foreach (TargetEval target in targets)
                {
                    int HP = 0;
                    if (target.target is Unit) HP = (target.target as Unit).CurrentHP;
                    if (HP > HighHP)
                    {
                        HighHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                break;
            case TCondition.LowestCurrentHP: 
                LowHP = 9999;
                foreach (TargetEval target in targets)
                {
                    int HP = 9999;
                    if (target.target is Unit) HP = (target.target as Unit).CurrentHP;
                    if (HP < LowHP)
                    {
                        LowHP = HP;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                break;
            case TCondition.Closest:
                int lowDist = 999;
                foreach (TargetEval target in targets)
                {
                    TargetData data = dataDict[target];
                    if (data.rangeTo < lowDist)
                    {
                        lowDist = data.rangeTo;
                        target.PriorityScore += 2;
                        target.TacticPriority = true;
                        choice = target;
                    }
                }
                break;
        }
    }
}

public class TargetEval
{
    [JsonIgnore] public IOccupyBattleSpace target;
    public string targetName;
    public bool inRange = false;
    public bool TacticRequirement = false;
    public bool TacticPreference = false;
    public bool TacticPriority = false;
    public int PriorityScore = 0;
    public bool TacticSelected = false;

    public TargetEval(IOccupyBattleSpace target)
    {
        this.target = target;
        this.targetName = target.Name;
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

