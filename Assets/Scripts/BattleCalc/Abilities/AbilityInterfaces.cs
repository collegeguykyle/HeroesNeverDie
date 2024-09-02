//Ability interfaces for IHit, ITestSave, IDealDamage, IApplyStatus,  IMove

public interface IHit
{
    public int NumberOfAttacks { get; }
}

public interface ITestSave
{

}

public interface IDealDamage
{
    public Damage damage { get;  }

    public abstract ResultDamage SendDamage(Damage damage);


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