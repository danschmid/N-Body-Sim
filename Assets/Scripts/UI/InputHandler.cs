using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputField inputField;

    
    void Start()
    {
        inputField = this.GetComponent<InputField>();
        inputField.onEndEdit.AddListener(InputValueChanged);
    }

    private void InputValueChanged(string text)
    {
        Debug.Log("Text: " + text);
        text = text.Trim();
        if (text!="")
        {
            EventManager.events.RaiseInputEvent(text, transform.name);
        }
        else   //clear the input in case the user just entered whitespaces
        {
            inputField.text = "";
        }
    }
}
