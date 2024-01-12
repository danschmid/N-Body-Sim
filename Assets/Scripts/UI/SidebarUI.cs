using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditorInternal;

public class SidebarUI : MonoBehaviour
{
    public Client client;
    public DataManager DataMan = DataManager.Instance;

    Dictionary<string, string[]> index;

    public GameObject TogglePrefab;
    public GameObject ExpandHeaderPrefab;
    public GameObject ExpandAreaPrefab;

    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject page4;

    public bool firstLoad = false;
    
    public void UpdateBodySelectionList()  //gets the celestial bodies available on Horizons from Client.Index, and populates the body selection list in the sidebar
    {
        index = DataMan.HorizonsIndex; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
        Dictionary<string, List<string>> sortedIndex = new Dictionary<string, List<string>> {}; //[first char of ID#, [all horizons ID#s with that first char]]
        int indexlength = index.Count;


        foreach (KeyValuePair<string, string[]> body in index)  //I should probably just sort the HorizonsIndex from the start rather than doing it here.  
        {
            int keylen = body.Key.Length;
            if(body.Key == "10")
            {
                sortedIndex.Add("Sun", new List<string> { });
                sortedIndex["Sun"].Add(body.Key);
                continue;
            }
            else if(keylen < 3 || keylen > 5)  //skip barycenters and lagrange points, except for the Sun /// skip some comets, asteroids, and dwarf planets
            {
                continue;
            }
            else if (body.Value[0].Contains("Barycenter"))  //skip barycenters
            {
                continue;
            }
            else if (body.Value[2].Contains("Lagrange"))  //also skip lagrange points, but I should store all of these and make them available as massless unsimulated particles.  Also figure out how to calculate these myself
            {
                continue;
            }


            string firstChar = body.Key.Substring(0, 1);

            if (keylen <= 5)
            { 
                if(!sortedIndex.Keys.Contains(firstChar))
                {
                    sortedIndex.Add(firstChar, new List<string> { });
                }
                if (body.Key.EndsWith("99")) //this indicates that it is a planet, all other positive numbers with length 3-5 starting with the same digit will be this planet's satellites
                {
                    sortedIndex[firstChar].Insert(0, body.Key);
                }
                else
                {
                    sortedIndex[firstChar].Add(body.Key); //In the sorted index, the first element for each key will be the major planet, and any more will be its satellites.  if firstChar is "-" (their ID is negative) then it is a spacecraft.
                }
            }

        }

        foreach (KeyValuePair<string, List<string>> system in sortedIndex)
        {
            Canvas expandArea = null;
            GameObject expandHeader = null;
            GameObject toggle = null;

            if (system.Key == "-")  //Horizons IDs that start with a "-" are always spacecraft (Pretty sure anyways)
            {
                expandHeader = InstantiateHeader("Spacecraft", out expandArea);

                foreach (string id in system.Value)
                {
                    toggle = InstantiateToggle(id, expandArea);
                }

            }
            else
            {
                toggle = InstantiateToggle(system.Value[0], null);
                if (system.Value.Count > 1)  //If there is more than one body in the system values list then the main body has satellites.  Put them in an expandable header under main body
                {
                    expandHeader = InstantiateHeader(DataMan.GetBestName(system.Value[0]) + " Satellites:", out expandArea);

                    for (int i=1; i<system.Value.Count; i++)
                    {
                        string id = system.Value[i];
                        toggle = InstantiateToggle(id, expandArea);

                    }
                }
            }
        }
    }

    
    public void PopulateSelectionList(RectTransform scrollArea)  //Populates the selection list of the simulation tab with the bodies you have selected from the data tab.  (TODO: also add custom bodies to this list)
    {
        foreach (string name in DataMan.PreferredNames)
        {

            GameObject text = InstantiateText(name, scrollArea);
        }
    }

    GameObject InstantiateHeader(string name, out Canvas ExpandArea)  //returns the header itself, as well as the expandable area which it controls (where the content will go)
    {
        GameObject expandHeader = Instantiate(ExpandHeaderPrefab, page2.transform);

        Transform horizontalElements = expandHeader.transform.Find("Horizontal Elements");  //Find and getComponent are pretty slow, try to find a way to do these less often
        Text headerText = horizontalElements.GetComponentInChildren<Text>();
        headerText.text = name;

        ExpandableHeaderHandler buttonHandler = expandHeader.GetComponent<ExpandableHeaderHandler>();
        ExpandArea = buttonHandler.expandCanvas;
        return expandHeader;
    }

    GameObject InstantiateToggle(string id, Canvas parent = null, bool startOn = false)
    {
        if (parent == null)
        {
            parent = page2.GetComponent<Canvas>();
        }

        GameObject toggle = Instantiate(TogglePrefab, parent.transform);

        Text toggleText = toggle.GetComponentInChildren<Text>();
        toggleText.text = DataMan.GetBestName(id);  //sets the name of the toggle
        toggle.GetComponent<ToggleHandler>().ID = id;  //also set the id variable in ToggleHandler
        if(startOn)
        {
            toggle.GetComponent<Toggle>().isOn = true;
        }

        return toggle;
    }

    GameObject InstantiateText(string text, RectTransform parent)
    {
        GameObject newTextObject = new GameObject("DynamicText");
        Text newTextComponent = newTextObject.AddComponent<Text>();

        // Set text properties
        newTextComponent.text = text;
        newTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Use the built-in Arial font or replace it with your custom font
        newTextComponent.color = Color.black;

        RectTransform rectTransform = newTextObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(170f, 20f);
        rectTransform.SetParent(parent, false); // Set the parent to a scroll area
        //rectTransform.localPosition = Vector3.zero;


        return newTextObject;
    }
}
