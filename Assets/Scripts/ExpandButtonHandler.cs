using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandButtonHandler : MonoBehaviour
{
    public Canvas expandCanvas;
    public Text buttontext;
    private bool isExpanded = false;
    
    public void EnableCanvas()  //toggles the expansion of the canvas area, and the text of the button. The canvas area must be a sibling of the expandable header, below it in hierarchy
    {
        if(isExpanded)
        {
            buttontext.text = "+";
        }
        else
        {
            buttontext.text = "-";  //Need to improve the positioning of this when the text changes. It's not centered.
            
        }
        expandCanvas.gameObject.SetActive(!isExpanded);
        isExpanded = !isExpanded;
    }
}
