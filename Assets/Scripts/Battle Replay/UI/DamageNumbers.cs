using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;


//Reference for how to create a damage number for the battle replay class:
//DamageNumbers.Create(transform.position, UnityEngine.Random.Range(1, 100));

public class DamageNumbers : MonoBehaviour
{
    public float moveDistance = 1f;
    public float duration = 1f;
    public Color damageColor;

    public static int sortingOrder;

    private TextMeshPro TMP;

}
