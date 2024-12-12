using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCounter : MonoBehaviour
{
    [System.NonSerialized] public Resource trackedResource;

    public Image image;
    public TMP_Text resourceName;
    public TMP_Text ownedCount;
    public GameObject findButton;
    private void LateUpdate()
    {
        image.sprite = trackedResource.Sprite;
        resourceName.text = trackedResource.Name;
        ownedCount.text = $"Owned: {PlayerInfo.instance.resourceAmounts[trackedResource]}";
        if (trackedResource.Searchable)
            findButton.SetActive(true);
        else
            findButton.SetActive(false);

    }

    public void Find()
    {
        Goober.instance.Find(trackedResource);
    }
}
