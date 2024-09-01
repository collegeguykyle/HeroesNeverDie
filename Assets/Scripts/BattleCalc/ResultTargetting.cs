using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultTargetting
{
    public IOccupyBattleSpace Caster;
    public BattleSpace CasterBattleSpace;
    public Ability Ability;
    public List<TargetData> Targets = new List<TargetData>();
    public List<TargetData> PriorityList = new List<TargetData>();
    public TargetData SelectedTarget {  get; private set; }

    public ResultTargetting(Ability ability, BattleSpace space)
    {
        Ability = ability;
        Caster = Ability.OwningUnit;
        CasterBattleSpace = space;
    }

    public void AddTargetData(TargetData targetData)
    {
        Targets.Add(targetData);
    }

    public bool TargetInRange()
    {
        foreach (TargetData data in Targets)
        {
            if (data.inRange) return true;
        }
        return false;
    }

    public void SetSelectedTarget(TargetData target)
    {
        if (SelectedTarget == null && target != null)
        {
            SelectedTarget = target;
            target.TacticSelected = true;
        }
    }
    
}

public class TargetData
{
    public IOccupyBattleSpace targetType;
    public BattleSpace BattleSpace;
    public int rangeTo;
    public int pathDist;
    public List<IOccupyBattleSpace> OthersAOE;
    public bool inRange = false;
    public bool TacticRequirement = false;
    public bool TacticPreference = false;
    public bool TacticPriority = false;
    public int PriorityScore = 0;
    public bool TacticSelected = false;

    public TargetData(IOccupyBattleSpace target, int rangeTo, int pathDist, List<IOccupyBattleSpace> othersAOE)
    {
        this.targetType = target;
        this.rangeTo = rangeTo;
        this.pathDist = pathDist;
        OthersAOE = othersAOE;
    }
    public TargetData() { }
}

