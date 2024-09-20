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

    [JsonIgnore] public Ability Ability;

    [JsonIgnore] public List<TargetEval> EvalsList = new List<TargetEval>();

    [JsonIgnore] public IOccupyBattleSpace SelectedTarget { get; private set; }
    [JsonIgnore] public TargetEval SelectedTargetEval { get; private set; }
    [JsonIgnore] Dictionary<TargetEval, TargetData> dataPairs = new Dictionary<TargetEval, TargetData>();


    public ResultTargetting(Ability ability, TargetEval targetSelected, Dictionary<TargetEval, TargetData> dataPairs)
    {
        Ability = ability;
        Caster = Ability.OwningUnit;
        CasterName = Caster.Name;
        this.dataPairs = dataPairs;
        CasterBattleSpace = dataPairs[targetSelected].BattleSpace;
        foreach (TargetEval key in dataPairs.Keys) EvalsList.Add(key);
        SelectedTargetEval = targetSelected;
        SelectedTarget = targetSelected.target;

    }

    public TargetData getTargetData()
    {
        return dataPairs[SelectedTargetEval];
    }
    
}





