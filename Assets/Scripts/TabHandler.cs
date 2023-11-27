using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class TabHandler : MonoBehaviour
{
    public GameObject page;  //have to manually set a reference to the page you want the tab to control
    
    private bool needsLoaded = false;  //most pages won't need it, bodyselection page will
    private SidebarUI sidebarUI; 

    void Awake()  //awake so that it is called on initialization even if the script is disabled
    {
        sidebarUI = GameObject.Find("Sidebar Menu").GetComponent<SidebarUI>();

        if (page.name == "pg2 - ObjectDataSel") //only page2 needs this for refreshing selection.   
        {
            needsLoaded = true;
        }
    }

    private void OnClick()
    {
        //if any other tabs in the group are active, deactivate them
        foreach (Button tab in transform.parent.GetComponentsInChildren<Button>())
        {
            if (tab == this.gameObject.GetComponent<Button>())  //dont deactivate the tab that was just clicked
            {
                continue;
            }

            TabHandler th = tab.GetComponent<TabHandler>();
            if (th.page.activeInHierarchy)
            {
                th.page.SetActive(false);
                break;  //only one tab can be active at a time
            }
        }

        page.SetActive(true);

        if(needsLoaded)  //do this last or else it will try to update before page is set to active, and it will fail
        {
           // needsLoaded = !needsLoaded;
            sidebarUI.UpdateBodySelectionList();
        }
    }
}
