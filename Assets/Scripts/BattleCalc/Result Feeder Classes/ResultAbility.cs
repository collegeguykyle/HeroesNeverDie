using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultAbility : EventArgs
{
    public Unit Caster;
    public Ability Ability;
    public List<ResultAttack> AttackList = new List<ResultAttack>();

    public ResultAbility(Unit Caster, Ability Ability)
    {
        this.Caster = Caster;
        this.Ability = Ability;
    }
    public ResultAbility(Unit Caster, Ability Ability, List<ResultAttack> attacks)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.AttackList = attacks;
    }
    public ResultAbility(Unit Caster, Ability Ability, ResultAttack attack)
    {
        this.Caster = Caster;
        this.Ability = Ability;
        this.AttackList.Add(attack);
    }

    public void AddAttack(ResultAttack attack)
    {
        AttackList.Add(attack);
    }
}