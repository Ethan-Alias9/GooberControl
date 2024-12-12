using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    static Notification instance;

    public TMP_Text text;
    float timer;

    private void Awake()
    {
        instance = this;
    }
    public static void Notify(string message)
    {
        instance.text.text = message;
        instance.timer = 5;
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        text.enabled = timer > 0;
    }
}
