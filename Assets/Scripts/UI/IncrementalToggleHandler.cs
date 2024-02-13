using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncrementalToggleHandler : MonoBehaviour
{
    private IncrementalInputHandler parentInputHandler;
    [SerializeField] private bool upOrDown;  //it is an up button if false, down button if true.  Set it in the editor
    void Start()
    {
        this.GetComponentInParent<Button>().onClick.AddListener(ButtonClick);
        parentInputHandler = this.transform.parent.GetComponent<IncrementalInputHandler>();
    }

    private void ButtonClick()
    {
        parentInputHandler.IncrementDecrement(upOrDown);  //increments value of an input field up or down by 1 if it contains a number.  Could use an event but these only ever need to communicate with their parents
    }
}
