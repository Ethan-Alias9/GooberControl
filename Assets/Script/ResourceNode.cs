using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [field: SerializeField] public Resource Resource { get; private set; }

    [field: SerializeField] public float Cooldown { get; private set; }

    bool _available;
    public bool Available
    {
        get { return _available; }
        private set
        {
            if (_available == value)
                return;
            _available = value;

            foreach (var item in availableObjects)
                item.SetActive(_available);
            foreach (var item in unavailableObjects)
                item.SetActive(!_available);

            if (_available)
            {
                if (!Resource.harvestableNodes.ContainsKey(Resource))
                    Resource.harvestableNodes.Add(Resource, new());
                Resource.harvestableNodes[Resource].Add(this);
            }
            else
            {
                Resource.harvestableNodes[Resource].Remove(this);
            }
        }
    }

    [SerializeField] GameObject[] availableObjects;
    [SerializeField] GameObject[] unavailableObjects;


    public void Harvest()
    {
        Debug.Log("Node Harvested!");
        Available = false;
        StartCoroutine(StartCooldown());
    }


    private void Awake()
    {
        Available = true;
    }

    IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(Cooldown);
        Available = true;
    }
}
