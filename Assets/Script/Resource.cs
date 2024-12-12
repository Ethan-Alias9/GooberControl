using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "ScriptableObjects/Resource")]
public class Resource : ScriptableObject
{
    public static Dictionary<Resource, List<ResourceNode>> harvestableNodes = new();


    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public bool Searchable { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    public static Resource[] AllResources { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void LoadResources()
    {
        AllResources = Resources.LoadAll<Resource>("");

    }

}
