using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    static LevelGrid instance;


    public Vector3 size = Vector3.one;
    public Vector3 offset = Vector3.zero;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
#if DEBUG
            Debug.LogWarning("Level grid created while instance exists.");
#endif
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }




    //Static Functions========================================
    public static Vector3 Size
    {
        get
        {
            CheckInstance();
            return instance.size;
        }
        set
        {
            CheckInstance();
            instance.size = value;
        }
    }
    public static Vector3Int Cell(Vector3 pos)
    {
        CheckInstance();
        return instance.GetCell(pos);
    }

    public static Bounds CellBounds(Vector3Int cellPos)
    {
        CheckInstance();
        return instance.GetCellBounds(cellPos);
    }
    public static Vector3 CellPosition(Vector3Int cellPos)
    {
        CheckInstance();
        return instance.GetCellPosition(cellPos);
    }


    //Member Functions=================================
    public Vector3Int GetCell(Vector3 pos)
    {
        return (pos - offset).DivideComponentWise(size).FloorToVector3Int();
    }
    public Bounds GetCellBounds(Vector3Int cellPos) => new(GetCellPosition(cellPos), size);
    public Vector3 GetCellPosition(Vector3Int cellPos) => Vector3.Scale(cellPos.ToVector3(), size) + offset + (size * 0.5f);


    /// <summary>
    /// A small function that checks if instance exists only if DEBUG is defined.
    /// </summary>
    /// <exception cref="System.Exception"> Thrown if LevelGrid.instance is null. </exception>
    static void CheckInstance()
    {
#if DEBUG
        if (instance == null)
            throw new System.Exception("There is no instance of Level Grid!");
#endif
    }
}
