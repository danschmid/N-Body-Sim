using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public abstract class ToggleHandlerBase : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public abstract void OnToggle(bool value);
}

public class DefaultToggleHandler : ToggleHandlerBase  //this is for generic settings toggles that will mostly be placed into the menu by hand
{
    string something;  //I need to decide how to store the setting that the toggle should change.  maybe something other than a string.  Settings should be mostly in DataMan?
    public override void OnToggle(bool value)
    {
        EventManager.RaiseToggleEvent(value, something);
    }
}

public class BodyToggleHandler : ToggleHandlerBase  //this is for the instantiated toggles in the body selection list
{
    public string BodyID;

    public override void OnToggle(bool value)
    {
        EventManager.RaiseToggleEvent(value, BodyID);
    }
}


public class ToggleBuilders : MonoBehaviour
{
    public GameObject TogglePrefab;
    public GameObject InstantiateDefaultToggle(string something, Canvas parent, bool startOn = false)
    {
        GameObject toggle = Instantiate(TogglePrefab, parent.transform);
        toggle.AddComponent<DefaultToggleHandler>();
        return toggle;
    }

    public GameObject InstantiateBodyToggle(string something, Canvas parent, bool startOn = false)
    {
        GameObject toggle = Instantiate(TogglePrefab, parent.transform);
        toggle.AddComponent<DefaultToggleHandler>();
        return toggle;
    }



    //This got overly complicated I think.  MAybe Ill just decide what to do with the toggle by the string passed with the event
}