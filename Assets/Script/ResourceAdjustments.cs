using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceAdjustments : MonoBehaviour
{
    [ContextMenu("Adjust")]
    void Adjust()
    {
        List<string> names = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);

            t.position = new Vector3(System.MathF.Round(t.position.x, System.MidpointRounding.AwayFromZero),
                System.MathF.Round(t.position.y, System.MidpointRounding.AwayFromZero),
                System.MathF.Round(t.position.z, System.MidpointRounding.AwayFromZero));
            if (t.gameObject.name.Contains("hunk"))
                t.position += Vector3.up * 0.33f;

            t.gameObject.name = t.gameObject.name.Split(' ')[0];
            if (!names.Contains(t.gameObject.name))
                names.Add(t.gameObject.name);
        }
        names.Sort();
        int[] nameCount = new int[names.Count];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            nameCount[names.IndexOf(t.gameObject.name)]++;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            int pos = 0;
            for (int ii = 0; ii < names.IndexOf(t.gameObject.name); ii++)
            {
                pos += nameCount[ii];
            }
            t.SetSiblingIndex(pos);
        }

    }
}
