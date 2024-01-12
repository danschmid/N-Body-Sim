using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableHeaderHandler : MonoBehaviour
{
    public Canvas expandCanvas;  //should be the expand area that is the child of the header
    public Text buttontext;
    public bool isExpanded = false;
    
    public void EnableCanvas()
    {
        if(isExpanded)
        {
            buttontext.text = "+";
        }
        else
        {
            buttontext.text = "-";  //Need to improve the positioning of this when the text changes. It's not centered.  or just use 2 images instead of text
            
        }
        expandCanvas.gameObject.SetActive(!isExpanded);
        isExpanded = !isExpanded;
    }
}
