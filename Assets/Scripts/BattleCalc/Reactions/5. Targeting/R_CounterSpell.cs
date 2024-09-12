


public class R_CounterSpell : ReactionTargetting
{
    public R_CounterSpell(Battle battle, IOccupyBattleSpace owner) : base(battle, owner)
    {
    }

    public override string Name => throw new System.NotImplementedException();

    public override void onEvent(object sender, ResultTargetting result)
    {
        throw new System.NotImplementedException();
    }
}