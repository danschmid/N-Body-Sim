using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private InputType inputType;
    public EventManager Events = EventManager.events;

    public enum InputType  //to add a unique input, just add a new inputtype here to identify it with, and add logic on the listening end of the event to decide what to do with it
    {
        StepSize,
        Other
    }

    
    void Start()
    {
        inputField = this.GetComponent<InputField>();
        inputField.onEndEdit.AddListener(InputValueChanged);
        Events.InputChangedEvent += SetInput;
    }

    void OnDestroy()
    {
        Events.InputChangedEvent -= SetInput;
    }

    private void InputValueChanged(string text)
    {
        Debug.Log("Text: " + text);
        text = text.Trim();
        if (text!="")
        {
            EventManager.events.RaiseInputEvent(text, inputType);
        }
        else   //clear the input in case the user just entered whitespaces
        {
            inputField.text = "";
        }
    }

    public void SetInput(string input, InputHandler.InputType type)
    {
        Debug.Log("Setting input")
;        if(inputType == type)
        {
            inputField.text = input;
        }
    }
}
