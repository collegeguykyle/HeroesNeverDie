using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultTargetting : ToReport
{
    [JsonIgnore] public BattleSpacesController BattleSpacesController;
    [JsonIgnore] public IOccupyBattleSpace Caster;
    public BattleSpace CasterBattleSpace;
    [JsonIgnore] public Ability Ability;
    public List<TargetData> Targets = new List<TargetData>();
    public List<TargetData> PriorityList = new List<TargetData>();
    public TargetData SelectedTargetData {  get; private set; }
    [JsonIgnore] public IOccupyBattleSpace SelectedTarget { get; private set; }

    public ResultTargetting( Ability ability, BattleSpace space, BattleSpacesController battleSpacesController)
    {
        Ability = ability;
        Caster = Ability.OwningUnit;
        CasterBattleSpace = space;
        BattleSpacesController = battleSpacesController;    
    }

    public void AddTargetData(TargetData targetData)
    {
        Targets.Add(targetData);
    }

    public bool TargetInRange() //return true if at least one target is within range
    {
        foreach (TargetData data in Targets)
        {
            if (data.inRange) return true;
        }
        return false;
    }

    public bool TargetWithRequirements() //return true if at least one target within range meets targeting requirements
    {
        foreach (TargetData data in Targets)
        {
            if (data.TacticRequirement && data.inRange) return true;
        }
        return false;
    }

    public void SetSelectedTarget(TargetData target)
    {
        if (SelectedTargetData == null && target != null)
        {
            SelectedTargetData = target;
            target.TacticSelected = true;
            SelectedTarget = target.targetType;
        }
    }
    
    public Unit GetUnitTarget()  // I need to add logic to the tactic decision making to allow for targeting only units or terrain and update this work flow
    {
        return SelectedTargetData.targetType as Unit;
    }
    
}

public class TargetData
{
    public IOccupyBattleSpace targetType;
    public string targetName;
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
        this.targetName = target.Name;
        this.rangeTo = rangeTo;
        this.pathDist = pathDist;
        OthersAOE = othersAOE;
    }
    public TargetData() { }
}

