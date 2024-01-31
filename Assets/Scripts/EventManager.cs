using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager events;
    public void Awake()
    {
        events = this;
    }


    public Action<bool, ToggleHandler> ToggleEvent;
    public void RaiseToggleEvent(bool isToggled, ToggleHandler th)
    {
        Debug.Log("Toggle event raised");
        if (ToggleEvent != null)  //This basically ensures that an instance of DataManager exists (although I might move it to SidebarUI) 
        {
            ToggleEvent(isToggled, th);
        }
        else
        {
            Debug.LogWarning("No instance of Data Manager found!");
            //TODO: If no instance exists I should not allow it to stay toggled, as it may lead to a mismatch between the toggle state and the actual setting
        }
    }

    public Action<ToggleHandler> StartSimulation;
    public void RaiseStartSimulationEvent()
    {

    }

    public Action<string, string> InputEvent;
    public void RaiseInputEvent(string input, string fieldName)
    {
        Debug.Log("InputEvent");
        InputEvent(input, fieldName);
    }

    public Action<string> ButtonEvent;
    public void RaiseButtonEvent(string type)
    {

    }

    


}
