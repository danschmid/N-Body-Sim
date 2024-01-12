using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        if (ToggleEvent != null)  //This basically ensures that an instance of DataManager exists (although I might move it to SidebarUI) 
        {
            ToggleEvent(isToggled, th);
        }
        else
        {
            //TODO: If no instance exists I should not allow it to stay toggled, as it may lead to a mismatch between the toggle state and the actual setting
        }
    }

    public Action<bool, ToggleHandler> StartSimulation;
    public void RaiseStartSimulationEvent()
    {

    }

    


}
