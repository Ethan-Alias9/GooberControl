using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTrackerManager : MonoBehaviour
{
    public GameObject resourceTrackerPrefab;

    List<ResourceCounter> counters = new();
    private void Update()
    {
        if (counters.Count < PlayerInfo.instance.resourceAmounts.Count)
        {
            foreach (var item in PlayerInfo.instance.resourceAmounts)
            {
                bool found = false;
                for (int i = 0; i < counters.Count; i++)
                {
                    if (counters[i].trackedResource != item.Key)
                        continue;
                    found = true;
                    break;
                }
                if (found)
                    continue;
                counters.Add(Instantiate(resourceTrackerPrefab, transform).GetComponent<ResourceCounter>());
                counters[^1].trackedResource = item.Key;
            }
        }
    }
}
