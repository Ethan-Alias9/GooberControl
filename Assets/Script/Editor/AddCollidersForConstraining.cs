using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(OmniscientController))]
public class AddCollidersForConstraining : MonoBehaviour
{
    public GameObject[] constraintParents;
    OmniscientController user;

    private void OnValidate()
    {
        user = GetComponent<OmniscientController>();
    }


    [ContextMenu("Add Constraints")]
    void Constrain()
    {
        for (int i = 0; i < constraintParents.Length; i++)
        {
            ProcessObj(constraintParents[i]);
        }
        for (int i = 0; i < user.ConstrainedColliders.Count; i++)
        {
            Collider a = user.ConstrainedColliders[i];
            for (int ii = i + 1; ii < user.ConstrainedColliders.Count; ii++)
            {
                Collider b = user.ConstrainedColliders[ii];
                if (a == b)
                {
                    user.ConstrainedColliders.RemoveAt(ii);
                    ii--;
                }
            }
        }

        EditorUtility.SetDirty(user);

        void ProcessObj(GameObject obj)
        {
            user.ConstrainedColliders.AddRange(obj.GetComponents<Collider>());
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                ProcessObj(obj.transform.GetChild(i).gameObject);
            }
        }
    }
}
