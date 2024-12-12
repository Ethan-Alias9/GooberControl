using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeleteOverlap : MonoBehaviour
{
    [ContextMenu("Rename")]
    public void RenameChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            t.gameObject.name = t.gameObject.name.Split(' ')[0];
        }

    }
    [ContextMenu("Delete Unnecessary")]
    public void DeleteUnnecessary()
    {
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        List<GameObject> toDelete = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            t.position = new Vector3(System.MathF.Round(t.position.x, System.MidpointRounding.AwayFromZero), t.position.y, System.MathF.Round(t.position.z, System.MidpointRounding.AwayFromZero));

            if (min.x > t.position.x)
                min.x = t.position.x;
            if (min.y > t.position.y)
                min.y = t.position.y;
            if (min.z > t.position.z)
                min.z = t.position.z;
            if (max.x < t.position.x)
                max.x = t.position.x;
            if (max.y < t.position.y)
                max.y = t.position.y;
            if (max.z < t.position.z)
                max.z = t.position.z;

            Vector3[] directions = {
                Vector3.up,
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
            };
            bool[] checks = new bool[directions.Length];
            for (int ii = 0; ii < directions.Length; ii++)
            {
                List<RaycastHit> hits = new (Physics.RaycastAll(t.position, directions[ii], 0.6f));

                for (int iii = 0; iii < hits.Count; iii++)
                {
                    if (hits[iii].transform.root != transform.root || (ii != 0 && hits[iii].transform.gameObject.name.Contains("amp")))
                        hits.RemoveAt(iii);
                }

                if (hits.Count != 0)
                    continue;

                hits = new(Physics.RaycastAll(t.position + directions[ii], Vector3.up, 500));
                checks[ii] = true;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.root != transform.root)
                        continue;
                    checks[ii] = false;
                    break;
                }
            }

            for (int ii = 0; ii < directions.Length; ii++)
            {
                if (checks[ii])
                    break;

                if (ii == directions.Length - 1)
                    toDelete.Add(t.gameObject);
            }
        }

        foreach (var go in toDelete)
        {
            DestroyImmediate(go);
        }

        for (int x = (int)min.x - 1; x < (int)max.x + 1; x++)
        {
            for (int y = (int)min.y - 1; y < (int)max.y + 1; y++)
            {
                for (int z = (int)min.z - 1; z < (int)max.z + 1; z++)
                {
                    Collider[] hitColliders = Physics.OverlapBox(new Vector3(x, y - 0.5f, z), Vector3.one * 0.25f);
                    for (int iv = 1; iv < hitColliders.Length; iv++)
                    {
                        DestroyImmediate(hitColliders[iv].gameObject);
                    }
                }
            }
        }
    }
}
