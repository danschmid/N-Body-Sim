using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpDownToggleHandler : MonoBehaviour
{
    private InputHandler parentInputHandler;
    [SerializeField] private bool upOrDown;  //it is an up button if false, down button if true
    void Start()
    {
        this.GetComponentInParent<Button>().onClick.AddListener(ButtonClick);
        parentInputHandler = this.transform.parent.GetComponent<InputHandler>();
    }

    private void ButtonClick()
    {
        parentInputHandler.incrementDecrement(upOrDown);  //increments value of an input field up or down by 1
    }
}
