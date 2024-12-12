using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaviOrUnity : MonoBehaviour
{
    public enum State
    {
        Unity,
        Navi,
    }

    public static State state = State.Unity;

    public void Toggle()
    {
        if (state == State.Unity)
            state = State.Navi;
        else
            state = State.Unity;
    }
}
