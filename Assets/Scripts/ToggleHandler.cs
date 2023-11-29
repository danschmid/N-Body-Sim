using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class ToggleHandler : MonoBehaviour
{
   //public BodiesMenu bm;
    public SidebarUI sb;
    public string BodyID;
    public int indexNum;
    public Toggle toggle;


    // Start is called before the first frame update
    void Start()
    {
        //sb = GameObject.Find("Sidebar Menu").GetComponent<SidebarUI>();

        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void OnToggle(bool value)
    {
        EventManager.RaiseToggleEvent(value, BodyID);
    }
}
