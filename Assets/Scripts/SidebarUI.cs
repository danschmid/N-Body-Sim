using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SidebarUI : MonoBehaviour
{
    public RectTransform panel;
    public Client client;
    public GameObject bodySelectionList;
    Dictionary<string, string[]> index;
    public List<bool> ToggleStates;
    public List<string> ToggleNames;

    public GameObject TogglePrefab;
    public GameObject ExpandHeaderPrefab;
    public GameObject ExpandAreaPrefab;

    public DataManager DataMan = DataManager.Instance;

    int defaultnamecount = 0;

    public List<string> GetSelectedIDs()
    {
        int itr = 0;
        foreach (bool boo in ToggleStates)
        {
            if (boo == true)
            {
                DataMan.SelectedBodies.Add(ToggleNames[itr]);
            }
            itr++;
        }
        if(DataMan.SelectedBodies.Count < 1)
        {
            Debug.LogWarning("You must choose at least one body to simulate");
            return null;
        }

        return DataMan.SelectedBodies;
    }

    void Start()
    {
        bodySelectionList = GameObject.Find("pg2 - ObjectDataSel");
    }
    
    public void UpdateBodySelectionList()  //gets the celestial bodies available on Horizons from Client.Index, and populates the body selection list in the sidebar
    {

        index = DataMan.Index; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
        Dictionary<string, List<string>> sortedIndex = new Dictionary<string, List<string>> {}; //[first char of ID#, [all horizons ID#s with that first char]]   this helps to sort the major planetary bodies and their satellites for populating the list
        int indexlength = index.Count;


        foreach (KeyValuePair<string, string[]> body in index)
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
            else if (body.Value[2].Contains("Lagrange"))  //also skip lagrange points, but I should store all of these and make them available
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

        
        int toggleCount = 0;
        foreach (KeyValuePair<string, List<string>> system in sortedIndex)
        {
            //GameObject expandHeader = null;
            GameObject expandArea = null;
            GameObject toggle = null;

            if (system.Key == "-")
            {
                expandArea = InstantiateHeader("Spacecraft");

                foreach (string id in system.Value)
                {
                    toggle = InstantiateToggle(DataMan.GetBestName(id), expandArea);
                    ToggleStates.Add(toggle.GetComponent<Toggle>().isOn);
                    toggle.GetComponent<ToggleHandler>().indexNum = toggleCount;
                    //Debug.Log("indexnum: " + toggle.GetComponent<ToggleHandler>().indexNum);
                    toggleCount++;
                    ToggleNames.Add(id);
                }


            }
            else
            {
                string bestName = DataMan.GetBestName(system.Value[0]);
                ToggleNames.Add(system.Value[0]);
                toggle = InstantiateToggle(bestName, null, true);
                ToggleStates.Add(toggle.GetComponent<Toggle>().isOn);
                toggle.GetComponent<ToggleHandler>().indexNum = toggleCount;
                Debug.Log("indexnum: " + toggle.GetComponent<ToggleHandler>().indexNum);
                toggleCount++;

                if (system.Value.Count > 1)
                {
                    expandArea = InstantiateHeader(bestName + " Satellites:");

                    List<string> values = system.Value;
                    values.RemoveAt(0);
                    foreach (string id in values)
                    {
                        toggle = InstantiateToggle(DataMan.GetBestName(id), expandArea);
                        ToggleStates.Add(toggle.GetComponent<Toggle>().isOn);
                        toggle.GetComponent<ToggleHandler>().indexNum = toggleCount;
                        //Debug.Log("indexnum: " + toggle.GetComponent<ToggleHandler>().indexNum);
                        toggleCount++;
                        ToggleNames.Add(id);

                    }
                }
            }
        }
        Debug.Log("Toggles Length: " + ToggleStates.Count + ", Index Length: " + sortedIndex.Count());
    }

    void AddToDict()
    {

    }


    GameObject InstantiateHeader(string name)
    {
        GameObject expandHeader = Instantiate(ExpandHeaderPrefab, bodySelectionList.transform);
        expandHeader.transform.SetParent(bodySelectionList.transform);

        Transform horizontalElements = expandHeader.transform.Find("Horizontal Elements");
        Text headerText = horizontalElements.GetComponentInChildren<Text>();
        headerText.text = name;

        GameObject expandArea = Instantiate(ExpandAreaPrefab, bodySelectionList.transform);
        expandArea.transform.SetParent(bodySelectionList.transform);

        ExpandButtonHandler buttonHandler = horizontalElements.GetComponentInChildren<ExpandButtonHandler>();
        buttonHandler.expandCanvas = expandArea.GetComponent<Canvas>();

        return expandArea;
    }

    GameObject InstantiateToggle(string name, GameObject parent = null, bool startOn = false)
    {
        if (parent == null)
        {
            Debug.Log("parent set to default");
            parent = bodySelectionList;
        }
        GameObject toggle = Instantiate(TogglePrefab, parent.transform);
        toggle.transform.SetParent(parent.transform);

        Text toggleText = toggle.GetComponentInChildren<Text>();
        toggleText.text = name;
        if(startOn)
        {
            toggle.GetComponent<Toggle>().isOn = true;
        }

        return toggle;
    }
}
