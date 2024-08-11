using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Team { player, enemy, neutral, terrain, other }
public class Unit : MonoBehaviour, IOccupyBattleSpace
{
    public string Name = "Hero Name";

    public int MaxHP = 10;
    public int CurrentHP = 10;

    public int Actions = 1;
    public int SwiftActions = 1;

    public bool PassTurn = false;

    public Mana Mana = new Mana();

    public List<Ability> Abilities = new List<Ability>();

    public int StartingRow = 0;
    public int StartingCol = 0;
    public int Init = 0;

    public Team Team { get; private set; }
}
