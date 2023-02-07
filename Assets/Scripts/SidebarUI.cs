using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidebarUI : MonoBehaviour
{
    public RectTransform panel;

    void Start()
    {
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;

        panel.sizeDelta = new Vector2(screenWidth / 4, screenHeight);
        panel.anchoredPosition = new Vector2(screenWidth - (panel.sizeDelta.x / 2), 0);
    }
}
