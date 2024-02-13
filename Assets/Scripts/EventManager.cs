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
            Debug.LogWarning("No instance of Data Manager found!");
            //TODO: If no instance exists I should not allow it to stay toggled, as it may lead to a mismatch between the toggle state and the actual setting
        }
    }

    public Action<ToggleHandler> StartSimulation;
    public void RaiseStartSimulationEvent()
    {

    }

    public Action<string, string> InputEvent;
    public Action<DateTime, DateTimeInputHandler.InputType> DateTimeInputEvent;
    public void RaiseInputEvent(string input, string fieldName)
    {
        InputEvent(input, fieldName);
    }
    public void RaiseInputEvent(DateTime input, DateTimeInputHandler.InputType inputType)
    {
        DateTimeInputEvent(input, inputType);
    } 
    


    public Action<DateTime, DateTimeInputHandler.InputType> DateTimeChangedEvent;
    public void RaiseDateTimeChangedEvent(DateTime dateTime, DateTimeInputHandler.InputType inputType)
    {
        DateTimeChangedEvent(dateTime, inputType);
    }



    public Action<string> ButtonEvent;
    public void RaiseButtonEvent(string type)
    {

    }

    public Action LockoutEvent;
    public void LockoutWhileLoading()
    {
        LockoutEvent();
    }
    public Action UnlockEvent;
    public void UnlockAfterLoading()
    {
        UnlockEvent();
    }




}
