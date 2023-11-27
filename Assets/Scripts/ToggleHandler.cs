using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    /// <summary>
    /// The Horizons ID# of the celestial body this toggle represents
    /// </summary>
    public BodiesMenu bm;
    public SidebarUI sb;

    public int indexNum;


    // Start is called before the first frame update
    void Start()
    {
        GameObject controller = GameObject.Find("Client");
        //bm = controller.GetComponent<BodiesMenu>();
        sb = GameObject.Find("Sidebar Menu").GetComponent<SidebarUI>();
        //bm.GetSelectedBodies += UpdateSelections;
        Toggle toggle = gameObject.GetComponent<Toggle>();

        toggle.onValueChanged.AddListener(OnClick);

    }

    public void OnClick(bool isOn)
    {
        sb.ToggleStates[indexNum] = isOn;  //probably should change this, seems dumb to do it this way
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
