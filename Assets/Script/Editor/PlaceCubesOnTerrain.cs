using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlaceCubesOnTerrain : MonoBehaviour
{
    [SerializeField] Terrain terrain;
    [SerializeField] GameObject cubePrefab;

    [SerializeField] Transform parent;

    [SerializeField] Vector3 offset;

    public int width = 500;
    public int height = 500;

    public Vector3 terrainOffset;

    public Vector2Int xRange;
    public Vector2Int zRange;


    [ContextMenu("Process")]
    public void Process()
    {
        int res = terrain.terrainData.heightmapResolution;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, width, height);

        for (int x = xRange.x; x < width && (xRange.y == 0 || x < xRange.y); x++)
        {
            for (int z = zRange.x; z < width && (zRange.y == 0 || z < xRange.y); z++)
            {

                float y = Mathf.Round(terrain.SampleHeight(terrainOffset + new Vector3(x, 0, z))) + offset.y;


                float up = terrain.SampleHeight(terrainOffset + new Vector3(x, 0, z + 1));
                float left = terrain.SampleHeight(terrainOffset + new Vector3(x + 1, 0, z));
                float right = terrain.SampleHeight(terrainOffset + new Vector3(x - 1, 0, z));
                float down = terrain.SampleHeight(terrainOffset + new Vector3(x, 0, z - 1));

                float lowestHeight = up;
                if (left < lowestHeight)
                    lowestHeight = left;
                if (right < lowestHeight)
                    lowestHeight = right;
                if (down < lowestHeight)
                    lowestHeight = down;

                do
                {
                    GameObject createdObject = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab, parent);
                    createdObject.transform.position = terrainOffset + new Vector3(x + offset.x, y, z + offset.z);
                    y--;
                }
                while (y > lowestHeight);
            }
        }
    }
}
