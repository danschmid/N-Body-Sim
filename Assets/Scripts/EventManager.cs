using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static Action<bool, string> ToggleEvent;

    public static void RaiseToggleEvent(bool isToggled, string id)
    {
        ToggleEvent?.Invoke(isToggled, id);
    }
}
