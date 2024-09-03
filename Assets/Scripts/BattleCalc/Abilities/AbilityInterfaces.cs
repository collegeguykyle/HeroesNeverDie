//Ability interfaces for IHit, ITestSave, IDealDamage, IApplyStatus,  IMove

using System.Collections.Generic;

public interface IHit
{
    public abstract List<AttackType> GetAttackTypes { get; }
    public abstract int NumberOfAttacks { get; }
    public abstract List<ToHitBonus> ToHitBonus { get; }
    public abstract int GetAttackBonus();
}

public interface ITestSave
{
    public List<MagicSkillBonus> MagicSkillBonus { get; }
    public int GetMagicBonus();
}

public interface IDealDamage
{
    public abstract List<Damage> damageDice { get;  }

    public abstract ResultDamage SendDamage(List<Damage> damage);


}

public interface IApplyStatus
{

}

public interface IAOE
{
    public int MaxTargets { get; }
    public int AOESize { get; }
    public int AOERange { get; }
    public AOEShape AOEShape { get; }
}
public enum AOEShape { Circle, Line, Cone}

public interface IMove
{

}