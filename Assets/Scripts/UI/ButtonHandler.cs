using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{

    Button button;
    public EventManager Events = EventManager.events;
    // Start is called before the first frame update
    public void Awake()
    {
        Events.LockoutEvent += LockoutButton;
        Events.UnlockEvent += UnlockButton;
        button = GetComponent<Button>();
    }
    public void OnDestroy()
    {
        Events.LockoutEvent -= LockoutButton;
        Events.UnlockEvent -= UnlockButton;
    }

    public void LockoutButton()
    {
         if(button!=null)
        {
            button.interactable = false;
        }
        else
        {
            Debug.LogWarning("No lockable button found");
        }
    }
    public void UnlockButton()
    {
        if (button != null)
        {
            button.interactable = true;
        }
        else
        {
            Debug.LogWarning("No unlockable button found");
        }
    }
}
