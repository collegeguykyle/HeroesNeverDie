using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public Dice dice;
    public List<DieSides> Sides = new List<DieSides>();
    public SidePrefabList SidePrefabList; //master list of side prefabs for this class to reference

    public List<GameObject> SidePrefabs = new List<GameObject>();
    private int currentSide = 0;
    public float RollTime = 2.0f;
    public float RollSpeed = .1f;

    public delegate void SideRolledHandler(DieSide side);
    public static event SideRolledHandler SideRolled;

    public void RollDie() 
    {
        if (dice != null)
        {
            Sides = dice.Sides;
            SpawnSides();
            StartCoroutine("RollDie");
        }
        SideRolled += test;
    }


    private void SpawnSides()
    {
        foreach (DieSides side in Sides)
        {
            GameObject spawn = Instantiate(SidePrefabList.list[(int)side], gameObject.transform);
            SidePrefabs.Add(spawn);
            spawn.SetActive(false);
        }
    }

    private int Roll()
    {
        int num = Mathf.FloorToInt(UnityEngine.Random.Range(0f, Sides.Count));
        return num;
    }

    private IEnumerator ShowDieRoll()
    {
        //Cycle through icons on the die sides to show rolling
        float time = 0.0f;
        while (time < RollTime)
        {
            SidePrefabs[currentSide].SetActive(false);
            currentSide = Roll();
            SidePrefabs[currentSide].SetActive(true);

            yield return new WaitForSeconds(RollSpeed);
            time += RollSpeed;
        }
        
        //Temporarily increase size of sprite to show final result
        Transform t = SidePrefabs[currentSide].transform;
        Vector3 tscale = t.localScale;
        t.localScale = new Vector3(t.localScale.x * 1.5f, t.localScale.y * 1.5f, 0);
        yield return new WaitForSeconds(0.8f);
        t.localScale = tscale;

        //Send event with the DieSide that was rolled
        DieSide side = SidePrefabs[currentSide].GetComponent<DieSide>();
        SideRolled?.Invoke(side);
    }


    public void test(DieSide side)
    {
        Debug.Log($"Side rolled:  {side.Name}");
    }

}
