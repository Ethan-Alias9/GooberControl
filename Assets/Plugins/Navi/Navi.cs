using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class Navi
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AddSceneCallbacks()
    {
        SceneManager.sceneUnloaded += (_) => RemoveArea(areaCode);
        SceneManager.sceneLoaded += (_, _) => LoadNavMesh();
    }


    public class Path
    {
        public Vector3[] points;
    }

    static int areaCode = 0;
    public static void LoadNavMesh()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        List<Vector3> vertices = new(triangulation.vertices);
        List<int> indices = new(triangulation.indices);
        for (int i = 0; i < vertices.Count; i++)
        {
            if (float.IsNaN(vertices[i].x))
                continue;

            for (int ii = i + 1; ii < vertices.Count; ii++)
            {
                if (float.IsNaN(vertices[ii].x))
                    continue;

                if (Vector3.Distance(vertices[i], vertices[ii]) > 0.01f)
                    continue;

                for (int iii = 0; iii < indices.Count; iii++)
                {
                    if (indices[iii] == ii)
                        indices[iii] = i;
                }
                vertices[ii] = new Vector3(float.NaN, float.NaN, float.NaN);
            }
        }

        for (int i = vertices.Count - 1; i >= 0; i--)
        {
            if (!float.IsNaN(vertices[i].x))
                continue;
            vertices.RemoveAt(i);
            for (int ii = 0; ii < indices.Count; ii++)
            {
                if (indices[ii] > i)
                    indices[ii] -= 1;
            }
        }

        LoadMesh(vertices.ToArray(), indices.ToArray());
    }
    public static void LoadMesh(Vector3[] nodes, int[] indices)
    {
        if (areaCode != 0)
            RemoveArea(areaCode);
        areaCode = CreateArea(nodes, nodes.Length, indices, indices.Length);
        Debug.Log("Navi loaded mesh!");
    }
    
    public static Path FindPath(Vector3 start, Vector3 end)
    {
        Debug.Log("FindPath called");
        return FindPath(areaCode, start, end);
    }

    public static Path FindPathToClosest(Vector3 start, Vector3[] targets)
    {
        Debug.Log("FindPathToClosest called");
        return FindPathToClosest(areaCode, start, targets);
    }



    // Area Management
    [DllImport("Navi", CallingConvention = CallingConvention.Cdecl)]
    private static extern int CreateArea(Vector3[] nodes, int nodeCount, int[] indicies, int indexCount);

    [DllImport("Navi", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RemoveArea(int areaID);


    // Pathfinding
    [DllImport("NaviWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe byte* FindAndManagePath(int areaID, Vector3 start, Vector3 end);

    [DllImport("NaviWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe byte* FindAndManagePathToClosest(int areaID, Vector3 start, Vector3[] targets, int targetCount);

    private static unsafe Path FindPath(int areaID, Vector3 start, Vector3 end)
    {
        byte* ptr = FindAndManagePath(areaID, start, end);
        Path path = ProcessPath(ptr);

        ReleaseManagedObject(ptr);
        
        return path;
    }
    private static unsafe Path FindPathToClosest(int areaID, Vector3 start, Vector3[] targets)
    {
        byte* ptr = FindAndManagePathToClosest(areaID, start, targets, targets.Length);
        Path path = ProcessPath(ptr);

        ReleaseManagedObject(ptr);

        return path;
    }
    static unsafe Path ProcessPath(byte* ptr)
    {
        int* countPtr = (int*)(ptr + 8);
        Vector3* vectorArrayPtr = *(Vector3**)(ptr + 16);

        Path path = new();

        int pointCount = *countPtr;

        path.points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
            path.points[i] = *(vectorArrayPtr + i);

        return path;
    }


    [DllImport("NaviWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void ReleaseManagedObject(void* ptr);
}