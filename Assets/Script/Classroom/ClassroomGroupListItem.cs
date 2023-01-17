using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassroomGroupListItem : MonoBehaviour
{
    public Color oddLineColor;
    public Color evenLineColor;
    public Color selectedOddLineColor;
    public Color selectedEvenLineColor;

    Image background;
    Text text;

    Common.CallBack_In_Int buttonClick;
    int index; // A listában hányadik elem?
    public bool selected { get; private set; }

    void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
    }

    public void Initialize(int index, string text, Common.CallBack_In_Int buttonClick, bool selected = false)
    {
        this.index = index;
        this.text.text = text;
        this.buttonClick = buttonClick;
        SetSelected(selected);
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        /*
        if (index % 2 == 0)
        {
            // Páratlan sor
            background.color = (selected) ? selectedOddLineColor : oddLineColor;
        }
        else
        {
            // Páros sor
            background.color = (selected) ? selectedEvenLineColor : evenLineColor;
        }
        */
        background.color = (index %  2 == 0) ?
            (selected) ? selectedOddLineColor : oddLineColor : // Páratlan sor
            (selected) ? selectedEvenLineColor : evenLineColor; // Páros sor
    }

    public void ButtonClick()
    {
        if (buttonClick != null)
            buttonClick(index);
    }
}
