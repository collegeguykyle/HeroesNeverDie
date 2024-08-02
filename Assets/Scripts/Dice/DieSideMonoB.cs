using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieSideMonoB : MonoBehaviour
{
    public DieSide DieSide { get; private set; }

    public string Name  = "Blank";
    public List<ManaType> ManaList = new List<ManaType>();
    public Mana Mana  = new Mana(); //Amount of mana you get if the side is rolled

    //Dice Status Effects
    [SerializeField] bool pain = false; //When rolled take damage and remove this effect !!NOT IMPLIMENTED
    [SerializeField] bool agony = false; //When rolled take damage but effect is not removed !!NOT IMPLIMENTED
    [SerializeField] bool phased = false; //When rolled side does nothing but this effect is removed !!NOT IMPLIMENTED

    private void Start()
    {
        foreach (ManaType type in ManaList)
        {
            this.Mana.AddManaType(type, 1);
        }
        DieSide = new DieSide(Name, Mana);
    }
}
