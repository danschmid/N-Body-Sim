using UnityEngine;

public class PageHandler : MonoBehaviour
{
    //I'm just going to make this handler exclusively for refreshing selections list in the sim tab for now because I'm too lazy to set it up properly.
    //If more tab pages need similar functionality I can make this event based.  I know the data tab also needs a better way to refresh than what I am currently doing

    public SidebarUI sidebarUI;
    public RectTransform scrollrect;
    void OnEnable()
    {
    }

    public void PopulateBodySelectionList()
    {
        sidebarUI.PopulateSelectionList(scrollrect);
    }
}
