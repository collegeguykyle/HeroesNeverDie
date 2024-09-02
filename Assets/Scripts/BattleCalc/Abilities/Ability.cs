using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability
{
    public abstract string Name { get; protected set; }
    public abstract Mana cost { get; protected set; }
    public abstract Unit OwningUnit { get; protected set; }
    public abstract int Range { get; protected set; } // range 0 = movement ability
    public abstract int AOESize { get; protected set; }
    public abstract bool UseEngaged { get; protected set; }
    public abstract Team targets { get; protected set; }
    public abstract List<AttackType> GetAttackTypes { get; protected set; }
    public abstract int GetBonus { get; protected set; }

    //TODO: How send Attack Result and Ability Complete result??
    //TODO: Ensure that send Unit Death is embedded in Attack Result

    public Ability(Unit OwningUnit)
    {
        this.OwningUnit = OwningUnit;
    }

    public bool TestManaCost(Mana ManaPool)
    {
        return Mana.TryCost(ManaPool, cost);

    }

    public abstract void ExecuteAbility(ResultTargetting TargettingData);


    private ResultAbility ExampleExecuteAbility(ResultTargetting TargettingData) //Example method
    {
        ResultAbility abilityResult = new ResultAbility(OwningUnit, this);
        
        // For now, extra attacks will go against original target even if it dies from
        // first attack, extra attacks will be wasted unless other targets in AOE.
        List<Unit> targetList = new List<Unit>();
        targetList.Add(TargettingData.GetUnitTarget());
        if (this is IAOE)
        {
            //***TODO: get the other targets based on AOE data
        }

        for (int i = 0; i < (this as IHit).NumberOfAttacks; i++) 
        {
            ResultAttack attackResult = new ResultAttack();

            foreach (Unit target in targetList)
            {
                ResultHit resultHit = ResultHit.TryHit(this, TargettingData.GetUnitTarget());
                
                ResultDamage damageResult = new ResultDamage();
                if (this is IDealDamage) damageResult = ResultDamage.RollDamage(resultHit);
                
                ResultSave saveResult = new ResultSave();
                if (this is IApplyStatus) saveResult = new ResultSave(); //***TODO: update after impliment this container logic

                ResultTargetAttack targetAttack = new ResultTargetAttack(resultHit, damageResult, saveResult);
                attackResult.Targets.Add(targetAttack);
            }
            OwningUnit.Battle.Reactions.SendAttackResult(attackResult);
            abilityResult.AttackList.Add(attackResult);
        }
        OwningUnit.Battle.Reactions.SendAbilityComplete(abilityResult);
        return abilityResult;
    }

    public static List<Unit> GetAOETargets(ResultTargetting TargetingData, IAOE ability)
    {
        List<Unit> result = new List<Unit>();
        return result;
    }

}

public enum AttackType { Ranged, Melee, Physical, Smashing, Slashing, Stabbing, 
    Elemental, Fire, Ice, Lightning, Nature, Magic, Arcane, Holy, Shadow, Force }

public enum DefenseType { Dodge, Block, Parry, Aura }

public enum AbilityType { Attack, DeBuff, Move, Buff }



