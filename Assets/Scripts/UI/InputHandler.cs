using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    public InputField inputField;
    public string input;

    public void inputValueChanged(string text)
    {
        EventManager.events.RaiseInputEvent(text, transform.name);
    }
}
