using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultAbility : EventArgs
{
    public Unit Caster;
    public Ability Ability;
    public List<Action> ActionList = new List<Action>();



    #region constructors
    public ResultAbility(Unit Caster, Ability Ability)
    {
        this.Caster = Caster;
        this.Ability = Ability;
    }
    public ResultAbility(Unit Caster, Ability Ability, List<Action> actions)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.ActionList = actions;
    }
    public ResultAbility(Unit Caster, Ability Ability, Action action)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.ActionList.Add(action);
    }

    #endregion

    public void AddAction(Action attack)
    {
        ActionList.Add(attack);
    }
}