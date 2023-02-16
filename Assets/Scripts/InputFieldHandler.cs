using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldHandler : MonoBehaviour
{
    public string userAnswer = "";
    // Start is called before the first frame update
    void Start()
    {
        var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitName);
        input.onEndEdit = se;
    }

    private void SubmitName(string arg0)
    {
        // Store the user answer here                
        Debug.Log(arg0);
        userAnswer = arg0;
    }
}
