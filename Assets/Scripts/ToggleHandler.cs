using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class ToggleHandler : MonoBehaviour
{
    public string BodyID;
    public Toggle toggle;


    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void OnToggle(bool value)
    {
        EventManager.RaiseToggleEvent(value, BodyID);
    }
}
