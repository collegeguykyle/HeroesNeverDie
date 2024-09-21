using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurnOrder
{
    private List<Unit> turnOrder;
    private int currentIndex;
    private int currentRound;

    public TurnOrder(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        turnOrder = DetermineTurnOrder(playerTeam, enemyTeam);
        currentIndex = 0;
        currentRound = 1;
    }

    public Unit GetCurrentUnit()
    {
        if (turnOrder.Count == 0)
            throw new InvalidOperationException("No units in turn order.");

        return turnOrder[currentIndex];
    }

    public void AdvanceToNextUnit()
    {
        if (turnOrder.Count == 0)
            throw new InvalidOperationException("No units in turn order.");

        currentIndex++;

        if (currentIndex >= turnOrder.Count)
        {
            currentIndex = 0;
            currentRound++;
            // [ ] Log the start of a new round
        }
    }

    public void RemoveFromTurnOrder(Unit unit)
    {
        turnOrder.Remove(unit);
    }

    private List<Unit> DetermineTurnOrder(List<Unit> playerTeam, List<Unit> enemyTeam)
    {
        List<Unit> allUnits = playerTeam.Concat(enemyTeam).ToList();

        // Sort by Init in descending order
        allUnits.Sort((u1, u2) => u2.Init.CompareTo(u1.Init));

        List<Unit> turnOrder = new List<Unit>();
        int playerTurnsTaken = 0;
        int enemyTurnsTaken = 0;

        for (int i = 0; i < allUnits.Count;)
        {
            // Get all units with the current highest Init
            int currentInit = allUnits[i].Init;
            var tiedUnits = allUnits.Where(u => u.Init == currentInit).ToList();

            // Handle ties
            if (tiedUnits.Count > 1)
            {
                foreach (var unit in tiedUnits)
                {
                    if (unit.Team == Team.player)
                    {
                        playerTurnsTaken++;
                    }
                    else
                    {
                        enemyTurnsTaken++;
                    }
                }

                // Balance the turns based on team advantage
                while (tiedUnits.Count > 0)
                {
                    if (playerTurnsTaken > enemyTurnsTaken)
                    {
                        // Add the next enemy unit
                        var nextEnemy = tiedUnits.FirstOrDefault(u => u.Team == Team.enemy);
                        if (nextEnemy != null)
                        {
                            turnOrder.Add(nextEnemy);
                            tiedUnits.Remove(nextEnemy);
                            enemyTurnsTaken++;
                        }
                        else
                        {
                            turnOrder.AddRange(tiedUnits);
                            tiedUnits.Clear();
                        }
                    }
                    else if (enemyTurnsTaken > playerTurnsTaken)
                    {
                        // Add the next player unit
                        var nextPlayer = tiedUnits.FirstOrDefault(u => u.Team == Team.player);
                        if (nextPlayer != null)
                        {
                            turnOrder.Add(nextPlayer);
                            tiedUnits.Remove(nextPlayer);
                            playerTurnsTaken++;
                        }
                        else
                        {
                            turnOrder.AddRange(tiedUnits);
                            tiedUnits.Clear();
                        }
                    }
                    else
                    {
                        // Coin flip or random selection
                        Random rnd = new Random();
                        var randomUnit = tiedUnits[rnd.Next(tiedUnits.Count)];
                        turnOrder.Add(randomUnit);
                        tiedUnits.Remove(randomUnit);

                        if (randomUnit.Team == Team.player)
                        {
                            playerTurnsTaken++;
                        }
                        else
                        {
                            enemyTurnsTaken++;
                        }
                    }
                }

                // Move to the next Init value
                i += tiedUnits.Count;
            }
            else
            {
                // If there's no tie, just add the unit to the turn order
                turnOrder.Add(allUnits[i]);
                if (allUnits[i].Team == Team.player)
                {
                    playerTurnsTaken++;
                }
                else
                {
                    enemyTurnsTaken++;
                }
                i++;
            }
        }

        return turnOrder;
    }
}