using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class TabHandler : MonoBehaviour
{
    public GameObject page;  //have to manually set a reference to the page you want the tab to control
    
    public void OnClick()
    {
        //if any other tabs in the group are active, deactivate them
        foreach (Button tab in transform.parent.GetComponentsInChildren<Button>())
        {
            if (tab == this.gameObject.GetComponent<Button>())  //dont deactivate the tab that was just clicked
            {
                continue;
            }

            TabHandler th = tab.GetComponent<TabHandler>();
            if (th.page.activeInHierarchy == true)
            {
                th.page.SetActive(false);
                break;  //only one tab can be active at a time
            }
        }

        //Debug.Log("Setting active: " + page.gameObject.name);
        page.SetActive(true);
    }
}
