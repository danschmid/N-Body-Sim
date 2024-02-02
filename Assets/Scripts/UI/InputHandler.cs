using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputField inputField;

    [SerializeField] private int minValue=-1;  //if it is equal to -1, there is no enforced min or max value.  Because of this, this input field cannot handle negatives if using the up/down toggles
    [SerializeField] private int maxValue=-1;

    
    public void Start()
    {
        inputField = this.GetComponent<InputField>();
    }

    public void inputValueChanged()
    {
        string text = inputField.text;  //not sure why passing from endEdit action to a method parameter wasn't working, but I can just get the inputField's text this way
        Debug.Log("Text: " + text);
        if (text.Trim()!="")  //if user enters nothing, do nothing
        {
            if (int.TryParse(inputField.text, out _))  //discard result, just check if it is not a number
            {
                inputField.image.color = new Color(255, 255, 255);
                EventManager.events.RaiseInputEvent(text, transform.name);
            }
            else  //invalid input, highlight the field red
            {
                Debug.Log("Invalid input!");
                inputField.image.color = Color.red;
            }
        }
        else
        {
            Debug.Log("empty input");
        }
    }


    //this method only gets used if the InputField has up/down toggle buttons as children to call it
    public void incrementDecrement(bool isNegative)  //check if the field contains a number. If so, either increment or decrement the number by 1, depending on whether up or down button was pressed. Otherwise set as minValue
    {
        int add = 1;
        if(isNegative) { add = -1; }

        inputField.image.color = new Color(255, 255, 255);  //if the last input was invalid, set it to it's normal color

        int input;
        if (int.TryParse(inputField.text, out input))
        {
            int result = (input + add);
            if (maxValue != -1 && result > maxValue)
            {
                inputField.text = minValue.ToString();
                return;
            }
            else if (minValue != -1 && result < minValue)
            {
                inputField.text = maxValue.ToString();
                return;
            }

            inputField.text = result.ToString();
        }
        else
        {
            if(minValue == -1) //if there is no number typed in, just set it to the minimum, or 0 if no minimum set
            {
                inputField.text = "0";
            }
            else
            {
                inputField.text = minValue.ToString();  
            }
        }
    }
}
