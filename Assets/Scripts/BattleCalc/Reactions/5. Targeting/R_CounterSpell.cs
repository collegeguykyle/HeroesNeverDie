


public class R_CounterSpell : ReactionTargetting
{
    public R_CounterSpell(Battle battle, IOccupyBattleSpace owner) : base(battle, owner)
    {
    }

    public override void onEvent(object sender, ResultTargetting result)
    {
        throw new System.NotImplementedException();
    }
}