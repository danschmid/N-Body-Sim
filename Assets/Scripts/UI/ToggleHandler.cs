using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class ToggleHandler : MonoBehaviour
{
    public string ID;  //This string will be set if the toggle controls the selection of a body from the data tab
    public string Setting;  //This will be set instead of ID if the toggle instead controls a generic setting.  This and ID should never be set at the same time (maybe I should add a check)
    public Toggle toggle;


    public void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle); 
    }

    public void OnToggle(bool value)
    {
        EventManager.events.RaiseToggleEvent(value, this);
    }
}


