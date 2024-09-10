using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultAbility : EventArgs
{
    public Unit Caster;
    public Ability Ability;
    public List<ResultSingleTarget> AttackList = new List<ResultSingleTarget>();

    public ResultAbility(Unit Caster, Ability Ability)
    {
        this.Caster = Caster;
        this.Ability = Ability;
    }
    public ResultAbility(Unit Caster, Ability Ability, List<ResultSingleTarget> attacks)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.AttackList = attacks;
    }
    public ResultAbility(Unit Caster, Ability Ability, ResultSingleTarget attack)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.AttackList.Add(attack);
    }

    public void AddAttack(ResultSingleTarget attack)
    {
        AttackList.Add(attack);
    }
}