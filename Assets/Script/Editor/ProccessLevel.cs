using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

public class ProccessLevel : MonoBehaviour
{
    [ContextMenu("Process")]
    void Process()
    {
        Dictionary<ProBuilderMesh, List<Face>> meshRequiredFaces = new();

        for (int i = 0; i < transform.childCount; i++)
        {
            Recursive(transform.GetChild(i).gameObject);
        }
        foreach (var item in meshRequiredFaces)
        {
            var proBuilderMesh = item.Key;

            proBuilderMesh.faces = item.Value;
            proBuilderMesh.ToMesh();
            proBuilderMesh.Refresh();
        }

        void Recursive(GameObject obj)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            if (obj.TryGetComponent(out ProBuilderMesh proBuilderMesh))
                meshRequiredFaces.Add(proBuilderMesh, FindNecessaryFaces(proBuilderMesh));


            Transform t = obj.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Recursive(t.GetChild(i).gameObject);
            }
        }
    }

     List<Face> FindNecessaryFaces(ProBuilderMesh mesh)
    {
        bool[] keepFaces = new bool[mesh.faceCount];

        for (int i = 0; i < mesh.faceCount; i++)
        {
            Face face = mesh.faces[i];
            Vector3 normal = mesh.GetNormals()[face.indexes[0]];
            normal = transform.rotation * normal;

            if (normal.y < 0)
            {
                keepFaces[i] = false;
                continue;
            }

            List<RaycastHit> hits = new(Physics.RaycastAll(mesh.transform.position, normal, 0.6f));

            for (int ii = 0; ii < hits.Count; ii++)
            {
                if (hits[ii].transform.root != transform.root || hits[ii].transform.gameObject.name.Contains("amp"))
                    hits.RemoveAt(ii);
            }

            if (hits.Count != 0)
                continue;

            hits = new(Physics.RaycastAll(mesh.transform.position + normal, Vector3.up, 500));
            keepFaces[i] = true;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.root != transform.root)
                    continue;
                keepFaces[i] = false;
                break;
            }
        }

        List<Face> resultingFaces = new();

        for (int i = 0; i < keepFaces.Length; i++)
        {
            if (!keepFaces[i])
                continue;
            resultingFaces.Add(mesh.faces[i]);
        }

        return resultingFaces;
    }

}
