using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurnOrderDisplay : MonoBehaviour
{
    //[ ] I should probably split this into two seperate classes, one for TurnOrderManager, and TurnOrderDisplay

    public static TurnOrderDisplay Singleton;

    public GameObject unitCardPrefab;
    public float cardSpacing = 65f;
    public Vector2 cardSize = new Vector2(57.67f, 83.39f);
    public float activeCardSize = 1.18f;

    private List<Unit> units = new List<Unit>();
    private List<GameObject> unitCards = new List<GameObject>();
    public UnitCard currentUnitTurn;

    private bool TurnComplete = false;

    private void OnEnable()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(this);
    }

    public void StartCombat(List<Unit> unitList)
    {
        //create initial set of player and enemy unit cards
        units = unitList;
        foreach (Unit unit in units)
        {
            CreateUnitCard(unit);
        }
        RepositionCards(0);
        StartCoroutine(EnterBattleSequence());
    }

    private IEnumerator EnterBattleSequence()
    {
        yield return new WaitForSeconds(0.5f);
        // Move cards onto the screen
        foreach (GameObject card in unitCards)
        {
            card.transform.localPosition = new Vector3(card.transform.localPosition.x, 300, 0); // Start off-screen
            //TO FIX: card.transform.DOLocalMoveY(-50, 1.0f); // Move card to on-screen position
            yield return new WaitForSeconds(0.05f); // Stagger the entry of each card
        }

        // Wait for all cards to move on-screen
        yield return new WaitForSeconds(1f);
    }

    public void EndUnitTurn(Unit unit)
    {
        if (unit == currentUnitTurn.unit)
        {
            TurnComplete = true;
            currentUnitTurn.ChangeCardColor(Color.grey);
        }
    }

    public bool StartNextUnitTurn()
    {
        if (TurnComplete == true)
        {
            TurnComplete = false;
            return true;
        }
        else return false;
    }

    private void CreateUnitCard(Unit unit)
    {
        GameObject card = Instantiate(unitCardPrefab, transform);

        // Setup the card with unit info here
        card.GetComponent<UnitCard>().Initialize(unit);

        unitCards.Add(card);

    }

    void RepositionCards(float speed)
    {
        float centerIndex = (unitCards.Count - 1) / 2f;
        for (int i = 0; i < unitCards.Count; i++)
        {
            float positionX = (i - centerIndex) * cardSpacing;
            //TO FIX: unitCards[i].transform.DOLocalMoveX(positionX, speed); // Animate card movement
        }
    }

    public void HighlightActiveUnitCard(GameObject activeCard)
    {
        Vector3 originalScale = new Vector3(1, 1, 1); // Assuming this is the original scale
        Vector3 enlargedScale = new Vector3(activeCardSize, activeCardSize, activeCardSize);

        foreach (GameObject card in unitCards)
        {
            if (card == activeCard)
            {
               //TO FIX: card.transform.DOScale(enlargedScale, 0.25f); // Animate increase in size
            }
            else
            {
               //TO FIX: card.transform.DOScale(originalScale, 0.25f); // Animate decrease in size
            }
        }
    }

    public void AddUnitToBattle(Unit unit)
    {

    }


    // Call this method whenever the turn changes or the initiative values change
    public void UpdateTurnOrder()
    {


    }


    public void RemoveUnitCard(UnitCard card)
    {

    }
}
