using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee1 : Ability
{


    public override string Name { get; protected set; } = "Test Melee 1";
    public override Mana cost { get; protected set; } = new Mana();
    public override Unit OwningUnit { get; protected set; }
    public override int Range { get; protected set; } = 1;
    public override bool UseEngaged { get; protected set; } = true;
    public override Team targets { get; protected set; } = Team.enemy;
    public override List<AttackType> GetAttackTypes { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }
    public override int GetBonus { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }

    public Melee1(Unit unit) : base(unit)
    {
        cost.AddManaType(ManaType.sword);
    }

    public override void ExecuteAbility(IOccupyBattleSpace target)
    {
        //this is where we would put in our logic of all the possible upgrades for this ability
    }

}
