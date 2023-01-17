using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCoin : MonoBehaviour
{
    Common.CallBack_In_String buttonClick;
    string buttonName;

    public void Initialize(string buttonName, Common.CallBack_In_String buttonClick)
    {
        this.buttonName = buttonName;
        this.buttonClick = buttonClick;
    }

    public void ButtonClick()
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
