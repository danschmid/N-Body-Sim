using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DateTimeInputHandler : MonoBehaviour
{
    [SerializeField] private InputField[] inputFields;
    public EventManager Events = EventManager.events;

    [SerializeField] private bool PreciseTime = false; //adds a milliseconds field to time input fields
    [SerializeField] private InputType FieldType;

    private int CurrentValue;
    DateTimeInputHandler[] siblings;

    public enum InputType
    {
        StartDate,
        EndDate,
        StartTime,
        EndTime
    }


    public void Start()
    {
        inputFields = GetComponentsInChildren<InputField>();

        Events.DateTimeChangedEvent += SetInput;
        foreach(InputField inputField in inputFields)
        {
            inputField.onEndEdit.AddListener(InputValueChanged);
        }
    }


    private void SetInput(DateTime value, InputType inputType)  //called when DataManager's date/time parameters are modified to reflect any unexpected changes from parsing or external changes in the user input
    {
        bool startDateTime = (inputType == InputType.StartTime || inputType == InputType.StartDate);  //if not startDateTime, it is an end date/time

        if ((this.FieldType == InputType.StartTime && startDateTime) || (this.FieldType == InputType.EndTime && !startDateTime))
        {
            inputFields[0].text = value.Hour.ToString();
            inputFields[0].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Hour.ToString());  //lots of getcomponent can probably be avoided, but it works for now.  
                                                                                                            //CheckValidInput just needed to properly format string, but it can't hurt to check that the parsed output is still valid
            inputFields[1].text = value.Minute.ToString();
            inputFields[1].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Minute.ToString());

            inputFields[2].text = value.Second.ToString();
            inputFields[2].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Second.ToString());

            if (PreciseTime && inputFields[3] != null)
            {
                inputFields[3].text = value.Millisecond.ToString();
                inputFields[3].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Millisecond.ToString());

            }

        }
        else if ((this.FieldType == InputType.StartDate && startDateTime) || (this.FieldType == InputType.EndDate && !startDateTime))
        {
            inputFields[0].text = value.Month.ToString();
            inputFields[0].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Month.ToString());
             
            inputFields[1].text = value.Day.ToString();
            inputFields[1].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Day.ToString());

            inputFields[2].text = value.Year.ToString();
            inputFields[2].GetComponent<IncrementalInputHandler>().CheckValidInput(value.Year.ToString());
        }

        //don't call inputvalue changed. we have to set values and do the checks without triggering the InputEvent again, as that could cause a loop
    }


    public void InputValueChanged(string text)
    {
        //Debug.Log("Text: " + text);
        text = text.Trim();

        DateTime? dateTime;
        if (text != "" && CheckFieldsAndCombine(out dateTime) && dateTime != null)
        {
            EventManager.events.RaiseInputEvent((DateTime)dateTime, this.FieldType);
        }
    }


    private bool CheckFieldsAndCombine(out DateTime? result)  //For a valid DateTime parse, this component's input and the inputs of all its siblings which share this script need to be valid
    {
        bool valid = true;
        result = null;
        IncrementalInputHandler[] IIHs = new IncrementalInputHandler[] {null, null, null, null};

        for(int i=0; i<inputFields.Count(); i++)
        {
            IIHs[i] = inputFields[i].GetComponent<IncrementalInputHandler>();

            if (!IIHs[i].CheckValidInput(inputFields[i].text))  //probably don't need to check them again here since the incrementalinputfields handle that themselves
            {
                valid = false;
            }
        }

        if(valid)
        {
            if (this.FieldType == InputType.StartDate || this.FieldType == InputType.EndDate)
            {
                //Debug.Log("date: " + IIHs[0].CurrentValue + ", " + IIHs[1].CurrentValue + ", " + IIHs[2].CurrentValue);
                result = new DateTime(IIHs[2].CurrentValue, IIHs[0].CurrentValue, IIHs[1].CurrentValue);
                //Debug.Log("result: " + result);
            }
            else if (this.FieldType == InputType.StartTime || this.FieldType == InputType.EndTime)
            {
                if(PreciseTime && inputFields[3] != null)
                {
                    result = new DateTime(1, 1, 1, IIHs[0].CurrentValue, IIHs[1].CurrentValue, IIHs[2].CurrentValue, IIHs[3].CurrentValue);
                }
                else
                {
                    result = new DateTime(1, 1, 1, IIHs[0].CurrentValue, IIHs[1].CurrentValue, IIHs[2].CurrentValue);
                }

            }
        }

        return valid;
    }
}
