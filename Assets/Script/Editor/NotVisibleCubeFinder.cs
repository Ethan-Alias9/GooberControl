using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class NotVisibleCubeFinder : MonoBehaviour
{
    List<Collider> notVisible;

    private void Awake()
    {
        notVisible = new(transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            notVisible.Add(t.GetComponent<Collider>());
        }
    }

    void Update()
    {
        // Iterate backwards to minimise rewriting of list
        for (int i = notVisible.Count - 1; i >= 0; i--)
        {
            if (!IsVisible(notVisible[i]))
                continue;
            notVisible.RemoveAt(i);
        }
    }

    bool IsVisible(Collider box)
    {
        Camera cam = Camera.main;
        Vector3 camPosition = cam.transform.position;
        Vector3 camForward = cam.transform.forward;

        Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(cam);

        Bounds boxBounds = box.bounds;

        if (!GeometryUtility.TestPlanesAABB(cameraFrustum, boxBounds))
            return false;

        Vector3[] testPoints =
        {
            boxBounds.min,
            new (boxBounds.min.x, boxBounds.min.y, boxBounds.max.z),
            new (boxBounds.min.x, boxBounds.max.y, boxBounds.min.z),
            new (boxBounds.min.x, boxBounds.max.y, boxBounds.max.z),
            new (boxBounds.max.x, boxBounds.min.y, boxBounds.min.z),
            new (boxBounds.max.x, boxBounds.min.y, boxBounds.max.z),
            new (boxBounds.max.x, boxBounds.max.y, boxBounds.min.z),
            boxBounds.max,

            boxBounds.center,
            new (boxBounds.min.x, boxBounds.center.y, boxBounds.center.z),
            new (boxBounds.max.x, boxBounds.center.y, boxBounds.center.z),
            new (boxBounds.center.x, boxBounds.min.y, boxBounds.center.z),
            new (boxBounds.center.x, boxBounds.max.y, boxBounds.center.z),
            new (boxBounds.center.x, boxBounds.center.y, boxBounds.min.z),
            new (boxBounds.center.x, boxBounds.center.y, boxBounds.max.z),
        };

        foreach (Vector3 point in testPoints)
        {
            if (!Physics.Raycast(point, Vector3.Normalize(camPosition - point), Vector3.Magnitude(camPosition - point), LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
                return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        Write();
    }

    [ContextMenu("Write")]
    void Write()
    {
        List<string> lines = new(notVisible.Count);
        foreach (var c in notVisible)
        {
            lines.Add(c.gameObject.GetInstanceID().ToString());
        }
        File.WriteAllLines(Application.dataPath + "/NotVisibleBoxes.txt", lines);
    }

    [ContextMenu("Select Unseen")]
    void SelectUnseen()
    {
        string[] strings = File.ReadAllLines(Application.dataPath + "/NotVisibleBoxes.txt");

        int[] ids = new int[strings.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = int.Parse(strings[i]);
        }

        Object[] objects = new Object[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            objects[i] = EditorUtility.InstanceIDToObject(ids[i]);
        }


        Selection.objects = objects;
    }

    [ContextMenu("Sort By Material")]
    void SortByMaterial()
    {
        Dictionary<string, Transform> materialNameToParent = new();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            if (!t.TryGetComponent(out Renderer r))
                continue;

            string matName = r.sharedMaterial.name;

            if (!materialNameToParent.ContainsKey(matName))
            {
                materialNameToParent.Add(matName, new GameObject(matName).transform);
                materialNameToParent[matName].parent = transform;
            }

            t.parent = materialNameToParent[matName];
            i--;
        }
    }
}
