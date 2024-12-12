using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridSnap : MonoBehaviour
{
    public Vector3 snapPoint;

    void LateUpdate()
    {
        SetPosition(transform.position);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = LevelGrid.CellPosition(LevelGrid.Cell(position)) + snapPoint;
    }
}
