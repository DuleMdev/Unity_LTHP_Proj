using UnityEngine;
using System.Collections;

public class PanelTeacherInfo : MonoBehaviour
{

    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake()
    {

    }

    public void Initialize(Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
