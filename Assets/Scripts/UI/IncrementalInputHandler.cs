using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class IncrementalInputHandler : MonoBehaviour
{
    [SerializeField] public InputField inputField { get; private set; }

    [SerializeField] private int minValue=-1;  //if it is equal to -1, there is no enforced min or max value.  Because of this, this input field cannot handle negatives
    [SerializeField] private int maxValue=-1;  //even when set to -1, maximum value will still be limited by the InputFields character limit 

    public int CurrentValue;
    private DateTimeInputHandler parent;
    [SerializeField] public InputType inputType;

    public enum InputType
    {
        dateTime,
        integer
    }
    
    public void Awake()  //Awake is called before start
    {
        parent = GetComponentInParent<DateTimeInputHandler>();
        inputField = this.GetComponent<InputField>();
    }
    public void Start()
    {
        inputField.onEndEdit.AddListener(InputValueChanged); 
        if(minValue < -1)
        {
            minValue = -1;  //-1 just means no limit, cannot be less
        }
        if (maxValue < -1)
        {
            maxValue = -1;
        }
    }

    public void InputValueChanged(string text)
    {
        text = text.Trim();
        if (text!="")
        {
            int? output = 0;
            if(CheckValidInput(text, out output) && inputType == InputType.integer && output != null) //if it is InputType.dateTime, then it should have a DateTimeInputHandler that will listen for changes to its children and handle them
            {
                CurrentValue = (int)output;
                EventManager.events.RaiseInputEvent(CurrentValue, inputType);
            }
            else if (CheckValidInput(text, out output) && inputType == InputType.dateTime)
            {
                CurrentValue = (int)output;
                parent.InputValueChanged(text);
            }
        }
        else   //clear the input in case the user just entered whitespaces (even if inputType = dateTime we should do this here at the individual input field level)
        {
            inputField.text = "";
        }
    }


    public void SetText(string text)
    {
        if (text.Length <= inputField.characterLimit)
        {
            inputField.text = text;
            CheckValidInput(text);
        }
        else
        {
            Debug.LogWarning("Warning! Unable to set input text, it might no longer represent the true value");
        }
    }

    public void IncrementDecrement(bool decrement)  //check if the field contains a number. If so, either increment or decrement the number by 1, depending on whether up or down button was pressed.
    {
        int add = 1;
        if(decrement) { add = -1; }

        if (int.TryParse(inputField.text, out CurrentValue))
        {
            int result = (CurrentValue + add);
            if ((maxValue != -1 && result > maxValue) || result.ToString().Count() > inputField.characterLimit) //loop to minimum if maxValue !=-1 and result exceeds it, or if it exceeds char limit
            {
                inputField.text = GetMinValue();
            }
            else if ((minValue != -1 && result < minValue) || result < 0)  //loop to maximum if minimum !=-1 and result becomes less, or if it goes below zero
            {
                inputField.text = GetMaxValue();
            }
            else
            {
                inputField.text = result.ToString();
            }
        }
        else  //if the input was blank
        {
            inputField.text = GetMinValue();
        }

        InputValueChanged(inputField.text);
    }


    public bool CheckValidInput(string input, out int? output, bool emptyIsHighlighted = true)  //Check valid input must run AFTER a field's text has been set, so it can format it by adding leading zeros, etc...
    {
        //Debug.Log("Check string " + input);
        int value;
        if (int.TryParse(input, out value))
        {
            //Debug.Log("Parsed int output: " + CurrentValue);
            output = value;
            if ((minValue != -1 && value < minValue) || (maxValue != -1 && value > maxValue))  //if it exceeds minimum or maximum values it is invalid
            {
                Debug.Log("Invalid input! Exceeds maximum or minimum values");
                inputField.image.color = Color.red;
                return false;
            }

            if (input.Count() < inputField.characterLimit)  //add leading zeros before input to fill the field's character limit
            {
                string zeros = new string('0', inputField.characterLimit - input.Count());
                inputField.text = zeros + input;
            }

            inputField.image.color = new Color(255, 255, 255);  //set to default field color in case last input was invalid
            return true;
        }
        else  //invalid input, highlight the field red
        {
            //Debug.Log("Could not parse int");
            output = null;
            if(!emptyIsHighlighted)
            {
                return false; //not valid, but don't color it red
            }
            Debug.Log("Invalid input! Please enter a valid number");
            inputField.image.color = Color.red;
            return false;
        }
    }
    public bool CheckValidInput(string input, bool emptyIsHighlighted = true)
    {
        input = input.Trim();
        //Debug.Log("Checking if " + input + " is valid...");
        if (int.TryParse(input, out CurrentValue))
        {
            //Debug.Log("Parse successful: " + CurrentValue);
            if ((minValue != -1 && CurrentValue < minValue) || (maxValue != -1 && CurrentValue > maxValue))  //if it exceeds minimum or maximum values it is invalid
            {
                //Debug.Log("Invalid input! Exceeds maximum or minimum values");
                inputField.image.color = Color.red;
                return false;
            }

            if (input.Count() < inputField.characterLimit)  //add leading zeros before input to fill the field's character limit
            {
                string zeros = new string('0', inputField.characterLimit - input.Count());
                inputField.text = zeros + input;
            }

            inputField.image.color = new Color(255, 255, 255);  //set to default field color in case last input was invalid
            return true;
        }
        else  //invalid input, highlight the field red
        {
            //Debug.Log("Parse failed!");
            if (!emptyIsHighlighted)
            {
                return false; //not valid, but don't color it red
            }
            //Debug.Log("Invalid input! Please enter a valid number");
            inputField.image.color = Color.red;
            return false;
        }
    }  //same as above, w/o output




    protected string GetMinValue()  //returns the minimum allowed input value, equal to minValue if not -1, or based on the character limit of the input field otherwise
    {
        if(minValue == -1)
        {
            return "0";
            /*if(canBeNegative)
            {
                return ("-" + new string('9', inputField.characterLimit));
            }
            else
            {
                return "0";
            }*/  //at the moment these fields cannot be negative, though it wouldn't be hard to add
        }
        else
        {
            return minValue.ToString();
        }
    }

    protected string GetMaxValue()
    {
        string result = minValue.ToString();
        if (maxValue == -1)
        {
            return (new string('9', inputField.characterLimit));
        }
        else
        {
            return maxValue.ToString();
        }
    }
}
