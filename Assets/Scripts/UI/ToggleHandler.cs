using UnityEngine;
using UnityEngine.UI;


public class ToggleHandler : MonoBehaviour
{
    public string ID;  //This string will be set if the toggle controls the selection of a body from the data tab
    public string Setting;  //This will be set instead of ID if the toggle instead controls a generic setting.  This and ID should never be set at the same time (maybe I should add a check)
    public Toggle toggle;
    PageHandler page;


    public void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
        page = toggle.GetComponentInParent<PageHandler>();
        if(page == null)
        {
            Debug.LogWarning("No pagehandler found");
        }
    }

    public void OnToggle(bool value)
    {
        EventManager.events.RaiseToggleEvent(value, this);
        page.PopulateBodySelectionList();
    }
}


