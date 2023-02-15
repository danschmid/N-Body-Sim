﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    /// <summary>
    /// The Horizons ID# of the celestial body this toggle represents
    /// </summary>
    public string bodyID;
    public NB nb;
    public Client cl;
    public BodiesMenu bm;
    public SidebarUI sb;


    // Start is called before the first frame update
    void Start()
    {
        GameObject controller = GameObject.Find("Client");
        nb = controller.GetComponent<NB>();
        cl = controller.GetComponent<Client>();
        bm = controller.GetComponent<BodiesMenu>();
        sb = GameObject.Find("Sidebar Menu").GetComponent<SidebarUI>();
        bm.GetSelectedBodies += UpdateSelections;
        Toggle toggle = gameObject.GetComponent<Toggle>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateSelections() 
    {
        
        
    }
}
