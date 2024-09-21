using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//purpose of this class is to report to all listening status who was targeted by an ability
//  and from who, from where, with what.
public class ResultTargetting : ToReport 
{
    [JsonIgnore] public IOccupyBattleSpace Caster;
    public string CasterName;
    public BattleSpace CasterBattleSpace;
    public int CurrentMove;
    public string ManaReport;
    
    [JsonIgnore] public Ability Ability;
    public string AbilityName;

    public BasicConditions conditions;

    [JsonIgnore] public IOccupyBattleSpace SelectedTarget;
    public string selectedTarget;
    [JsonIgnore] public TargetEval SelectedTargetEval = null;

    public List<TargetEval> EvalsList = new List<TargetEval>();
    [JsonIgnore] Dictionary<TargetEval, TargetData> dataPairs = new Dictionary<TargetEval, TargetData>();

    public bool AnyOptions;
    public bool AnyInRange;
    public bool AnyRequirements;
    public bool TacticSelected;

    public ResultTargetting(Ability ability, Dictionary<TargetEval, TargetData> dataPairs)
    {
        Ability = ability;
        AbilityName = ability.Name;
        Caster = Ability.OwningUnit;
        CasterName = Caster.Name;
        if (Caster is Unit) CurrentMove = (Caster as Unit).CurrentMove;
        if (Caster is Unit) ManaReport = (Caster as Unit).Mana.ManaAsString();
        this.dataPairs = dataPairs;
        foreach (TargetEval key in dataPairs.Keys) EvalsList.Add(key);
    }
    public ResultTargetting(Ability ability, BasicConditions conditions)
    {
        Ability = ability;
        AbilityName= ability.Name;
        Caster = ability.OwningUnit;
        CasterName = Caster.Name;
        if (Caster is Unit) CurrentMove = (Caster as Unit).CurrentMove;
        if (Caster is Unit) ManaReport = (Caster as Unit).Mana.ManaAsString();
        this.conditions = conditions;
    }

    public TargetData getTargetData()
    {
        if (dataPairs.Keys.Count == 0 || selectedTarget == null) return null;
        return dataPairs[SelectedTargetEval];
    }

    public void SelectTarget(TargetEval target)
    {
        SelectedTarget = target.target;
        selectedTarget = target.target.Name;
        SelectedTargetEval = target;
    }
    
}





