using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    private void Awake()
    {
        instance = this;
    }

    public Dictionary<Resource, int> resourceAmounts = new();


    public void Add(Resource resource, int amount)
    {
        if (!resourceAmounts.ContainsKey(resource))
            resourceAmounts.Add(resource, 0);
        resourceAmounts[resource] += amount;
    }
}
