using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Reaction 
{
    public IOccupyBattleSpace Owner { get; set; }
    protected bool subscribed = false;
    public abstract void Subscribe(Battle battle);
    public abstract void Unsubscribe();
}

public abstract class ReactionStartBattle : Reaction
{
    public ReactionStartBattle(IOccupyBattleSpace owner)
    {
        Owner = owner;
    }
    public abstract void onEvent();
}

public abstract class ReactionStartTurn : Reaction
{
    private Battle Battle;
    public ReactionStartTurn(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onStartOfTurn += onEvent;
    }

    public abstract void onEvent(object sender, Unit unit);

    public override void Unsubscribe()
    {
        Battle.Reactions.onStartOfTurn -= onEvent;
    }

    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onStartOfTurn += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionDiceRoll : Reaction
{
    private Battle Battle;
    public ReactionDiceRoll(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onDiceRoll += onEvent;
    }

    public abstract void onEvent(object sender, DieSide side);

    public override void Unsubscribe()
    {
        Battle.Reactions.onDiceRoll -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onDiceRoll += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionRollResult : Reaction
{
    private Battle Battle;
    public ReactionRollResult(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onRollResult += onEvent;
    }

    public abstract void onEvent(object sender, ResultRoll result);

    public override void Unsubscribe()
    {
        Battle.Reactions.onRollResult -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onRollResult += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionTargetting : Reaction
{
    private Battle Battle;
    public ReactionTargetting(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onTargeting += onEvent;
    }

    public abstract void onEvent(object sender, ResultTargetting result);

    public override void Unsubscribe()
    {
        Battle.Reactions.onTargeting -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onTargeting += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionAttackResult : Reaction
{
    private Battle Battle;
    public ReactionAttackResult(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onAttackResult += onEvent;
    }

    public abstract void onEvent(object sender, ResultAttack result);

    public override void Unsubscribe()
    {
        Battle.Reactions.onAttackResult -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onAttackResult += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionAbilityDone : Reaction
{
    private Battle Battle;
    public ReactionAbilityDone(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onAbilityComplete += onEvent;
    }

    public abstract void onEvent(object sender, ResultAbility result);

    public override void Unsubscribe()
    {
        Battle.Reactions.onAbilityComplete -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onAbilityComplete += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionEndTurn : Reaction
{
    private Battle Battle;
    public ReactionEndTurn(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onEndOfTurn += onEvent;
    }

    public abstract void onEvent(object sender, Unit unit);

    public override void Unsubscribe()
    {
        Battle.Reactions.onEndOfTurn -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onEndOfTurn += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionEndBattle : Reaction
{
    private Battle Battle;
    public ReactionEndBattle(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onEndOfBattle += onEvent;
    }

    public abstract void onEvent(object sender, EventArgs e);


    public override void Unsubscribe()
    {
        Battle.Reactions.onEndOfBattle -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onEndOfBattle += onEvent;
            subscribed = true;
        }
    }
}

public abstract class ReactionUnitDeath : Reaction
{
    private Battle Battle;
    public ReactionUnitDeath(Battle battle, IOccupyBattleSpace owner)
    {
        Owner = owner;
        Battle = battle;
        battle.Reactions.onUnitDeath += onEvent;
    }

    public abstract void onEvent(object sender, Unit unit);

    public override void Unsubscribe()
    {
        Battle.Reactions.onUnitDeath -= onEvent;
    }
    public override void Subscribe(Battle battle)
    {
        if (!subscribed)
        {
            Battle = battle;
            battle.Reactions.onUnitDeath += onEvent;
            subscribed = true;
        }
    }
}