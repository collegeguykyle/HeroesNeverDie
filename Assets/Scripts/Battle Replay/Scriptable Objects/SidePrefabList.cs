using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/SidePrefabList")]
public class SidePrefabList : ScriptableObject
{
    [SerializeField] public List<GameObject> list = new List<GameObject>();
}

//SingletonScriptableObject<SidePrefabList>