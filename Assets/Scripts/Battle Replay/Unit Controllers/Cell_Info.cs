using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell_Info : MonoBehaviour
{
    public int row;
    public int col;
    RectTransform rect;
    public Vector3 center;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        center = getWorldPosition();
    }

    public Vector3 getWorldPosition()
    {

        return rect.transform.position; // THIS WILL NOT WORK if the gridlayout group is active
    }



}
