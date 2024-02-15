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
    public EventManager Events = EventManager.events;

    [SerializeField] private bool PreciseTime = false; //adds a milliseconds field to time input fields
    [SerializeField] public InputType inputType;

    private int CurrentValue;
    DateTimeInputHandler[] siblings;
    [SerializeField] private IncrementalInputHandler[] IncrementalInputHandlers;

    public enum InputType
    {
        StartDate,
        EndDate,
        StartTime,
        EndTime
    }

    public void Awake()
    {
        IncrementalInputHandlers = GetComponentsInChildren<IncrementalInputHandler>();
    }
    public void Start()
    {
        if(inputType == InputType.StartTime || inputType == InputType.StartDate)
        {
            Events.StartDateTimeChangedEvent += SetFields;
        }
        else if (inputType == InputType.EndTime || inputType == InputType.EndDate)
        {
            Events.EndDateTimeChangedEvent += SetFields;
        }


        /*foreach (IncrementalInputHandler iih in IncrementalInputHandlers)
        {
            iih.inputField.onEndEdit.AddListener(InputValueChanged);
        }*/
        
    }

    private void CheckDate(int days, int month, int year)  //check that the number of days corresponds to the
    {
        if(days > DateTime.DaysInMonth(year, month))
        {

        }
    }

    private void SetDaysInMonth(int month, int year = 1999) //if we don't know the year yet, just assume it isn't a leap year
    {
        if(inputType == InputType.StartDate || inputType == InputType.EndDate)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            IncrementalInputHandlers[1].maxValue = daysInMonth;
            if ((int)IncrementalInputHandlers[1].CurrentValue > daysInMonth)
            {
                IncrementalInputHandlers[1].InputValueChanged(daysInMonth.ToString());
            }
        }
        else
        {
            Debug.LogWarning("This isn't a date field, why are you calling this?");
        }
    }

    public void OnDestroy()
    {
        Events.StartDateTimeChangedEvent -= SetFields;
        Events.EndDateTimeChangedEvent -= SetFields;
    }


    private void SetFields(DateTime value, InputType setInputType)  //called when DataManager's date/time parameters are modified to reflect any unexpected changes from parsing or external changes in the user input
    {
        bool thisIsDate = (inputType == InputType.StartDate || inputType == InputType.EndDate);

        if (thisIsDate)
        {
            IncrementalInputHandlers[0].SetText(value.Month.ToString());

            IncrementalInputHandlers[1].SetText(value.Day.ToString());

            IncrementalInputHandlers[2].SetText(value.Year.ToString());

            SetDaysInMonth(value.Month, value.Year);
        }
        else if (!thisIsDate)  //if this isn't a date, it's a time
        {
            IncrementalInputHandlers[0].SetText(value.Hour.ToString());

            IncrementalInputHandlers[1].SetText(value.Minute.ToString());

            IncrementalInputHandlers[2].SetText(value.Second.ToString());

            if (PreciseTime && IncrementalInputHandlers[3] != null)
            {
                IncrementalInputHandlers[3].SetText(value.Millisecond.ToString());
            }
        }

        //don't call inputvalue changed. we have to set values and do the checks without triggering the InputEvent again, as that could cause a loop
    }


    public void InputValueChanged(string text, IncrementalInputHandler iih)
    {
        //Debug.Log("Text: " + text);
        text = text.Trim();

        DateTime? dateTime;
        if((iih == IncrementalInputHandlers[0] || iih == IncrementalInputHandlers[2]) && (inputType == InputType.StartDate || inputType == InputType.EndDate))  //then the month or year was changed and we should change the permitted days in the month accordingly
        {
            int? year = IncrementalInputHandlers[2].CurrentValue;
            int? day = IncrementalInputHandlers[0].CurrentValue;
            if (year == null)  //if there is no year
            {
                SetDaysInMonth((int)day);
            }
            else
            {
                SetDaysInMonth((int)day, (int)year);
            }
        }


        if (text != "" && CheckFieldsAndCombine(out dateTime) && dateTime != null)
        {
            EventManager.events.RaiseInputEvent((DateTime)dateTime, this.inputType);
        }
    }


    private bool CheckFieldsAndCombine(out DateTime? result)  //For a valid DateTime parse, this component's input and the inputs of all its siblings which share this script need to be valid
    {
        bool valid = true;
        result = null;

        for(int i=0; i<IncrementalInputHandlers.Count(); i++)
        {
            if (!IncrementalInputHandlers[i].CheckValidInput(IncrementalInputHandlers[i].inputField.text))  //probably don't need to check them again here since the incrementalinputfields handle that themselves
            {
                valid = false;
            }
        }

        if(valid)
        {
            int val0 = (int)IncrementalInputHandlers[0].CurrentValue;
            int val1 = (int)IncrementalInputHandlers[1].CurrentValue;
            int val2 = (int)IncrementalInputHandlers[2].CurrentValue;
            if (this.inputType == InputType.StartDate || this.inputType == InputType.EndDate)
            {
                //Debug.Log("date: " + IIHs[0].CurrentValue + ", " + IIHs[1].CurrentValue + ", " + IIHs[2].CurrentValue);
                result = new DateTime(val2, val0, val1);
                //Debug.Log("result: " + result);
            }
            else if (this.inputType == InputType.StartTime || this.inputType == InputType.EndTime)
            {
                if(PreciseTime && IncrementalInputHandlers[3] != null)
                {
                    int val3 = (int)IncrementalInputHandlers[3].CurrentValue;
                    result = new DateTime(1, 1, 1, val0, val1, val2, val3);
                }
                else
                {
                    result = new DateTime(1, 1, 1, val0, val1, val2);
                }
            }
        }

        return valid;
    }
}
