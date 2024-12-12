using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateDisplay : MonoBehaviour
{
    public GameObject counter;
    List<TMP_Text> counters = new();
    private void LateUpdate()
    {
        int i = 0;
        foreach (var item in PlayerInfo.instance.resourceAmounts)
        {
            if (counters.Count <= i)
            {
                GameObject obj = Instantiate(counter, transform);
                counters.Add(obj.GetComponentInChildren<TMP_Text>());
            }
            counters[i].text = item.Key.Name + ": " + item.Value;
            i++;
        }
    }
}
